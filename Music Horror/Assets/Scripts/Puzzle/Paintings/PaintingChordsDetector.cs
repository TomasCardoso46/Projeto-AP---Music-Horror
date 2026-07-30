using UnityEngine;

public class PaintingChordsDetector : MonoBehaviour
{
    [SerializeField] private GameObject objectToActivate;
    [SerializeField] private GameObject objectToDeactivate;
    [SerializeField] private KeyCode chord;
    [SerializeField] private int queuePosition;
    [SerializeField] private bool isLast;

    private bool playerInside;

    private PaintingChordsManager manager;

    public int QueuePosition => queuePosition;

    public void Initialize(PaintingChordsManager manager)
    {
        this.manager = manager;
    }

    void Update()
    {
        if (!playerInside)
            return;

        if (Input.GetKeyDown(chord))
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