using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("UI Object Groups")]
    [SerializeField] private GameObject[] disableOnOpen;
    [SerializeField] private GameObject[] enableOnOpen;

    [Header("Settings")]
    public float volume = 1f;
    public float gamma = 1f;
    public bool crouchToggleMode = true;

    private const string VOLUME_KEY = "VOLUME";
    private const string GAMMA_KEY = "GAMMA";
    private const string CROUCH_KEY = "CROUCH_MODE";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        AudioListener.volume = volume;
        ApplyGammaToAllVolumes();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyGammaToAllVolumes();
    }

    public void SetVolume(float v)
    {
        volume = v;
        AudioListener.volume = volume;
        Save();
    }

    public void SetGamma(float g)
    {
        gamma = g;
        ApplyGammaToAllVolumes();
        Save();
    }

    public void SetCrouchMode(bool toggle)
    {
        crouchToggleMode = toggle;
        Save();
    }

    public void ResetSettings()
    {
        volume = 1f;
        gamma = 1f;
        crouchToggleMode = true;

        AudioListener.volume = volume;
        ApplyGammaToAllVolumes();

        Save();
    }

    /// <summary>
    /// Applies gamma to ALL volumes in the current scene
    /// </summary>
    private void ApplyGammaToAllVolumes()
    {
        Volume[] volumes = FindObjectsByType<Volume>(FindObjectsSortMode.None);

        float g = Mathf.Clamp(gamma, 0.1f, 3f);
        float colorMultiplier = g; // direct mapping

        foreach (var volume in volumes)
        {
            if (volume == null || volume.profile == null)
                continue;

            if (volume.profile.TryGet(out ColorAdjustments colorAdjustments))
            {
                colorAdjustments.colorFilter.overrideState = true;

                // Apply gamma as brightness tint (white scaled)
                colorAdjustments.colorFilter.value = new Color(
                    colorMultiplier,
                    colorMultiplier,
                    colorMultiplier,
                    1f
                );
            }
        }
    }

    public void EnterSettings() => SetGroupState(false, true);
    public void ExitSettings() => SetGroupState(true, false);

    private void SetGroupState(bool enableA, bool enableB)
    {
        if (disableOnOpen != null)
        {
            foreach (var obj in disableOnOpen)
                if (obj) obj.SetActive(enableA);
        }

        if (enableOnOpen != null)
        {
            foreach (var obj in enableOnOpen)
                if (obj) obj.SetActive(enableB);
        }
    }

    void Save()
    {
        PlayerPrefs.SetFloat(VOLUME_KEY, volume);
        PlayerPrefs.SetFloat(GAMMA_KEY, gamma);
        PlayerPrefs.SetInt(CROUCH_KEY, crouchToggleMode ? 1 : 0);
        PlayerPrefs.Save();
    }

    void Load()
    {
        volume = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);
        gamma = PlayerPrefs.GetFloat(GAMMA_KEY, 1f);
        crouchToggleMode = PlayerPrefs.GetInt(CROUCH_KEY, 1) == 1;
    }
}