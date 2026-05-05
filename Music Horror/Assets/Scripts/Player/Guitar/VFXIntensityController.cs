using UnityEngine;
using UnityEngine.VFX;

public class VFXIntensityController : MonoBehaviour
{
    [Header("Targets (only one active at a time)")]
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private VisualEffect[] vfxGraphs;

    [Header("Property Settings")]
    [SerializeField] private string propertyName = "_Intensity";
    [SerializeField] private bool useMaterial = true;
    [SerializeField] private bool useVFXGraph = false;

    [Header("Behavior")]
    [SerializeField] private float increaseAmount = 0.2f;
    [SerializeField] private float decaySpeed = 1f;
    [SerializeField] private float maxValue = 1f;

    private float currentValue = 0f;

    void Update()
    {
        // Decay over time
        if (currentValue > 0f)
        {
            currentValue -= decaySpeed * Time.deltaTime;
            if (currentValue < 0f)
                currentValue = 0f;

            ApplyValue();
        }
    }

    /// <summary>
    /// Call this whenever a note is played
    /// </summary>
    public void Pulse()
    {
        currentValue += increaseAmount;
        currentValue = Mathf.Clamp(currentValue, 0f, maxValue);

        ApplyValue();
    }

    void ApplyValue()
    {
        if (useMaterial && renderers != null)
        {
            foreach (var rend in renderers)
            {
                if (rend != null && rend.gameObject.activeInHierarchy)
                {
                    foreach (var mat in rend.materials)
                        mat.SetFloat(propertyName, currentValue);
                }
            }
        }

        if (useVFXGraph && vfxGraphs != null)
        {
            foreach (var vfx in vfxGraphs)
            {
                if (vfx != null && vfx.gameObject.activeInHierarchy)
                {
                    vfx.SetFloat(propertyName, currentValue);
                }
            }
        }
    }
}