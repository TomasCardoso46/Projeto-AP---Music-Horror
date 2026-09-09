using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PressAnyKeyToContinue : MonoBehaviour
{
    [Header("Primary Text")]
    [SerializeField] private TMP_Text primaryText;
    [SerializeField] private float primaryFadeInDuration = 1f;
    [SerializeField] private float primaryFadeOutDuration = 0.5f;

    [Header("Secondary Text")]
    [SerializeField] private TMP_Text secondaryText;
    [SerializeField] private float secondaryFadeInDelay = 2f;
    [SerializeField] private float secondaryFadeInDuration = 1f;
    [SerializeField] private float secondaryFadeOutDuration = 0.25f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pressSound;

    [Header("Scene")]
    [SerializeField] private string sceneToLoad;

    private bool pressed;

    private void Start()
    {
        SetAlpha(primaryText, 0f);
        SetAlpha(secondaryText, 0f);

        StartCoroutine(FadeText(primaryText, 0f, 1f, primaryFadeInDuration));
        StartCoroutine(SecondaryTextRoutine());
    }

    private void Update()
    {
        if (pressed)
            return;

        if (AnyInputPressed())
            Continue();
    }

    private bool AnyInputPressed()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        if (Mouse.current != null &&
            (Mouse.current.leftButton.wasPressedThisFrame ||
             Mouse.current.rightButton.wasPressedThisFrame ||
             Mouse.current.middleButton.wasPressedThisFrame ||
             Mouse.current.forwardButton.wasPressedThisFrame ||
             Mouse.current.backButton.wasPressedThisFrame))
            return true;

        if (Gamepad.current != null && AnyGamepadInput())
            return true;

        return false;
    }

    private bool AnyGamepadInput()
    {
        Gamepad gamepad = Gamepad.current;

        return
            gamepad.buttonSouth.wasPressedThisFrame ||
            gamepad.buttonNorth.wasPressedThisFrame ||
            gamepad.buttonEast.wasPressedThisFrame ||
            gamepad.buttonWest.wasPressedThisFrame ||
            gamepad.leftShoulder.wasPressedThisFrame ||
            gamepad.rightShoulder.wasPressedThisFrame ||
            gamepad.leftTrigger.wasPressedThisFrame ||
            gamepad.rightTrigger.wasPressedThisFrame ||
            gamepad.startButton.wasPressedThisFrame ||
            gamepad.selectButton.wasPressedThisFrame ||
            gamepad.leftStickButton.wasPressedThisFrame ||
            gamepad.rightStickButton.wasPressedThisFrame ||
            gamepad.dpad.up.wasPressedThisFrame ||
            gamepad.dpad.down.wasPressedThisFrame ||
            gamepad.dpad.left.wasPressedThisFrame ||
            gamepad.dpad.right.wasPressedThisFrame;
    }

    private void Continue()
    {
        pressed = true;

        if (audioSource != null && pressSound != null)
            audioSource.PlayOneShot(pressSound);

        StartCoroutine(FadeOutAndLoad());
    }

    private IEnumerator FadeOutAndLoad()
    {
        Coroutine primaryFade = StartCoroutine(
            FadeText(primaryText, 1f, 0f, primaryFadeOutDuration)
        );

        Coroutine secondaryFade = StartCoroutine(
            FadeText(secondaryText, GetAlpha(secondaryText), 0f, secondaryFadeOutDuration)
        );

        yield return primaryFade;
        yield return secondaryFade;

        SceneManager.LoadScene(sceneToLoad);
    }

    private IEnumerator SecondaryTextRoutine()
    {
        yield return new WaitForSeconds(secondaryFadeInDelay);

        if (pressed)
            yield break;

        yield return FadeText(
            secondaryText,
            0f,
            1f,
            secondaryFadeInDuration
        );
    }

    private IEnumerator FadeText(
        TMP_Text text,
        float startAlpha,
        float endAlpha,
        float duration)
    {
        if (text == null)
            yield break;

        float elapsed = 0f;
        Color color = text.color;
        color.a = startAlpha;
        text.color = color;

        if (duration <= 0f)
        {
            color.a = endAlpha;
            text.color = color;
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);

            text.color = color;

            yield return null;
        }

        color.a = endAlpha;
        text.color = color;
    }

    private void SetAlpha(TMP_Text text, float alpha)
    {
        if (text == null)
            return;

        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }

    private float GetAlpha(TMP_Text text)
    {
        return text != null ? text.color.a : 0f;
    }
}