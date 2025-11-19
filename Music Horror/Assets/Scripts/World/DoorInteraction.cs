using UnityEngine;
using TMPro;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class DoorInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform door;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Transform sigilsParent;
    [SerializeField] private string playerTag = "Player";

    [Header("Door Rotation Settings")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float rotationSpeed = 3f;

    [Header("NavMesh Activation")]
    [SerializeField] private NavMeshSurface navMeshSurface; 
    private NavMeshObstacle navMeshObstacle;   // auto-detected at runtime
    private bool surfaceActivated = false;

    private bool isPlayerInRange = false;
    private bool hasUnlocked = false;
    private bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotationA;
    private Quaternion openRotationB;
    private Quaternion targetRotation;

    private Transform player;

    private void Start()
    {
        // Find DoorStatus TMP if not assigned
        if (promptText == null)
        {
            TextMeshProUGUI[] allTMPs = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            foreach (var tmp in allTMPs)
            {
                if (tmp.name == "DoorStatus")
                {
                    promptText = tmp;
                    break;
                }
            }
        }

        if (promptText != null)
            promptText.gameObject.SetActive(false);

        if (door != null)
        {
            closedRotation = door.localRotation;

            // 🔥 Auto-detect NavMeshObstacle on the door object
            navMeshObstacle = door.GetComponent<NavMeshObstacle>();

            Vector3 baseEuler = door.localEulerAngles;

            openRotationA = Quaternion.Euler(baseEuler + new Vector3(0, openAngle, 0));
            openRotationB = Quaternion.Euler(baseEuler + new Vector3(0, -openAngle, 0));

            targetRotation = closedRotation;
        }

        if (navMeshSurface != null)
        {
            navMeshSurface.enabled = false;
        }

        bool allSigilsInactive = AreAllSigilsInactive();
        if (allSigilsInactive)
        {
            if (!hasUnlocked)
                UnlockDoor();
        }
    }

    private void Update()
    {
        if (!isPlayerInRange)
            return;

        bool allSigilsInactive = AreAllSigilsInactive();
        if (allSigilsInactive)
        {
            if (!hasUnlocked)
                UnlockDoor();
        }

        UpdatePromptText();

        if (hasUnlocked && Input.GetKeyDown(KeyCode.F))
            ToggleDoor();
    }

    private void FixedUpdate()
    {
        if (door != null)
        {
            door.localRotation = Quaternion.Lerp(
                door.localRotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }

    private void UnlockDoor()
    {
        hasUnlocked = true;
        Debug.Log("Door Unlocked!");

        //Disable the NavMeshObstacle automatically
        if (navMeshObstacle != null)
        {
            navMeshObstacle.enabled = false;
            Debug.Log("NavMeshObstacle disabled on door object.");
        }

        // Activate the NavMeshSurface ONCE
        if (!surfaceActivated && navMeshSurface != null)
        {
            navMeshSurface.enabled = true;
            navMeshSurface.BuildNavMesh();

            Debug.Log("NavMeshSurface Activated & Rebuilt!");
            surfaceActivated = true;
        }
    }

    private void ToggleDoor()
    {
        if (!isOpen)
        {
            targetRotation = DetermineOpenDirection();
            isOpen = true;
        }
        else
        {
            targetRotation = closedRotation;
            isOpen = false;
        }
    }

    private Quaternion DetermineOpenDirection()
    {
        if (player == null)
            return openRotationA;

        Vector3 playerDirection = (player.position - transform.position).normalized;
        float side = Vector3.Dot(transform.forward, playerDirection);

        return side > 0 ? openRotationB : openRotationA;
    }

    private void OpenDoorForEnemy(Transform enemy)
    {
        if (!isOpen)
        {
            targetRotation = DetermineOpenDirectionForEnemy(enemy);
            isOpen = true;
        }
    }

    private Quaternion DetermineOpenDirectionForEnemy(Transform enemy)
    {
        Vector3 direction = (enemy.position - transform.position).normalized;
        float side = Vector3.Dot(transform.forward, direction);

        return side > 0 ? openRotationB : openRotationA;
    }

    private void UpdatePromptText()
    {
        if (promptText == null)
            return;

        if (!hasUnlocked)
        {
            promptText.gameObject.SetActive(false);
            return;
        }

        promptText.gameObject.SetActive(true);
        promptText.text = isOpen ? "Press F to close" : "Press F to open";
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{other} entered door ");
        bool allSigilsInactive = AreAllSigilsInactive();
        if (allSigilsInactive && !hasUnlocked)
            UnlockDoor();

        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            player = other.transform;
        }

        if (other.CompareTag("Enemy") && hasUnlocked)
        {
            OpenDoorForEnemy(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;

            if (promptText != null)
                promptText.gameObject.SetActive(false);
        }
    }

    private bool AreAllSigilsInactive()
    {
        if (sigilsParent == null)
            return true;

        foreach (Transform sigil in sigilsParent)
            if (sigil.gameObject.activeSelf)
                return false;

        return true;
    }
}
