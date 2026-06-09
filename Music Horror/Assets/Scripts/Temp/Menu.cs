using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI Groups")]
    [SerializeField] private GameObject[] objectsToEnableInSettings;
    [SerializeField] private GameObject[] objectsToDisableInSettings;

    private void Start()
    {
        // Unlock and show the mouse cursor when in the menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // This method loads the game scene
    public void StartGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene("Prototype");
    }

    // Opens settings UI
    public void Settings()
    {
        SetGroupState(objectsToEnableInSettings, true);
        SetGroupState(objectsToDisableInSettings, false);
    }

    // Returns back from settings UI
    public void Return()
    {
        SetGroupState(objectsToEnableInSettings, false);
        SetGroupState(objectsToDisableInSettings, true);
    }

    // Loads menu scene
    public void Menu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("Menu");
    }

    public void Credits()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("Victory");
    }
    // This method quits the application
    public void Quit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Helper function to toggle groups safely
    private void SetGroupState(GameObject[] objects, bool state)
    {
        if (objects == null) return;

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
                objects[i].SetActive(state);
        }
    }
}