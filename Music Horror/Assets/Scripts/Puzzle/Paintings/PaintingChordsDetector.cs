using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class PaintingChordsDetector : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject objectToActivate;
    [SerializeField] private GameObject objectToDeactivate;

    [Header("Keyboard Input")]
    [SerializeField] private KeyCode keyboardChord;

    [Header("Gamepad Input")]
    [SerializeField] private GamepadButton gamepadChord;

    [Header("Queue Settings")]
    [SerializeField] private int queuePosition;
    [SerializeField] private bool isLast;

    private bool playerInside;

    private PaintingChordsManager manager;

    public int QueuePosition => queuePosition;

    public void Initialize(PaintingChordsManager manager)
    {
        this.manager = manager;
    }

    private void Update()
    {
        if (!playerInside)
            return;

        // Keyboard input
        if (Input.GetKeyDown(keyboardChord))
        {
            manager.TryInteract(this);
            return;
        }

        // Gamepad input
        if (Gamepad.current != null && Gamepad.current[gamepadChord].wasPressedThisFrame)
        {
            manager.TryInteract(this);
        }
    }

    public void CorrectInteraction()
    {
        if (objectToActivate != null)
            objectToActivate.SetActive(true);

        if (isLast && objectToDeactivate != null)
            objectToDeactivate.SetActive(false);
    }

    public void ResetPainting()
    {
        if (objectToActivate != null)
            objectToActivate.SetActive(false);

        if (isLast && objectToDeactivate != null)
            objectToDeactivate.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }
}