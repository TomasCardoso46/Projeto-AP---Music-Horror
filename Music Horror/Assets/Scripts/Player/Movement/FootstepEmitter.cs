using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepEmitter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private EnemyAudioEmitter enemyAudioEmitter;

    [Header("Footstep Clips")]
    [SerializeField] private AudioClip[] footstepClips;

    [Header("Step Timing")]
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float sprintStepInterval = 0.35f;
    [SerializeField] private float crouchStepInterval = 0.8f;

    [Header("Volume Settings")]
    [SerializeField] [Range(0f, 1f)] private float walkVolume = 0.6f;
    [SerializeField] [Range(0f, 1f)] private float sprintVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float crouchVolume = 0.3f;

    [Header("Sound Emit Duration")]
    [SerializeField] private float soundDuration = 0.2f;

    private float stepTimer = 0f;
    private bool wasMovingLastFrame = false;

    private void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        if (!enemyAudioEmitter)
            Debug.LogWarning("EnemyAudioEmitter not assigned! AI won't hear footsteps.");
    }

    private void Update()
    {
        bool isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
        bool isCrouching = Input.GetKey(KeyCode.LeftControl);
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && !isCrouching;

        if (!isMoving)
        {
            stepTimer = 0f;
            wasMovingLastFrame = false;
            return;
        }

        float stepInterval = walkStepInterval;
        if (isSprinting) stepInterval = sprintStepInterval;
        if (isCrouching) stepInterval = crouchStepInterval;

        stepTimer += Time.deltaTime;

        if (!wasMovingLastFrame)
        {
            stepTimer = stepInterval * 0.5f;
            wasMovingLastFrame = true;
        }

        if (stepTimer >= stepInterval)
        {
            PlayFootstep(isSprinting, isCrouching);
            stepTimer = 0f;
        }
    }

    private void PlayFootstep(bool sprinting, bool crouching)
    {
        if (footstepClips.Length == 0) return;

        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];

        float volume = walkVolume;
        float pitch = 1f;
        EnemyAudioEmitter.SoundLevel soundLevel = EnemyAudioEmitter.SoundLevel.Normal;

        switch (true)
        {
            case true when sprinting:
                volume = sprintVolume;
                pitch = 1.15f;
                soundLevel = EnemyAudioEmitter.SoundLevel.High;
                break;
            case true when crouching:
                volume = crouchVolume;
                pitch = 0.9f;
                soundLevel = EnemyAudioEmitter.SoundLevel.Low;
                break;
            default:
                volume = walkVolume;
                pitch = 1f;
                soundLevel = EnemyAudioEmitter.SoundLevel.Normal;
                break;
        }

        audioSource.pitch = pitch * Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(clip, volume);

        if (enemyAudioEmitter != null)
            enemyAudioEmitter.EmitSound(soundLevel, soundDuration);
    }
}
