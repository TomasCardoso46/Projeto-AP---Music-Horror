using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f;

    private Bounds objectBounds;

    private void Start()
    {
        CalculateBounds();
    }

    private void Update()
    {
        if (Input.GetMouseButton(1))
        {
            RotateObjectWithMouse();
        }
    }

    private void CalculateBounds()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            objectBounds = new Bounds(transform.position, Vector3.zero);
            return;
        }

        objectBounds = renderers[0].bounds;

        foreach (Renderer renderer in renderers)
        {
            objectBounds.Encapsulate(renderer.bounds);
        }
    }

    private void RotateObjectWithMouse()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Get the visual center of the object in world space
        Vector3 center = objectBounds.center;

        // Horizontal rotation around the visual center
        transform.RotateAround(
            center,
            Vector3.up,
            -mouseX * rotationSpeed
        );

        // Vertical rotation around the visual center
        transform.RotateAround(
            center,
            transform.right,
            mouseY * rotationSpeed
        );

        // Recalculate because the bounds have changed after rotation
        CalculateBounds();
    }
}