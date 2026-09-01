using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseMenuUI;

    [Header("References")]
    [SerializeField] private CrosshairEnable crosshairController;
    [SerializeField] private MenuAudioController menuAudioController;

    [Header("Disable While Paused")]
    [SerializeField] private MonoBehaviour[] gameplayScripts;

    private void Start()
    {
        GameState.IsPaused = false;

        pauseMenuUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Gamepad.current.startButton.wasPressedThisFrame)
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

        foreach (MonoBehaviour script in gameplayScripts)
        {
            if (script != null)
                script.enabled = false;
        }

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

        foreach (MonoBehaviour script in gameplayScripts)
        {
            if (script != null)
                script.enabled = true;
        }

        crosshairController?.ShowCrosshair();
        menuAudioController?.CloseMenuAudio();
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        GameState.IsPaused = false;
    }
}