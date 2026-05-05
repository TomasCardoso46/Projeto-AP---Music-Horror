using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ScriptedChase : MonoBehaviour
{
    [System.Serializable]
    public class ChaseCheckpoint
    {
        public Transform checkpoint;
        [Tooltip("Speed the moving object will use AFTER reaching this checkpoint.")]
        public float speedAfterCheckpoint = 3f;
    }

    [Header("Chase Setup")]
    [SerializeField] private GameObject movingObject;
    [SerializeField] private List<ChaseCheckpoint> checkpoints = new List<ChaseCheckpoint>();
    [SerializeField] private SequenceLockController sequenceLockController;
    [SerializeField] private string lockedSequence = "1113";

    [Header("Movement Settings")]
    [SerializeField] private float initialSpeed = 3f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float reachDistance = 0.2f;

    [Header("Looping Movement Audio & Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animationStateName = "Chase";
    [SerializeField] private AudioSource loopingAudioSource;

    [Header("Event Sounds")]
    [SerializeField] private AudioSource eventAudioSource;
    [SerializeField] private AudioClip chaseStartClip;
    [SerializeField] private AudioClip chaseEndClip;

    [Header("Chase Music")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private float musicFadeInDuration = 2f;
    [SerializeField] private float musicFadeOutDuration = 2f;

    [Header("Speed Influence")]
    [SerializeField] private float animationSpeedMultiplier = 0.25f;
    [SerializeField] private float audioPitchMultiplier = 0.25f;

    private bool chaseStarted = false;
    private float currentSpeed;
    private int currentCheckpointIndex = 0;

    private float musicOriginalVolume = 1f;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Awake()
    {
        if (loopingAudioSource != null)
        {
            loopingAudioSource.playOnAwake = false;
            loopingAudioSource.loop = true;
            loopingAudioSource.Stop();
        }

        if (eventAudioSource != null)
            eventAudioSource.playOnAwake = false;

        if (musicAudioSource != null)
        {
            musicAudioSource.playOnAwake = false;
            musicAudioSource.loop = true;
            musicOriginalVolume = musicAudioSource.volume;
            musicAudioSource.volume = 0f;
            musicAudioSource.Stop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (chaseStarted)
            return;

        if (other.GetComponent<FirstPersonRigidbodyController>() != null)
        {
            Debug.Log("[ScriptedChase] Player detected. Starting chase.");
            sequenceLockController.LockSequence(lockedSequence);
            StartChase();
        }
    }

    private void StartChase()
    {
        if (movingObject == null || checkpoints.Count == 0)
        {
            Debug.LogError("[ScriptedChase] Missing movingObject or checkpoints.");
            return;
        }

        chaseStarted = true;
        currentSpeed = initialSpeed;
        currentCheckpointIndex = 0;

        if (eventAudioSource != null && chaseStartClip != null)
            eventAudioSource.PlayOneShot(chaseStartClip);

        if (animator != null && !string.IsNullOrEmpty(animationStateName))
        {
            animator.Play(animationStateName);
            animator.speed = currentSpeed * animationSpeedMultiplier;
        }

        if (loopingAudioSource != null)
        {
            loopingAudioSource.pitch = currentSpeed * audioPitchMultiplier;
            loopingAudioSource.Play();
        }

        if (musicAudioSource != null)
            StartCoroutine(FadeMusicIn());

        StartCoroutine(MoveAlongCheckpoints());
    }

    private IEnumerator MoveAlongCheckpoints()
    {
        while (currentCheckpointIndex < checkpoints.Count)
        {
            Transform target = checkpoints[currentCheckpointIndex].checkpoint;

            if (target == null)
            {
                currentCheckpointIndex++;
                continue;
            }

            while (Vector3.Distance(movingObject.transform.position, target.position) > reachDistance)
            {
                MoveTowards(target);
                UpdateAnimationAndAudioSpeed();
                yield return null;
            }

            movingObject.transform.position = target.position;
            currentSpeed = checkpoints[currentCheckpointIndex].speedAfterCheckpoint;

            currentCheckpointIndex++;
        }

        yield return EndChase();
    }

    private void MoveTowards(Transform target)
    {
        Vector3 direction = (target.position - movingObject.transform.position).normalized;

        movingObject.transform.position += direction * currentSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            movingObject.transform.rotation = Quaternion.Slerp(
                movingObject.transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void UpdateAnimationAndAudioSpeed()
    {
        if (animator != null)
            animator.speed = currentSpeed * animationSpeedMultiplier;

        if (loopingAudioSource != null)
            loopingAudioSource.pitch = currentSpeed * audioPitchMultiplier;
    }

    private IEnumerator FadeMusicIn()
    {
        musicAudioSource.volume = 0f;
        musicAudioSource.Play();

        float time = 0f;
        while (time < musicFadeInDuration)
        {
            time += Time.deltaTime;
            musicAudioSource.volume = Mathf.Lerp(0f, musicOriginalVolume, time / musicFadeInDuration);
            yield return null;
        }

        musicAudioSource.volume = musicOriginalVolume;
    }

    private IEnumerator EndChase()
    {
        if (eventAudioSource != null && chaseEndClip != null)
            eventAudioSource.PlayOneShot(chaseEndClip);

        if (loopingAudioSource != null)
            loopingAudioSource.Stop();

        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            float startVolume = musicAudioSource.volume;
            float time = 0f;

            while (time < musicFadeOutDuration)
            {
                time += Time.deltaTime;
                musicAudioSource.volume = Mathf.Lerp(startVolume, 0f, time / musicFadeOutDuration);
                yield return null;
            }

            musicAudioSource.Stop();
            musicAudioSource.volume = musicOriginalVolume;
        }

        if (movingObject != null)
            Destroy(movingObject);

        GetComponent<Collider>().enabled = false;
    }
}