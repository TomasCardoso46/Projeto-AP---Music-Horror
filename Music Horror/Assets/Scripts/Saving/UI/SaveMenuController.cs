using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveMenuController : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject loadMenuRoot;
    [SerializeField] private GameObject mainMenuRoot;

    private void Start()
    {
        RefreshContinueButton();
    }

    public void ContinueGame()
    {
        var latest = SaveSystem.GetLatestSave();

        if (latest == null)
            return;

        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        SaveManager.Instance.LoadGame(latest.fileName);
    }

    public void NewGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OpenLoadMenu()
    {
        mainMenuRoot.SetActive(false);
        loadMenuRoot.SetActive(true);
    }

    public void RefreshContinueButton()
    {
        bool hasSaves = SaveSystem.GetAllSaves().Count > 0;
        continueButton.interactable = hasSaves;
    }
}