
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Defaults")]
    [SerializeField] float defaultVolume = 1f;
    [SerializeField] float defaultGamma = 1f;
    [SerializeField] bool defaultCrouchToggle = false;

    [HideInInspector] public float volume;
    [HideInInspector] public float gamma;
    [HideInInspector] public bool crouchToggleMode;

    const string VolumeKey = "Volume";
    const string GammaKey = "Gamma";
    const string CrouchKey = "CrouchToggle";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyGammaToAllVolumes();
    }

    public void SetVolume(float value)
    {
        volume = Mathf.Clamp01(value);

        AudioListener.volume = volume;

        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
    }

    public void SetGamma(float value)
    {
        gamma = value;

        ApplyGammaToAllVolumes();

        PlayerPrefs.SetFloat(GammaKey, gamma);
        PlayerPrefs.Save();
    }

    public void ApplyGammaToAllVolumes()
    {
        Volume[] volumes = FindObjectsByType<Volume>(FindObjectsSortMode.None);

        float exposure = Mathf.Clamp(gamma, 0.1f, 3f);

        foreach (var v in volumes)
        {
            if (v == null || v.profile == null)
                continue;

            if (v.profile.TryGet(out ColorAdjustments colorAdjustments))
            {
                colorAdjustments.postExposure.overrideState = true;
                colorAdjustments.postExposure.value = exposure;
            }
        }
    }

    public void SetCrouchMode(bool value)
    {
        crouchToggleMode = value;

        PlayerPrefs.SetInt(CrouchKey, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    void LoadSettings()
    {
        volume = PlayerPrefs.GetFloat(VolumeKey, defaultVolume);
        gamma = PlayerPrefs.GetFloat(GammaKey, defaultGamma);
        crouchToggleMode = PlayerPrefs.GetInt(CrouchKey, defaultCrouchToggle ? 1 : 0) == 1;

        AudioListener.volume = volume;
        ApplyGammaToAllVolumes();
    }

    public void ResetSettings()
    {
        SetVolume(defaultVolume);
        SetGamma(defaultGamma);
        SetCrouchMode(defaultCrouchToggle);
    }

    public void EnterSettings()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitSettings()
    {
        
    }
}

