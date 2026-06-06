using System.Collections;
using TMPro;
using UnityEngine;

public class LookPromptManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private CanvasGroup promptCanvasGroup;

    [Header("Audio")]
    [SerializeField] private AudioSource voiceAudioSource;

    [Header("Raycast Settings")]
    [SerializeField] private float rayDistance = 20f;

    [Header("Fade")]
    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float fadeOutDuration = 0.4f;

    private Coroutine currentRoutine;
    private LookPrompt activeLookPrompt;

    private void Start()
    {
        promptCanvasGroup.alpha = 0f;
        promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        HandleLookPrompt();
    }

    private void HandleLookPrompt()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            LookPrompt prompt = hit.collider.GetComponentInParent<LookPrompt>();

            if (prompt != null &&
                prompt.CanAppear() &&
                Vector3.Distance(playerCamera.transform.position, prompt.transform.position) <= prompt.maxDistance)
            {
                if (activeLookPrompt != prompt)
                {
                    TriggerLookPrompt(prompt);
                }

                return;
            }
        }
    }

    private void TriggerLookPrompt(LookPrompt prompt)
    {
        activeLookPrompt = prompt;
        prompt.RegisterAppearance();

        PlayPrompt(
            prompt.promptText,
            prompt.displayDuration,
            prompt.voiceClip
        );
    }

    private void PlayPrompt(string text, float duration, AudioClip clip)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        if (voiceAudioSource != null)
        {
            voiceAudioSource.Stop();

            if (clip != null)
                voiceAudioSource.PlayOneShot(clip);
        }

        currentRoutine = StartCoroutine(PromptRoutine(text, duration));
    }

    private IEnumerator PromptRoutine(string text, float duration)
    {
        promptText.text = text;
        promptText.gameObject.SetActive(true);

        yield return Fade(0f, 1f, fadeInDuration);

        yield return new WaitForSeconds(duration);

        yield return Fade(1f, 0f, fadeOutDuration);

        promptText.gameObject.SetActive(false);

        activeLookPrompt = null;
        currentRoutine = null;
    }

    private IEnumerator Fade(float from, float to, float time)
    {
        float t = 0f;

        while (t < time)
        {
            t += Time.deltaTime;
            promptCanvasGroup.alpha = Mathf.Lerp(from, to, t / time);
            yield return null;
        }

        promptCanvasGroup.alpha = to;
    }
}