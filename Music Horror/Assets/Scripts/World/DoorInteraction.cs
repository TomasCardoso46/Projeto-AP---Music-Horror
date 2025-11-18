using UnityEngine;
using TMPro;

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

    private bool isPlayerInRange = false;
    private bool hasUnlocked = false;
    private bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotationA;   // Opens to +openAngle
    private Quaternion openRotationB;   // Opens to -openAngle
    private Quaternion targetRotation;

    private Transform player;

    private void Start()
    {
        // Automatically find the TextMeshProUGUI named "DoorStatus", even if inactive
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

            Vector3 baseEuler = door.localEulerAngles;

            openRotationA = Quaternion.Euler(baseEuler + new Vector3(0, openAngle, 0));
            openRotationB = Quaternion.Euler(baseEuler + new Vector3(0, -openAngle, 0));

            targetRotation = closedRotation;
        }
    }



    private void Update()
    {
        if (!isPlayerInRange)
            return;

        bool allSigilsInactive = AreAllSigilsInactive();
        if (allSigilsInactive)
            hasUnlocked = true;

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

    private void ToggleDoor()
    {
        if (!isOpen)
        {
            // Decide which direction to open based on player position
            targetRotation = DetermineOpenDirection();
            isOpen = true;
        }
        else
        {
            // Close door
            targetRotation = closedRotation;
            isOpen = false;
        }
    }

    private Quaternion DetermineOpenDirection()
    {
        if (player == null)
            return openRotationA; // fallback

        // Direction from the trigger object (this) to the player
        Vector3 playerDirection = (player.position - transform.position).normalized;

        // Compare player position to THIS object's local right axis
        float side = Vector3.Dot(transform.forward, playerDirection);

        // side > 0 → player is on right side → open to the left (negative rotation)
        // side < 0 → player is on left side → open to the right (positive rotation)
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
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            player = other.transform;
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
