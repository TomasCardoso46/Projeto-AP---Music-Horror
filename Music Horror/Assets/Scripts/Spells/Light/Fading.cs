using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Light))]
public class Fading : MonoBehaviour
{
    [Header("UV Light Settings")]
    [SerializeField] private float initialIntensity = 5f;
    [SerializeField] private float decayRate = 1f;

    [Tooltip("Color of the UV flashlight.")]
    [SerializeField] private Color uvColor = new Color(0.35f, 0f, 1f);

    [SerializeField] private float range = 10f;

    [SerializeField] private float spotAngle = 45f;

    [Header("Recharge Settings")]
    [SerializeField] private KeyCode rechargeKey = KeyCode.E;
    [SerializeField] private float rechargeAmount = 1.5f;

    private Light lightComponent;
    private float currentIntensity;

    public Light UVLight => lightComponent;
    public float CurrentIntensity => currentIntensity;

    private void Awake()
    {
        lightComponent = GetComponent<Light>();

        currentIntensity = initialIntensity;

        SetupUVLight();
        AttachToFlashlight();
    }

    private void SetupUVLight()
    {
        lightComponent.type = LightType.Spot;
        lightComponent.color = uvColor;
        lightComponent.intensity = currentIntensity;
        lightComponent.range = range;
        lightComponent.spotAngle = spotAngle;
    }

    private void AttachToFlashlight()
    {
        Flashlight flashlight = FindObjectOfType<Flashlight>();

        if (flashlight != null)
        {
            transform.SetParent(flashlight.transform);

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning("Fading: No Flashlight component found in scene.");
        }
    }

    private void Update()
    {
        HandleDecay();
        HandleRecharge();
    }

    private void HandleDecay()
    {
        if (currentIntensity > 0f)
        {
            currentIntensity -= decayRate * Time.deltaTime;
            currentIntensity = Mathf.Max(currentIntensity, 0f);

            lightComponent.intensity = currentIntensity;

            if (currentIntensity <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void HandleRecharge()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2) || Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            currentIntensity += rechargeAmount;
            currentIntensity = Mathf.Min(currentIntensity, initialIntensity);

            lightComponent.intensity = currentIntensity;
        }
    }
}