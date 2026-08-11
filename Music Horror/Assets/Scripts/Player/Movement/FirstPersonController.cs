using UnityEngine;

public class FirstPersonRigidbodyController : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] float sprintSpeed = 8f;
    [SerializeField] float crouchSpeed = 2.5f;

    [Header("Mouse")]
    [SerializeField] float mouseSensitivity = 100f;

    [Header("Lean")]
    [SerializeField] float leanAngle = 15f;
    [SerializeField] float leanSpeed = 8f;
    [SerializeField] float leanOffsetAmount = 0.3f;

    [Header("Camera")]
    [SerializeField] public Transform cameraTransform;
    [SerializeField] float cameraFollowSpeed = 15f;
    [SerializeField] float cameraHeight = 1.7f;
    [SerializeField] float crouchCameraHeight = 0.8f;

    [Header("Head Bob")]
    [SerializeField] float walkShakeAmount = 0.05f;
    [SerializeField] float walkShakeSpeed = 10f;
    [SerializeField] float sprintShakeAmount = 0.1f;
    [SerializeField] float sprintShakeSpeed = 15f;
    [SerializeField] float crouchShakeAmount = 0.02f;
    [SerializeField] float crouchShakeSpeed = 5f;

    [Header("Crouch")]
    [SerializeField] float crouchHeight = 1f;

    [Header("References")]
    [SerializeField] Rigidbody rb;

    public bool isLoading;
    public bool freezeCamera;

    float yaw;
    float pitch;

    float cameraYaw;
    float yawVelocity;
    float pitchVelocity;

    float targetLean;
    float currentLean;

    float shakeTime;

    float normalHeight;

    Vector3 cameraVelocity;
    Vector3 movementInput;

    bool isCrouching;
    bool isSprinting;
    bool crouchState;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        Cursor.lockState = CursorLockMode.Locked;

        normalHeight = transform.localScale.y;

        yaw = transform.eulerAngles.y;
        cameraYaw = yaw;

        if (cameraTransform != null)
            cameraTransform.parent = null;
    }

    void Update()
    {
        if (GameState.IsPaused || isLoading)
            return;

        ReadInput();
        HandleMouseLook();
        HandleLean();
        HandleCrouch();
        HandleStepShake();
    }

    void LateUpdate()
    {
        if (GameState.IsPaused || isLoading)
            return;

        UpdateCamera();
        HandleMovement();
    }

    void ReadInput()
    {
        movementInput = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0f,
            Input.GetAxisRaw("Vertical")
        ).normalized;

        if (SettingsManager.Instance != null &&
            SettingsManager.Instance.crouchToggleMode)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
                crouchState = !crouchState;

            isCrouching = crouchState;
        }
        else
        {
            isCrouching = Input.GetKey(KeyCode.LeftControl);
        }

        isSprinting = Input.GetKey(KeyCode.LeftShift) && !isCrouching;
    }

    void HandleMovement()
    {
        float speed = walkSpeed;

        if (isSprinting) speed = sprintSpeed;
        else if (isCrouching) speed = crouchSpeed;

        Vector3 moveDir = transform.TransformDirection(movementInput) * speed;

        Vector3 velocity = new Vector3(
            moveDir.x,
            rb.linearVelocity.y,
            moveDir.z
        );

        rb.linearVelocity = velocity;
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    void HandleLean()
    {
        if (Input.GetKey(KeyCode.Q))
            targetLean = leanAngle;
        else if (Input.GetKey(KeyCode.E))
            targetLean = -leanAngle;
        else
            targetLean = 0f;

        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);
    }

    void HandleCrouch()
    {
        Vector3 scale = transform.localScale;

        float target = isCrouching ? crouchHeight : normalHeight;

        scale.y = Mathf.Lerp(scale.y, target, Time.deltaTime * 10f);

        transform.localScale = scale;
    }

    void HandleStepShake()
    {
        bool isMoving = movementInput.x != 0 || movementInput.z != 0;

        if (isMoving)
            shakeTime = 1f;
        else
            shakeTime = Mathf.Lerp(shakeTime, 0f, Time.deltaTime * 5f);
    }

    void UpdateCamera()
    {
        if (!cameraTransform) return;

        float targetHeight = isCrouching ? crouchCameraHeight : cameraHeight;

        Vector3 targetPos = transform.position + Vector3.up * targetHeight;

        Vector3 leanOffset = transform.right * (currentLean / leanAngle) * leanOffsetAmount;
        targetPos += leanOffset;

        float shakeAmount = isCrouching ? crouchShakeAmount : walkShakeAmount;
        float shakeSpeed = isCrouching ? crouchShakeSpeed : walkShakeSpeed;

        if (isSprinting)
        {
            shakeAmount = sprintShakeAmount;
            shakeSpeed = sprintShakeSpeed;
        }

        float shakeOffset =
            Mathf.Sin(Time.time * shakeSpeed) *
            shakeAmount *
            shakeTime;

        targetPos += Vector3.up * shakeOffset;

        if (freezeCamera)
        {
            cameraVelocity = Vector3.zero;
            yawVelocity = 0f;
            pitchVelocity = 0f;

            cameraTransform.position = targetPos;
            cameraTransform.rotation = Quaternion.Euler(pitch, yaw, currentLean);

            cameraYaw = yaw;
            return;
        }

        cameraTransform.position = Vector3.SmoothDamp(
            cameraTransform.position,
            targetPos,
            ref cameraVelocity,
            1f / cameraFollowSpeed
        );

        cameraYaw = Mathf.SmoothDampAngle(
            cameraYaw,
            yaw,
            ref yawVelocity,
            0.05f
        );

        float smoothPitch = Mathf.SmoothDampAngle(
            cameraTransform.eulerAngles.x,
            pitch,
            ref pitchVelocity,
            0.05f
        );

        cameraTransform.rotation = Quaternion.Euler(
            smoothPitch,
            cameraYaw,
            currentLean
        );
    }

    public void HardResetCameraMotion()
    {
        cameraVelocity = Vector3.zero;
        yawVelocity = 0f;
        pitchVelocity = 0f;
    }

    public float GetCameraHeight()
    {
        return isCrouching ? crouchCameraHeight : cameraHeight;
    }

    public void ResetAfterLoad()
    {
        yaw = transform.eulerAngles.y;
        pitch = 0f;

        cameraYaw = yaw;

        yawVelocity = 0f;
        pitchVelocity = 0f;

        cameraVelocity = Vector3.zero;
        shakeTime = 0f;
    }
}