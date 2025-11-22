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

    [Header("Audio")]
    [SerializeField] private AudioSource openSoundSource;
    [SerializeField] private AudioClip openSoundClip;

    [Header("NavMesh Activation")]
    [SerializeField] private NavMeshSurface navMeshSurface;
    private NavMeshObstacle navMeshObstacle;

    private bool isPlayerInRange = false;
    private bool hasUnlocked = false;
    private bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotationA;
    private Quaternion openRotationB;
    private Quaternion targetRotation;

    private Transform player;

    private EnemyAudioEmitter enemyAudioEmitter;

    private void Start()
    {
        enemyAudioEmitter = FindObjectOfType<EnemyAudioEmitter>();

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
            navMeshObstacle = door.GetComponent<NavMeshObstacle>();

            Vector3 baseEuler = door.localEulerAngles;

            openRotationA = Quaternion.Euler(baseEuler + new Vector3(0, openAngle, 0));
            openRotationB = Quaternion.Euler(baseEuler + new Vector3(0, -openAngle, 0));

            targetRotation = closedRotation;
        }

        if (navMeshSurface != null)
            navMeshSurface.enabled = false;

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

        if (navMeshObstacle != null)
            navMeshObstacle.enabled = false;
    }

    private void ToggleDoor()
    {
        if (!isOpen)
        {
            targetRotation = DetermineOpenDirection();
            isOpen = true;

            PlayOpenSound();
            EmitNormalSoundForPlayer();
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

            PlayOpenSound();
        }
    }

    private Quaternion DetermineOpenDirectionForEnemy(Transform enemy)
    {
        Vector3 direction = (enemy.position - transform.position).normalized;
        float side = Vector3.Dot(transform.forward, direction);

        return side > 0 ? openRotationB : openRotationA;
    }

    private void PlayOpenSound()
    {
        if (openSoundSource != null && openSoundClip != null)
            openSoundSource.PlayOneShot(openSoundClip);
    }

    private void EmitNormalSoundForPlayer()
    {
        if (enemyAudioEmitter != null && player != null)
            enemyAudioEmitter.EmitSound(EnemyAudioEmitter.SoundLevel.Normal);
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
        bool allSigilsInactive = AreAllSigilsInactive();
        if (allSigilsInactive && !hasUnlocked)
            UnlockDoor();

        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            player = other.transform;
        }

        if (other.CompareTag("Enemy") && hasUnlocked)
            OpenDoorForEnemy(other.transform);
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
