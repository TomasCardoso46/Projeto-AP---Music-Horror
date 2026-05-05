using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class Barricade : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;

    private bool isFading = false;
    private Renderer[] renderers;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isFading) return;

        if (other.GetComponent<StickyLight>() != null)
        {
            audioSource.PlayOneShot(audioClip);
            Destroy(other);
            StartCoroutine(FadeAndDestroy());
        }
    }

    private IEnumerator FadeAndDestroy()
    {
        isFading = true;
        float elapsed = 0f;
        float startVolume = audioSource.volume;


        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            // Fade audio
            if (audioSource != null)
                audioSource.volume = Mathf.Lerp(startVolume, 0f, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}