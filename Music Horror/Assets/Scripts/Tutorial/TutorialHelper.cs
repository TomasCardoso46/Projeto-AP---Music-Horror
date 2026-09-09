using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialHelper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Chord chord;

    [Header("Stage 1")]
    [SerializeField] private GameObject objectToDisable;
    [SerializeField] private GameObject objectToEnable;
    [SerializeField] private int chordsRequired = 5;
    [SerializeField] private float stageOneTimeout = 20f;

    [Header("Stage 2")]
    [SerializeField] private int modeChangesRequired = 3;
    [SerializeField] private float stageTwoTimeout = 20f;

    [Header("Screen Shake")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float shakeDuration = 1.5f;
    [SerializeField] private float shakeMagnitude = 0.15f;
    [SerializeField] private float shakeFrequency = 25f;

    [Header("Shake Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shakeSound;

    [Header("Fade")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float timeBeforeFade = 2f;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float timeAfterFade = 2f;

    private int chordsPlayed;
    private int modeChanges;
    private bool stageOneComplete;
    private bool stageTwoStarted;
    private bool finished;

    private Coroutine tutorialCoroutine;

    private void Awake()
    {
        if (chord == null)
            chord = FindFirstObjectByType<Chord>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
        }
    }

    private void OnEnable()
    {
        if (chord != null)
        {
            chord.OnChordPlayed += HandleChordPlayed;
            chord.OnGuitarModeChanged += HandleGuitarModeChanged;
        }
    }

    private void OnDisable()
    {
        if (chord != null)
        {
            chord.OnChordPlayed -= HandleChordPlayed;
            chord.OnGuitarModeChanged -= HandleGuitarModeChanged;
        }
    }

    private void Start()
    {
        tutorialCoroutine = StartCoroutine(TutorialSequence());
    }

    private void HandleChordPlayed()
    {
        if (stageOneComplete || finished)
            return;

        chordsPlayed++;

        if (chordsPlayed >= chordsRequired)
            CompleteStageOne();
    }

    private void HandleGuitarModeChanged()
    {
        if (!stageOneComplete || stageTwoStarted || finished)
            return;

        modeChanges++;

        if (modeChanges >= modeChangesRequired)
            StartStageTwo();
    }

    private IEnumerator TutorialSequence()
    {
        float timer = 0f;

        while (!stageOneComplete && !finished)
        {
            timer += Time.deltaTime;

            if (timer >= stageOneTimeout)
            {
                CompleteStageOne();
                break;
            }

            yield return null;
        }

        while (!stageTwoStarted && !finished)
            yield return null;

        if (finished)
            yield break;

        yield return StartCoroutine(FinalSequence());
    }

    private void CompleteStageOne()
    {
        if (stageOneComplete || finished)
            return;

        stageOneComplete = true;

        if (objectToDisable != null)
            objectToDisable.SetActive(false);

        if (objectToEnable != null)
            objectToEnable.SetActive(true);

        StartCoroutine(StageTwoTimer());
    }

    private IEnumerator StageTwoTimer()
    {
        float timer = 0f;

        while (!stageTwoStarted && !finished)
        {
            timer += Time.deltaTime;

            if (timer >= stageTwoTimeout)
            {
                StartStageTwo();
                yield break;
            }

            yield return null;
        }
    }

    private void StartStageTwo()
    {
        if (stageTwoStarted || finished)
            return;

        stageTwoStarted = true;
    }

    private IEnumerator FinalSequence()
    {
        if (audioSource != null && shakeSound != null)
            audioSource.PlayOneShot(shakeSound);

        StartCoroutine(ShakeScreen());

        yield return new WaitForSeconds(timeBeforeFade);

        yield return StartCoroutine(FadeToBlack());

        yield return new WaitForSeconds(timeAfterFade);

        finished = true;

        SceneManager.LoadScene("Text2");
    }

    private IEnumerator ShakeScreen()
    {
        if (cameraTransform == null)
            yield break;

        Vector3 originalPosition = cameraTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float damper = 1f - Mathf.Clamp01(elapsed / shakeDuration);

            float x = (Mathf.PerlinNoise(
                Time.time * shakeFrequency, 0f) - 0.5f) * 2f;

            float y = (Mathf.PerlinNoise(
                0f, Time.time * shakeFrequency) - 0.5f) * 2f;

            cameraTransform.localPosition = originalPosition +
                new Vector3(x, y, 0f) * shakeMagnitude * damper;

            yield return null;
        }

        cameraTransform.localPosition = originalPosition;
    }

    private IEnumerator FadeToBlack()
    {
        if (fadeImage == null)
            yield break;

        Color color = fadeImage.color;
        float startingAlpha = color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            color.a = Mathf.Lerp(
                startingAlpha,
                1f,
                Mathf.Clamp01(elapsed / fadeDuration)
            );

            fadeImage.color = color;

            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
    }
}