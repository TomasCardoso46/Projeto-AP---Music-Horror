using UnityEngine;

public class FirstPersonRigidbodyController : MonoBehaviour
{
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] float sprintSpeed = 8f;
    [SerializeField] float mouseSensitivity = 100f;
    [SerializeField] float leanAngle = 15f;
    [SerializeField] float leanSpeed = 8f;
    [SerializeField] Transform cameraTransform;
    [SerializeField] float cameraFollowSpeed = 10f;
    [SerializeField] float cameraHeight = 1.7f;
    [SerializeField] float leanOffsetAmount = 0.3f;
    [SerializeField] float stepDistance = 2f;
    [SerializeField] float walkShakeAmount = 0.05f;
    [SerializeField] float walkShakeSpeed = 10f;
    [SerializeField] float sprintShakeAmount = 0.1f;
    [SerializeField] float sprintShakeSpeed = 15f;

    [SerializeField] Rigidbody rb;
    [SerializeField] float pitch;
    [SerializeField] float targetLean;
    [SerializeField] float currentLean;
    [SerializeField] float shakeTime;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMouseLook();
        HandleLean();
        HandleStepShake();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;

        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float speed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 moveDir = transform.TransformDirection(input) * speed;
        Vector3 velocity = new Vector3(moveDir.x, rb.linearVelocity.y, moveDir.z);
        rb.linearVelocity = velocity;
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        Quaternion bodyRotation = transform.rotation * Quaternion.Euler(0f, mouseX, 0f);
        transform.rotation = bodyRotation;
    }

    void HandleLean()
    {
        if (Input.GetKey(KeyCode.Q)) targetLean = leanAngle;
        else if (Input.GetKey(KeyCode.E)) targetLean = -leanAngle;
        else targetLean = 0f;

        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);
        transform.localRotation = Quaternion.Euler(0f, transform.localEulerAngles.y, currentLean);
    }

    void HandleStepShake()
    {
        bool isWalking = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        if (isWalking)
        {
            shakeTime = 1f;
        }
        else
        {
            shakeTime = Mathf.Lerp(shakeTime, 0f, Time.deltaTime * walkShakeSpeed);
        }
    }

    void LateUpdate()
    {
        if (cameraTransform)
        {
            Vector3 targetPos = transform.position + Vector3.up * cameraHeight;
            Vector3 leanOffset = transform.right * (currentLean / leanAngle) * leanOffsetAmount;
            targetPos += leanOffset;

            bool isSprinting = Input.GetKey(KeyCode.LeftShift) &&
                               (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0);
            float currentShakeAmount = isSprinting ? sprintShakeAmount : walkShakeAmount;
            float currentShakeSpeed = isSprinting ? sprintShakeSpeed : walkShakeSpeed;

            float shakeOffset = Mathf.Sin(Time.time * currentShakeSpeed) * currentShakeAmount * shakeTime;
            targetPos += Vector3.up * shakeOffset;

            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPos, Time.deltaTime * cameraFollowSpeed);

            Quaternion targetRot = Quaternion.Euler(pitch, transform.eulerAngles.y, currentLean);
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetRot, Time.deltaTime * cameraFollowSpeed);
        }
    }
}
