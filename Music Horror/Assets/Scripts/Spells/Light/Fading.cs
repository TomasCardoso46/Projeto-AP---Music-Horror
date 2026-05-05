using UnityEngine;

public class Fading : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private float initialIntensity = 5f;
    [SerializeField] private float decayRate = 1f;

    [Header("Recharge Settings")]
    [SerializeField] private KeyCode rechargeKey = KeyCode.E;
    [SerializeField] private float rechargeAmount = 1.5f;

    private Light lightComponent;
    private float currentIntensity;

    private void Awake()
    {
        lightComponent = GetComponent<Light>();

        currentIntensity = initialIntensity;
        lightComponent.intensity = currentIntensity;

        AttachToFlashlight();
    }

    private void AttachToFlashlight()
    {
        Flashlight flashlight = FindObjectOfType<Flashlight>();

        if (flashlight != null)
        {
            transform.SetParent(flashlight.transform);
            transform.localPosition = Vector3.zero;
        }
        else
        {
            Debug.LogWarning("FadingFollowLight: No Flashlight component found in scene.");
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
        if (Input.GetKeyDown(rechargeKey))
        {
            currentIntensity += rechargeAmount;
            currentIntensity = Mathf.Min(currentIntensity, initialIntensity);

            lightComponent.intensity = currentIntensity;
        }
    }
}