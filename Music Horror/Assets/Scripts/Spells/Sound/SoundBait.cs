using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class SoundBait : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private Renderer objectRenderer;

    [Header("Temporary Material Settings")]
    [SerializeField] private Material activeMaterial;
    [SerializeField] private float activeDuration = 5f;

    [Header("Music Playlist")]
    [SerializeField] private List<AudioClip> musicClips = new();

    [Header("Speaker Pulse")]
    [SerializeField] private bool pulseEnabled = true;
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float pulseAmount = 0.1f;

    [Header("Enemy Attack")]
    [SerializeField] private float attackDisableDuration = 5f;

    private AudioSource audioSource;
    private EnemyAudioEmitter emitter;
    private float startVolume;

    private Material originalMaterial;

    private Coroutine emitLoopCoroutine;
    private Coroutine musicLoopCoroutine;

    private bool enemyTriggered = false;

    private Vector3 originalScale;

    private void OnEnable()
    {
        audioSource = GetComponent<AudioSource>();
        emitter = GetComponent<EnemyAudioEmitter>();

        if (objectRenderer == null)
            objectRenderer = GetComponentInChildren<Renderer>();

        startVolume = audioSource.volume;

        if (objectRenderer != null)
            originalMaterial = objectRenderer.material;

        if (activeMaterial != null && objectRenderer != null)
        {
            objectRenderer.material = activeMaterial;
        }

        originalScale = transform.localScale;

        musicLoopCoroutine = StartCoroutine(MusicLoop());
        emitLoopCoroutine = StartCoroutine(EmitHighSoundLoop());

        StartCoroutine(DeactivateAfterDelay());
    }

    private void Update()
    {
        if (!pulseEnabled)
            return;

        if (audioSource != null && audioSource.isPlaying)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

            transform.localScale = originalScale * pulse;
        }
    }

    private IEnumerator MusicLoop()
    {
        while (true)
        {
            if (musicClips.Count > 0)
            {
                AudioClip randomClip = musicClips[Random.Range(0, musicClips.Count)];

                audioSource.clip = randomClip;
                audioSource.Play();

                yield return new WaitForSeconds(randomClip.length);
            }
            else
            {
                yield return null;
            }
        }
    }

    private IEnumerator EmitHighSoundLoop()
    {
        while (true)
        {
            if (emitter != null)
            {
                emitter.EmitSound(
                    EnemyAudioEmitter.SoundLevel.High,
                    0.3f
                );
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(activeDuration);

        RestoreMaterialAndStop();

        enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemyTriggered)
            return;

        EnemyAttack enemyAttack = other.GetComponent<EnemyAttack>();

        if (enemyAttack != null)
        {
            enemyTriggered = true;

            RestoreMaterialAndStop();

            // The coroutine must finish before destroying this object,
            // otherwise the coroutine would be stopped along with it.
            StartCoroutine(DisableEnemyScriptsDuringAttack(enemyAttack));
        }
    }

    private IEnumerator DisableEnemyScriptsDuringAttack(EnemyAttack enemyAttack)
    {
        // Get every MonoBehaviour attached to the same GameObject
        // as EnemyAttack.
        MonoBehaviour[] scripts = enemyAttack.GetComponents<MonoBehaviour>();

        // Keep track of only the scripts that were originally enabled.
        // This prevents us from accidentally enabling scripts that were
        // already disabled before the attack.
        List<MonoBehaviour> disabledScripts = new List<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            if (script == null)
                continue;

            // EnemyAttack must remain enabled.
            if (script == enemyAttack)
                continue;

            if (script.enabled)
            {
                script.enabled = false;
                disabledScripts.Add(script);
            }
        }

        // Trigger the attack while all other scripts are disabled.
        enemyAttack.TriggerAttackAnimationOnly();

        // Keep the enemy locked for the specified amount of time.
        yield return new WaitForSeconds(attackDisableDuration);

        // Restore the scripts that were enabled before the attack.
        foreach (MonoBehaviour script in disabledScripts)
        {
            if (script != null)
            {
                script.enabled = true;
            }
        }

        // The bait has finished its job.
        Destroy(gameObject);
    }

    private void RestoreMaterialAndStop()
    {
        if (originalMaterial != null && objectRenderer != null)
        {
            objectRenderer.material = originalMaterial;
        }

        transform.localScale = originalScale;

        if (emitLoopCoroutine != null)
        {
            StopCoroutine(emitLoopCoroutine);
            emitLoopCoroutine = null;
        }

        if (musicLoopCoroutine != null)
        {
            StopCoroutine(musicLoopCoroutine);
            musicLoopCoroutine = null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}