using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private float gamepadLookSensitivity = 150f;

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

    [Header("Smooth Camera Rotation")]
    [SerializeField] float smoothRotationDuration = 0.5f;

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

    bool inputLocked;

    bool smoothRotationActive;
    float smoothRotationTimer;
    float smoothRotationStartPitch;
    float smoothRotationTargetPitch;
    private bool gamepadCrouchState = false;
    private bool gamepadSprintState = false;

    void Awake()
    {
        if (!rb)
            rb = GetComponent<Rigidbody>();

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

        if (!inputLocked)
        {
            ReadInput();
            HandleMouseLook();
            HandleLean();
            HandleCrouch();
        }

        HandleStepShake();
    }

    void LateUpdate()
    {
        if (GameState.IsPaused || isLoading)
            return;

        UpdateCamera();

        if (!inputLocked)
            HandleMovement();
        else
            StopMovement();
    }

   void ReadInput()
    {
        movementInput = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0f,
            Input.GetAxisRaw("Vertical")
        ).normalized;

        if (Gamepad.current != null &&
            Gamepad.current.rightStickButton.wasPressedThisFrame)
        {
            gamepadCrouchState = !gamepadCrouchState;

            if (gamepadCrouchState)
                gamepadSprintState = false;
        }

        if (SettingsManager.Instance != null &&
            SettingsManager.Instance.crouchToggleMode)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
                crouchState = !crouchState;

            isCrouching = crouchState || gamepadCrouchState;
        }
        else
        {
            isCrouching = Input.GetKey(KeyCode.LeftControl) || gamepadCrouchState;
        }

        if (Gamepad.current != null &&
            Gamepad.current.leftStickButton.wasPressedThisFrame)
        {
            gamepadSprintState = !gamepadSprintState;

            if (gamepadSprintState)
                gamepadCrouchState = false;
        }

        if (movementInput.sqrMagnitude <= 0.01f)
            gamepadSprintState = false;

        isSprinting =
            (Input.GetKey(KeyCode.LeftShift) && !isCrouching) ||
            (gamepadSprintState && !isCrouching);
    }

    void HandleMovement()
    {
        float speed = walkSpeed;

        if (isSprinting)
            speed = sprintSpeed;
        else if (isCrouching)
            speed = crouchSpeed;

        Vector3 moveDir =
            transform.TransformDirection(movementInput) * speed;

        Vector3 velocity = new Vector3(
            moveDir.x,
            rb.linearVelocity.y,
            moveDir.z
        );

        rb.linearVelocity = velocity;
    }

    void StopMovement()
    {
        rb.linearVelocity = new Vector3(
            0f,
            rb.linearVelocity.y,
            0f
        );
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        float joystickX = 0f;
        float joystickY = 0f;

        if (Gamepad.current != null)
        {
            joystickX = Gamepad.current.rightStick.x.ReadValue();
            joystickY = Gamepad.current.rightStick.y.ReadValue();
        }

        yaw += mouseX;
        pitch -= mouseY;

        yaw += joystickX * gamepadLookSensitivity * Time.deltaTime;
        pitch -= joystickY * gamepadLookSensitivity * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    void HandleLean()
    {
        if (Input.GetKey(KeyCode.Q) || Gamepad.current.leftTrigger.isPressed)
            targetLean = leanAngle;
        else if (Input.GetKey(KeyCode.E) || Gamepad.current.rightTrigger.isPressed)
            targetLean = -leanAngle;
        else
            targetLean = 0f;

        currentLean = Mathf.Lerp(
            currentLean,
            targetLean,
            Time.deltaTime * leanSpeed
        );
    }

    void HandleCrouch()
    {
        Vector3 scale = transform.localScale;

        float target =
            isCrouching ? crouchHeight : normalHeight;

        scale.y = Mathf.Lerp(
            scale.y,
            target,
            Time.deltaTime * 10f
        );

        transform.localScale = scale;
    }

    void HandleStepShake()
    {
        if (inputLocked)
        {
            shakeTime = Mathf.Lerp(
                shakeTime,
                0f,
                Time.deltaTime * 10f
            );

            return;
        }

        bool isMoving =
            movementInput.x != 0 ||
            movementInput.z != 0;

        if (isMoving)
            shakeTime = 1f;
        else
            shakeTime = Mathf.Lerp(
                shakeTime,
                0f,
                Time.deltaTime * 5f
            );
    }

    void UpdateCamera()
    {
        if (!cameraTransform)
            return;

        float targetHeight =
            isCrouching
                ? crouchCameraHeight
                : cameraHeight;

        Vector3 targetPos =
            transform.position +
            Vector3.up * targetHeight;

        Vector3 leanOffset =
            transform.right *
            (currentLean / leanAngle) *
            leanOffsetAmount;

        targetPos += leanOffset;

        float shakeAmount =
            isCrouching
                ? crouchShakeAmount
                : walkShakeAmount;

        float shakeSpeed =
            isCrouching
                ? crouchShakeSpeed
                : walkShakeSpeed;

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

            cameraTransform.rotation =
                Quaternion.Euler(
                    pitch,
                    yaw,
                    currentLean
                );

            cameraYaw = yaw;

            return;
        }

        cameraTransform.position =
            Vector3.SmoothDamp(
                cameraTransform.position,
                targetPos,
                ref cameraVelocity,
                1f / cameraFollowSpeed
            );

        cameraYaw =
            Mathf.SmoothDampAngle(
                cameraYaw,
                yaw,
                ref yawVelocity,
                0.05f
            );

        float targetPitch;

        if (smoothRotationActive)
        {
            smoothRotationTimer += Time.deltaTime;

            float t =
                smoothRotationTimer /
                smoothRotationDuration;

            t = Mathf.Clamp01(t);

            t = t * t * (3f - 2f * t);

            targetPitch =
                Mathf.Lerp(
                    smoothRotationStartPitch,
                    smoothRotationTargetPitch,
                    t
                );

            if (smoothRotationTimer >= smoothRotationDuration)
            {
                smoothRotationActive = false;

                pitch = smoothRotationTargetPitch;

                targetPitch = smoothRotationTargetPitch;

                pitchVelocity = 0f;

                inputLocked = true;
            }
        }
        else
        {
            targetPitch = pitch;
        }

        float smoothPitch =
            Mathf.SmoothDampAngle(
                cameraTransform.eulerAngles.x,
                targetPitch,
                ref pitchVelocity,
                0.05f
            );

        cameraTransform.rotation =
            Quaternion.Euler(
                smoothPitch,
                cameraYaw,
                currentLean
            );
    }

    public void SmoothResetCamera()
    {
        smoothRotationStartPitch = pitch;
        smoothRotationTargetPitch = 0f;

        smoothRotationTimer = 0f;
        smoothRotationActive = true;

        pitchVelocity = 0f;
        inputLocked = true;
    }

    public void LockPlayer()
    {
        inputLocked = true;
    }

    public void UnlockPlayer()
    {
        inputLocked = false;
    }

    public void HardResetCameraMotion()
    {
        cameraVelocity = Vector3.zero;
        yawVelocity = 0f;
        pitchVelocity = 0f;
    }

    public float GetCameraHeight()
    {
        return isCrouching
            ? crouchCameraHeight
            : cameraHeight;
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

        smoothRotationActive = false;
        inputLocked = false;
    }
}