using UnityEngine;
using TMPro;

public class DoorInteraction : MonoBehaviour
{
    [Header("References")]
    public Animator doorAnimator;
    public TextMeshProUGUI promptText;
    public Transform sigilsParent; // The "Sigils" object (parent of all lock sigils)

    [Header("Settings")]
    public string animationTrigger = "Open";
    public string playerTag = "Player";

    private bool isPlayerInRange = false;
    private bool hasOpened = false;

    void Start()
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        // Only interact if player is inside trigger and door hasn't opened
        if (isPlayerInRange && !hasOpened)
        {
            bool allSigilsInactive = AreAllSigilsInactive();

            // Show or hide the prompt based on sigils
            if (promptText != null)
                promptText.gameObject.SetActive(allSigilsInactive);

            // Open the door if player presses F and all sigils are inactive
            if (allSigilsInactive && Input.GetKeyDown(KeyCode.F))
            {
                doorAnimator.SetTrigger(animationTrigger);
                hasOpened = true;

                if (promptText != null)
                    promptText.gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
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
            return true; // No sigils = door unlocked

        foreach (Transform sigil in sigilsParent)
        {
            if (sigil.gameObject.activeSelf)
                return false; // At least one sigil is still active
        }
        return true; // All inactive → unlocked
    }
}
