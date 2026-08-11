using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ChangePostExposure : MonoBehaviour
{
    public Volume volume;

    private ColorAdjustments colorAdjustments;
    private Bloom bloom;

    [Header("Exposure Settings")]
    public float changeAmount = 0.1f;

    [Header("Bloom Compensation")]
    [Tooltip("Controls how strongly bloom is compensated when exposure changes.")]
    public float bloomCompensation = 1.0f;

    private float originalExposure;
    private float originalBloom;

    private void Start()
    {
        if (volume != null &&
            volume.profile.TryGet(out colorAdjustments) &&
            volume.profile.TryGet(out bloom))
        {


            originalExposure = colorAdjustments.postExposure.value;
            originalBloom = bloom.intensity.value;
        }
        else
        {
            Debug.LogError(
                "ColorAdjustments or Bloom not found in the Volume profile!"
            );
        }
    }

    private void Update()
    {
        if (colorAdjustments == null || bloom == null)
            return;


        if (Input.GetKeyDown(KeyCode.P))
        {
            colorAdjustments.postExposure.value += changeAmount;

            UpdateBloom();
        }


        if (Input.GetKeyDown(KeyCode.O))
        {
            colorAdjustments.postExposure.value -= changeAmount;

            UpdateBloom();
        }


        if (Input.GetKeyDown(KeyCode.I))
        {
            colorAdjustments.postExposure.value = originalExposure;
            bloom.intensity.value = originalBloom;
        }
    }

    private void UpdateBloom()
    {

        float exposureDifference =
            colorAdjustments.postExposure.value - originalExposure;


        float compensatedBloom =
            originalBloom *
            Mathf.Pow(2f, -exposureDifference * bloomCompensation);


        bloom.intensity.value = Mathf.Max(0f, compensatedBloom);
    }
}