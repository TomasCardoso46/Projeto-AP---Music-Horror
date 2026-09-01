using UnityEngine;

public class Gamma : MonoBehaviour
{
    public static Gamma Instance;

    [Header("Gamma Settings")]
    [SerializeField] private float minGamma = 0.5f;
    [SerializeField] private float maxGamma = 2.5f;
    [SerializeField] private float defaultGamma = 1f;

    public float CurrentGamma { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentGamma = defaultGamma;
        ApplyGamma();
    }

    public void SetGamma(float value)
    {
        CurrentGamma = Mathf.Clamp(value, minGamma, maxGamma);

        ApplyGamma();
    }

    public void ResetGamma()
    {
        CurrentGamma = defaultGamma;

        ApplyGamma();
    }

    void ApplyGamma()
    {
        Shader.SetGlobalFloat("_GlobalGamma", CurrentGamma);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Shader.SetGlobalFloat("_GlobalGamma", 1f);
            Instance = null;
        }
    }
}