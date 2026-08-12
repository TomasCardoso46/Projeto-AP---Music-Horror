using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] private GameObject objectToMove;
    [SerializeField] private float rotationSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            RotateObjectWithMouse();
        }
    }

    private void RotateObjectWithMouse()
    {
        // Get the mouse movement
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Rotate the object based on mouse movement
        objectToMove.transform.Rotate(Vector3.up, mouseX * -rotationSpeed, Space.World);
        objectToMove.transform.Rotate(Vector3.left, mouseY * rotationSpeed, Space.World);
    }
}
