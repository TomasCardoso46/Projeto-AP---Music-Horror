using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; 

public class ChangePostExposure : MonoBehaviour
{
    public Volume volume;
    private ColorAdjustments colorAdjustments;

    public float changeAmount = 0.1f;

    private float originalExposure;

    private void Start()
    {
        if (volume != null && volume.profile.TryGet(out colorAdjustments))
        {
            originalExposure = colorAdjustments.postExposure.value;
        }
        else
        {
            Debug.LogError("ColorAdjustments not found in Volume profile!");
        }
    }

    private void Update()
    {
        if (colorAdjustments == null)
            return;

        // Increase exposure with P
        if (Input.GetKeyDown(KeyCode.P))
        {
            colorAdjustments.postExposure.value += changeAmount;
        }

        // Decrease exposure with O
        if (Input.GetKeyDown(KeyCode.O))
        {
            colorAdjustments.postExposure.value -= changeAmount;
        }

        // Reset exposure with I
        if (Input.GetKeyDown(KeyCode.I))
        {
            colorAdjustments.postExposure.value = originalExposure;
        }
    }
}