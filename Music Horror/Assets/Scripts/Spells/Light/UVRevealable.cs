using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class UVRevealable : MonoBehaviour
{
    [Header("Reveal Settings")]
    [SerializeField] private Color revealColor = Color.white;
    [SerializeField] private float revealIntensity = 3f;
    [SerializeField, Range(0.01f, 1f)] private float edgeSoftness = 0.2f;
    [SerializeField] private float revealMultiplier = 1f;

    [Header("UV Flashlight")]
    [Tooltip("Optional. Leave empty to automatically find the currently active Fading light.")]
    [SerializeField] private Fading uvFlashlight;

    private Renderer objectRenderer;
    private MaterialPropertyBlock propertyBlock;

    private static readonly int RevealColorID =
        Shader.PropertyToID("_UVRevealColor");

    private static readonly int RevealIntensityID =
        Shader.PropertyToID("_UVRevealIntensity");

    private static readonly int RevealMultiplierID =
        Shader.PropertyToID("_UVRevealMultiplier");

    private static readonly int RevealStrengthID =
        Shader.PropertyToID("_UVRevealStrength");

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();

        ApplyProperties();
    }

    private void Update()
    {
        // If we don't currently have a flashlight reference,
        // look for an active Fading component.
        if (uvFlashlight == null)
        {
            uvFlashlight = FindFirstObjectByType<Fading>();
        }

        // No UV flashlight currently exists.
        if (uvFlashlight == null)
        {
            SetRevealStrength(0f);
            return;
        }

        Light uvLight = uvFlashlight.UVLight;

        // Fading object exists, but its Light is unavailable.
        if (uvLight == null)
        {
            SetRevealStrength(0f);
            return;
        }

        float revealStrength = CalculateRevealStrength(uvLight);

        SetRevealStrength(revealStrength);
    }

    private float CalculateRevealStrength(Light uvLight)
    {
        Vector3 lightPosition = uvLight.transform.position;

        Vector3 toObject = transform.position - lightPosition;

        float distance = toObject.magnitude;

        // Outside the light's range.
        if (distance > uvLight.range)
        {
            return 0f;
        }

        Vector3 directionToObject = toObject.normalized;

        // Angle between the flashlight direction
        // and the object.
        float angle = Vector3.Angle(
            uvLight.transform.forward,
            directionToObject
        );

        float halfAngle = uvLight.spotAngle * 0.5f;

        // Outside the spotlight cone.
        if (angle > halfAngle)
        {
            return 0f;
        }

        // Distance falloff.
        float distanceFactor =
            1f - Mathf.Clamp01(distance / uvLight.range);

        // Soft edge around the spotlight.
        float edgeStart =
            halfAngle * (1f - edgeSoftness);

        float angleFactor = Mathf.InverseLerp(
            halfAngle,
            edgeStart,
            angle
        );

        // Account for the battery/intensity.
        float intensityFactor =
            Mathf.Clamp01(uvLight.intensity / 5f);

        float reveal =
            distanceFactor *
            angleFactor *
            intensityFactor *
            revealMultiplier;

        return Mathf.Clamp01(reveal);
    }

    private void ApplyProperties()
    {
        objectRenderer.GetPropertyBlock(propertyBlock);

        propertyBlock.SetColor(
            RevealColorID,
            revealColor
        );

        propertyBlock.SetFloat(
            RevealIntensityID,
            revealIntensity
        );

        propertyBlock.SetFloat(
            RevealMultiplierID,
            revealMultiplier
        );

        propertyBlock.SetFloat(
            RevealStrengthID,
            0f
        );

        objectRenderer.SetPropertyBlock(propertyBlock);
    }

    private void SetRevealStrength(float strength)
    {
        objectRenderer.GetPropertyBlock(propertyBlock);

        propertyBlock.SetFloat(
            RevealStrengthID,
            strength
        );

        objectRenderer.SetPropertyBlock(propertyBlock);
    }

    private void OnValidate()
    {
        if (objectRenderer == null)
            objectRenderer = GetComponent<Renderer>();

        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        if (objectRenderer != null)
            ApplyProperties();
    }
}