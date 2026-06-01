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

    void Start()
    {
        var s = SettingsManager.Instance;

        RefreshUI();

        volumeSlider.onValueChanged.AddListener(s.SetVolume);
        gammaSlider.onValueChanged.AddListener(s.SetGamma);
        crouchToggle.onValueChanged.AddListener(s.SetCrouchMode);

        resetButton.onClick.AddListener(() =>
        {
            s.ResetSettings();
            RefreshUI();
        });
    }

    void RefreshUI()
    {
        var s = SettingsManager.Instance;

        volumeSlider.SetValueWithoutNotify(s.volume);
        gammaSlider.SetValueWithoutNotify(s.gamma);
        crouchToggle.SetIsOnWithoutNotify(s.crouchToggleMode);
    }

    public void Open()
    {
        RefreshUI();

        if (previousMenuRoot != null)
            previousMenuRoot.SetActive(false);

        menuRoot.SetActive(true);

        SettingsManager.Instance.EnterSettings();
    }

    public void Close()
    {
        menuRoot.SetActive(false);

        if (previousMenuRoot != null)
            previousMenuRoot.SetActive(true);

        SettingsManager.Instance.ExitSettings();
    }
}