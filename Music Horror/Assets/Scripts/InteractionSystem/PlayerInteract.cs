using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactLayer;

    [Header("UI")]
    [SerializeField] private CrosshairController crosshair;

    private Camera cam;
    private IInteractable currentInteractable;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        DetectInteractable();

        if (Input.GetKeyDown(KeyCode.F) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }

        crosshair.SetInteractState(currentInteractable != null);
    }

    private void DetectInteractable()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        currentInteractable = null;

        if (Physics.Raycast(ray, out hit, interactRange, interactLayer))
        {
            currentInteractable = hit.collider.GetComponent<IInteractable>();
        }
    }
}