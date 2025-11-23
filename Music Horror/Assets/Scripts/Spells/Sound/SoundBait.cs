using UnityEngine;

public class SoundBait : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private Renderer objectRenderer;

    private AudioSource audioSource;
    private EnemyAudioEmitter emitter;
    private float startVolume;
    private Material fadeMaterial;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        emitter = GetComponent<EnemyAudioEmitter>();

        if (objectRenderer == null)
            objectRenderer = GetComponentInChildren<Renderer>();

        fadeMaterial = objectRenderer.material;

        startVolume = audioSource.volume;

        // Play audio immediately
        audioSource.Play();

        // Start constantly emitting high noise
        StartCoroutine(EmitHighSoundLoop());

        // Begin fade-out of object + audio
        StartCoroutine(FadeAndDestroy());
    }

    private System.Collections.IEnumerator EmitHighSoundLoop()
    {
        while (true)
        {
            emitter.EmitSound(EnemyAudioEmitter.SoundLevel.High, 0.3f);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private System.Collections.IEnumerator FadeAndDestroy()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            // Fade audio
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t);

            // Fade visual
            Color c = fadeMaterial.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            fadeMaterial.color = c;

            yield return null;
        }

        Destroy(gameObject);
    }
}
