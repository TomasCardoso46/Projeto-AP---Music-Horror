using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class SoundBait : MonoBehaviour
{
    [System.Serializable]
    public class AttackSound
    {
        public AudioClip clip;

        [Range(0f, 1f)]
        public float volume = 1f;

        [Tooltip("AudioSource used to play this sound. If empty, PlayClipAtPoint will be used.")]
        public AudioSource audioSource;
    }

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

    [Header("Attack Start Effects")]
    [SerializeField] private float attackEffectDelay = 0f;

    [Tooltip("Each sound can have its own AudioSource and volume.")]
    [SerializeField] private List<AttackSound> attackSounds = new();

    [Header("Smoke Effect")]
    [SerializeField] private GameObject smokePrefab;
    [SerializeField] private Transform smokeLocation;

    [Header("Enemy")]
    [SerializeField] private bool enableEnemy = false;
    [SerializeField] private GameObject enemy;
    [SerializeField] private EnemyAudioEmitter alternateEnemyAudioEmitter;

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
        if (enableEnemy)
        {
            emitter = alternateEnemyAudioEmitter;

            if (enemy != null)
                enemy.SetActive(true);
        }
        else
        {
            emitter = GetComponent<EnemyAudioEmitter>();
        }

        audioSource = GetComponent<AudioSource>();

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

            StartCoroutine(DisableEnemyScriptsDuringAttack(enemyAttack));
        }
    }

    private IEnumerator DisableEnemyScriptsDuringAttack(EnemyAttack enemyAttack)
    {

        MonoBehaviour[] scripts = enemyAttack.GetComponents<MonoBehaviour>();

        List<MonoBehaviour> disabledScripts = new List<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            if (script == null)
                continue;

            if (script == enemyAttack)
                continue;

            if (script.enabled)
            {
                script.enabled = false;
                disabledScripts.Add(script);
            }
        }

        // Disable the NavMeshAgent separately.
        NavMeshAgent navMeshAgent = enemyAttack.GetComponent<NavMeshAgent>();

        bool navMeshAgentWasEnabled = false;

        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgentWasEnabled = true;
            navMeshAgent.enabled = false;
        }

        enemyAttack.TriggerAttackAnimationOnly();


        if (attackEffectDelay > 0f)
        {
            yield return new WaitForSeconds(attackEffectDelay);
        }


        PlayAttackStartSounds();


        SpawnSmoke();


        float remainingLockTime = attackDisableDuration - attackEffectDelay;

        if (remainingLockTime > 0f)
        {
            yield return new WaitForSeconds(remainingLockTime);
        }

        foreach (MonoBehaviour script in disabledScripts)
        {
            if (script != null)
            {
                script.enabled = true;
            }
        }

        if (navMeshAgent != null && navMeshAgentWasEnabled)
        {
            navMeshAgent.enabled = true;
        }

        Destroy(gameObject);
    }

    private void PlayAttackStartSounds()
    {
        foreach (AttackSound sound in attackSounds)
        {
            if (sound == null || sound.clip == null)
                continue;


            if (sound.audioSource != null)
            {
                sound.audioSource.PlayOneShot(
                    sound.clip,
                    sound.volume
                );
            }

            else
            {
                GameObject tempAudioObject = new GameObject(
                    "Attack Sound - " + sound.clip.name
                );

                tempAudioObject.transform.position = transform.position;

                AudioSource tempSource =
                    tempAudioObject.AddComponent<AudioSource>();

                tempSource.clip = sound.clip;
                tempSource.volume = sound.volume;
                tempSource.spatialBlend = 1f;
                tempSource.Play();

                Destroy(
                    tempAudioObject,
                    sound.clip.length + 0.1f
                );
            }
        }
    }

    private void SpawnSmoke()
    {
        if (smokePrefab == null || smokeLocation == null)
            return;

        Instantiate(
            smokePrefab,
            smokeLocation.position,
            smokeLocation.rotation
        );
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