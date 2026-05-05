using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    [SerializeField] Slider volumeSlider;
    [SerializeField] Slider gammaSlider;
    [SerializeField] Toggle crouchToggle;
    [SerializeField] Button resetButton;

    [SerializeField] GameObject menuRoot;

    void Start()
    {
        var s = SettingsManager.Instance;

        volumeSlider.value = s.volume;
        gammaSlider.value = s.gamma;
        crouchToggle.isOn = s.crouchToggleMode;

        volumeSlider.onValueChanged.AddListener(s.SetVolume);
        gammaSlider.onValueChanged.AddListener(s.SetGamma);
        crouchToggle.onValueChanged.AddListener(s.SetCrouchMode);

        resetButton.onClick.AddListener(() =>
        {
            s.ResetSettings();

            volumeSlider.value = s.volume;
            gammaSlider.value = s.gamma;
            crouchToggle.isOn = s.crouchToggleMode;
        });
    }

    public void Open()
    {
        menuRoot.SetActive(true);
        SettingsManager.Instance.EnterSettings();
    }

    public void Close()
    {
        menuRoot.SetActive(false);
        SettingsManager.Instance.ExitSettings();
    }
}