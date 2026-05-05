using UnityEngine;
using System.Collections;

public class SoundBait : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private Renderer objectRenderer;

    [Header("Temporary Material Settings")]
    [SerializeField] private Material activeMaterial;
    [SerializeField] private float activeDuration = 5f;

    private AudioSource audioSource;
    private EnemyAudioEmitter emitter;
    private float startVolume;

    private Material originalMaterial;
    private Material fadeMaterial;

    private Coroutine emitLoopCoroutine;

    private bool enemyTriggered = false; // prevents double-trigger

    void OnEnable()
    {
        audioSource = GetComponent<AudioSource>();
        emitter = GetComponent<EnemyAudioEmitter>();

        if (objectRenderer == null)
            objectRenderer = GetComponentInChildren<Renderer>();

        fadeMaterial = objectRenderer.material;

        startVolume = audioSource.volume;

        originalMaterial = objectRenderer.material;

        if (activeMaterial != null)
        {
            objectRenderer.material = activeMaterial;
        }

        audioSource.Play();

        emitLoopCoroutine = StartCoroutine(EmitHighSoundLoop());

        StartCoroutine(DeactivateAfterDelay());
    }

    private IEnumerator EmitHighSoundLoop()
    {
        while (true)
        {
            emitter.EmitSound(EnemyAudioEmitter.SoundLevel.High, 0.3f);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(activeDuration);

        RestoreMaterialAndStop();

        this.enabled = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (enemyTriggered) return;

        EnemyAttack enemyAttack = other.GetComponent<EnemyAttack>();

        if (enemyAttack != null)
        {
            enemyTriggered = true;

            // Trigger ONLY the animation using EnemyAttack's animator reference
            enemyAttack.TriggerAttackAnimationOnly();

            RestoreMaterialAndStop();

            Destroy(gameObject);
        }
    }


    private void RestoreMaterialAndStop()
    {
        if (originalMaterial != null)
        {
            objectRenderer.material = originalMaterial;
        }

        if (emitLoopCoroutine != null)
        {
            StopCoroutine(emitLoopCoroutine);
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}