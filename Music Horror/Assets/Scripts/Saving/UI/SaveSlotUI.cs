using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private Button button;

    private string fileName;

    public void Setup(string file, SaveData data)
    {
        fileName = file;

        titleText.text = data.locationName;
        dateText.text = data.dateTime;

        if (SaveIconDatabase.Instance != null)
            icon.sprite = SaveIconDatabase.Instance.Get(data.locationImageKey);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(LoadSave);
    }

    private void LoadSave()
    {
        PlayerPrefs.SetString("PendingSaveToLoad", fileName);
        SceneManager.LoadScene("Prototype");
    }
}