using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseMenuUI;

    [Header("References")]
    [SerializeField] private CrosshairEnable crosshairController;
    [SerializeField] private MenuAudioController menuAudioController;

    private void Start()
    {
        GameState.IsPaused = false;

        pauseMenuUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameState.IsPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        GameState.IsPaused = true;

        pauseMenuUI.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        crosshairController?.HideCrosshair();
        menuAudioController?.OpenMenuAudio();
    }

    public void ResumeGame()
    {
        GameState.IsPaused = false;

        pauseMenuUI.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        crosshairController?.ShowCrosshair();
        menuAudioController?.CloseMenuAudio();
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        GameState.IsPaused = false;
    }
}