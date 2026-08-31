using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AnimationFootstepAudio : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Footstep Clips")]
    [SerializeField] private AudioClip[] footstepClips;

    [Header("Audio Settings")]
    [SerializeField] private float volume = 1f;
    [SerializeField] private float pitch = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float spatialBlend = 1f;

    [Header("Random Pitch")]
    [SerializeField] private bool useRandomPitch = false;
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

    private int lastClipIndex = -1;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        audioSource.spatialBlend = spatialBlend;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    public void PlayFootstep()
    {
        if (audioSource == null)
            return;

        if (footstepClips == null || footstepClips.Length == 0)
            return;

        int index;

        if (footstepClips.Length == 1)
        {
            index = 0;
        }
        else
        {
            do
            {
                index = Random.Range(0, footstepClips.Length);
            }
            while (index == lastClipIndex);
        }

        lastClipIndex = index;

        audioSource.pitch = useRandomPitch
            ? Random.Range(minPitch, maxPitch)
            : pitch;

        audioSource.PlayOneShot(
            footstepClips[index],
            volume
        );
    }

    public void PlayFootstep(int index)
    {
        if (audioSource == null)
            return;

        if (footstepClips == null || footstepClips.Length == 0)
            return;

        if (index < 0 || index >= footstepClips.Length)
            return;

        lastClipIndex = index;

        audioSource.pitch = useRandomPitch
            ? Random.Range(minPitch, maxPitch)
            : pitch;

        audioSource.PlayOneShot(
            footstepClips[index],
            volume
        );
    }

    public void SetClips(AudioClip[] clips)
    {
        footstepClips = clips;
        lastClipIndex = -1;
    }
}