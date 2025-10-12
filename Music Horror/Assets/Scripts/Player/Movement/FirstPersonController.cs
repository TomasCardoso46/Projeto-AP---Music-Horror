using UnityEngine;

public class FirstPersonRigidbodyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] float sprintSpeed = 8f;
    [SerializeField] float crouchSpeed = 2.5f;

    [Header("Camera")]
    [SerializeField] Transform cameraTransform;
    [SerializeField] float mouseSensitivity = 100f;
    [SerializeField] float cameraHeight = 1.7f;

    [Header("Lean")]
    [SerializeField] float leanAngle = 15f;
    [SerializeField] float leanSpeed = 8f;
    [SerializeField] float leanOffsetAmount = 0.3f;

    [Header("Crouch")]
    [SerializeField] float crouchHeight = 1f;

    [Header("References")]
    [SerializeField] Rigidbody rb;

    float pitch;
    float targetLean;
    float currentLean;
    float normalHeight;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        normalHeight = transform.localScale.y;
    }

    void Update()
    {
        HandleMouseLook();
        HandleLean();
        HandleCrouch();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        // Only move if pressing keys
        if (inputX == 0 && inputZ == 0)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        bool isCrouching = Input.GetKey(KeyCode.LeftControl);
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && !isCrouching;

        float speed = walkSpeed;
        if (isSprinting) speed = sprintSpeed;
        if (isCrouching) speed = crouchSpeed;

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 moveDir = (right * inputX + forward * inputZ).normalized * speed;
        rb.linearVelocity = new Vector3(moveDir.x, rb.linearVelocity.y, moveDir.z);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);
    }

    void HandleLean()
    {
        if (Input.GetKey(KeyCode.Q)) targetLean = leanAngle;
        else if (Input.GetKey(KeyCode.E)) targetLean = -leanAngle;
        else targetLean = 0f;

        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);
    }

    void HandleCrouch()
    {
        float targetHeight = Input.GetKey(KeyCode.LeftControl) ? crouchHeight : normalHeight;
        Vector3 scale = transform.localScale;
        scale.y = Mathf.Lerp(scale.y, targetHeight, Time.deltaTime * 10f);
        transform.localScale = scale;
    }

    void LateUpdate()
    {
        if (!cameraTransform) return;

        Vector3 targetPos = transform.position + Vector3.up * cameraHeight;

        // Lean offset
        Vector3 leanOffset = transform.right * (currentLean / leanAngle) * leanOffsetAmount;
        targetPos += leanOffset;

        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPos, Time.deltaTime * 10f);

        Quaternion targetRot = Quaternion.Euler(pitch, transform.eulerAngles.y, currentLean);
        cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetRot, Time.deltaTime * 10f);
    }
}
