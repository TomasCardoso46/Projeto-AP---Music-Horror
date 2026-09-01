using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    [SerializeField] Slider volumeSlider;
    [SerializeField] Slider gammaSlider;
    [SerializeField] Toggle crouchToggle;
    [SerializeField] Button resetButton;

    [SerializeField] GameObject menuRoot;
    [SerializeField] GameObject previousMenuRoot;

    private SettingsManager settingsManager;

    void Start()
    {
        settingsManager = SettingsManager.Instance;

        if (settingsManager == null)
        {
            Debug.LogError("SettingsMenuUI: SettingsManager.Instance is null. Make sure a SettingsManager exists in the scene before the SettingsMenuUI starts.");
            return;
        }

        RefreshUI();

        volumeSlider.onValueChanged.AddListener(settingsManager.SetVolume);
        gammaSlider.onValueChanged.AddListener(settingsManager.SetGamma);
        crouchToggle.onValueChanged.AddListener(settingsManager.SetCrouchMode);

        resetButton.onClick.AddListener(() =>
        {
            settingsManager.ResetSettings();
            RefreshUI();
        });
    }

    void RefreshUI()
    {
        if (settingsManager == null)
            return;

        volumeSlider.SetValueWithoutNotify(settingsManager.volume);
        gammaSlider.SetValueWithoutNotify(settingsManager.gamma);
        crouchToggle.SetIsOnWithoutNotify(settingsManager.crouchToggleMode);
    }

    public void Open()
    {
        settingsManager = SettingsManager.Instance;

        if (settingsManager == null)
        {
            Debug.LogError("SettingsMenuUI: Cannot open settings because SettingsManager.Instance is null.");
            return;
        }

        RefreshUI();

        if (previousMenuRoot != null)
            previousMenuRoot.SetActive(false);

        menuRoot.SetActive(true);

        settingsManager.EnterSettings();
    }

    public void Close()
    {
        menuRoot.SetActive(false);

        if (previousMenuRoot != null)
            previousMenuRoot.SetActive(true);

        if (settingsManager != null)
            settingsManager.ExitSettings();
    }
}