using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactLayer;

    [Header("References")]
    [SerializeField] private CrosshairController crosshair;

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        bool canInteract = false;

        if (Physics.Raycast(ray, out hit, interactRange, interactLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                canInteract = true;
            }
        }

        crosshair.SetInteractState(canInteract);
    }
}