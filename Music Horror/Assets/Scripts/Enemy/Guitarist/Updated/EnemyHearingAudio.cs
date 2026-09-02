using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyHearingAudio : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private EnemySettings enemySettings;

    [Header("Hearing Audio")]
    [SerializeField] private AudioClip hearingAudio;

    [Range(0f, 1f)]
    [SerializeField] private float insideVolume = 0.5f;

    [Tooltip("How quickly the audio fades in and out.")]
    [SerializeField] private float fadeSpeed = 5f;

    private AudioSource audioSource;
    private Transform player;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Find player automatically.
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError(
                $"{name}: No GameObject with the 'Player' tag was found."
            );
        }

        // Configure AudioSource.
        audioSource.clip = hearingAudio;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;

        // Make sure the AudioSource isn't muted.
        audioSource.mute = false;

        // Start the audio immediately.
        if (audioSource.clip != null)
        {
            audioSource.Play();

            Debug.Log($"{name}: Hearing audio started.");
        }
        else
        {
            Debug.LogError(
                $"{name}: No hearing audio clip has been assigned."
            );
        }
    }

    private void Update()
    {
        if (player == null || enemySettings == null || audioSource.clip == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        bool insideHighHearingRange =
            distance <= enemySettings.HighHearingRange;

        float targetVolume = insideHighHearingRange
            ? insideVolume
            : 0f;

        audioSource.volume = Mathf.MoveTowards(
            audioSource.volume,
            targetVolume,
            fadeSpeed * Time.deltaTime
        );

        // Safety check in case the AudioSource stopped unexpectedly.
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}