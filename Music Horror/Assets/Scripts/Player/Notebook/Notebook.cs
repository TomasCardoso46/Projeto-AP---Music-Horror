using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Notebook : MonoBehaviour
{
    [Header("GameObjects")]
    [SerializeField] private List<GameObject> objectsToToggle = new List<GameObject>();

    [Header("Scripts (MonoBehaviours)")]
    [SerializeField] private List<MonoBehaviour> scriptsToToggle = new List<MonoBehaviour>();

    [Header("Crosshair")]
    [SerializeField] private CrosshairEnable crosshairController;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip enableSound;
    [SerializeField] private AudioClip disableSound;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) || Gamepad.current.selectButton.wasPressedThisFrame)
        {
            SwapStates();
        }
    }

    private void SwapStates()
    {
        bool firstTransition = false;
        bool firstNewState = false;

        for (int i = 0; i < objectsToToggle.Count; i++)
        {
            GameObject obj = objectsToToggle[i];
            if (obj == null) continue;

            bool newState = !obj.activeSelf;

            if (i == 0)
            {
                firstTransition = true;
                firstNewState = newState;
            }

            obj.SetActive(newState);
        }

        for (int i = 0; i < scriptsToToggle.Count; i++)
        {
            MonoBehaviour script = scriptsToToggle[i];
            if (script == null) continue;

            bool newState = !script.enabled;

            if (!firstTransition)
            {
                firstTransition = true;
                firstNewState = newState;
            }

            script.enabled = newState;
        }

        if (crosshairController != null && firstTransition)
        {
            if (firstNewState)
                crosshairController.HideCrosshair();
            else
                crosshairController.ShowCrosshair();
        }

        if (audioSource != null && firstTransition)
        {
            AudioClip clip = firstNewState ? enableSound : disableSound;

            if (clip != null)
                audioSource.PlayOneShot(clip);
        }
    }
}