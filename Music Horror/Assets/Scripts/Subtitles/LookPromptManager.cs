using System.Collections;
using TMPro;
using UnityEngine;

public class LookPromptManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private CanvasGroup promptCanvasGroup;

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

    #region LOOK PROMPTS

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

        PlayPrompt(prompt.promptText, prompt.displayDuration);
    }

    #endregion

    #region ZONE PROMPTS (external trigger)

    public void TriggerZonePrompt(ZonePrompt zonePrompt)
    {
        if (zonePrompt == null || !zonePrompt.CanAppear())
            return;

        zonePrompt.RegisterAppearance();
        PlayPrompt(zonePrompt.promptText, zonePrompt.displayDuration);
    }

    #endregion

    #region CORE PROMPT SYSTEM

    private void PlayPrompt(string text, float duration)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(PromptRoutine(text, duration));
    }

    private IEnumerator PromptRoutine(string text, float duration)
    {
        promptText.text = text;
        promptText.gameObject.SetActive(true);

        // Fade In
        yield return Fade(0f, 1f, fadeInDuration);

        // Hold
        yield return new WaitForSeconds(duration);

        // Fade Out
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

    #endregion
}