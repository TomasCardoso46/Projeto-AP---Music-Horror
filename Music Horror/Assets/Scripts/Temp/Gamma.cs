using UnityEngine;

public class Gamma : MonoBehaviour
{
    [SerializeField] private float gammaStep = 0.1f;
    [SerializeField] private float minGamma = 0.1f;
    [SerializeField] private float maxGamma = 3f;

    private float currentGamma = 1f;
    private float originalGamma;

    void Start()
    {
        originalGamma = currentGamma;
        Shader.SetGlobalFloat("_GlobalGamma", currentGamma);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            IncreaseGamma();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            DecreaseGamma();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            ResetGamma();
        }
    }

    void IncreaseGamma()
    {
        currentGamma = Mathf.Clamp(currentGamma + gammaStep, minGamma, maxGamma);
        ApplyGamma();
    }

    void DecreaseGamma()
    {
        currentGamma = Mathf.Clamp(currentGamma - gammaStep, minGamma, maxGamma);
        ApplyGamma();
    }

    void ResetGamma()
    {
        currentGamma = originalGamma;
        ApplyGamma();
    }

    void ApplyGamma()
    {
        Shader.SetGlobalFloat("_GlobalGamma", currentGamma);
    }
}