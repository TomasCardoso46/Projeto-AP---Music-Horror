using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class AudioInteraction : MonoBehaviour, IInteractable
{
    [Header("Audio")]
    [SerializeField] private AudioClip audioClip;

    [Header("Image")]
    [SerializeField] private Image image;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private AudioSource audioSource;
    private CanvasGroup canvasGroup;
    private Coroutine interactionCoroutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;

        if (image != null)
        {
            canvasGroup = image.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = image.gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
        }
    }

    public void Interact()
    {
        if (interactionCoroutine != null)
            return;

        if (audioClip == null || image == null)
        {
            Debug.LogWarning($"{name}: Missing Audio Clip or Image reference.");
            return;
        }

        if (canvasGroup != null && canvasGroup.alpha >= 1f)
            return;

        interactionCoroutine = StartCoroutine(PlayInteraction());
    }

    private IEnumerator PlayInteraction()
    {
        audioSource.clip = audioClip;
        audioSource.Play();

        yield return StartCoroutine(FadeImage(0f, 1f, fadeInDuration));
        yield return new WaitWhile(() => audioSource.isPlaying);

        yield return StartCoroutine(FadeImage(1f, 0f, fadeOutDuration));

        interactionCoroutine = null;
    }

    private IEnumerator FadeImage(float startAlpha, float targetAlpha, float duration)
    {
        if (duration <= 0f)
        {
            canvasGroup.alpha = targetAlpha;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}