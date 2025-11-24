using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSoundPlayer : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;   // Your assigned audio source
    [SerializeField] private List<AudioClip> clips = new List<AudioClip>();

    [Header("Timing Settings")]
    [Tooltip("Minimum time between random sound attempts.")]
    [SerializeField] private float minDelay = 5f;

    [Tooltip("Maximum time between random sound attempts.")]
    [SerializeField] private float maxDelay = 10f;

    private Coroutine playRoutine;

    private void Start()
    {
        if (audioSource == null)
        {
            Debug.LogWarning($"{name}: RandomSoundPlayer has no AudioSource assigned!");
            enabled = false;
            return;
        }

        playRoutine = StartCoroutine(RandomSoundLoop());
    }

    private IEnumerator RandomSoundLoop()
    {
        while (true)
        {
            // Wait a random time
            float waitTime = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(waitTime);

            TryPlayRandomSound();
        }
    }

    private void TryPlayRandomSound()
    {
        // No clips? bail out.
        if (clips.Count == 0) return;

        // If audio is already playing, we "count it" but do not actually play a new clip.
        if (audioSource.isPlaying)
        {
            // Do nothing — still counts as an event.
            return;
        }

        // Pick a random clip
        AudioClip clip = clips[Random.Range(0, clips.Count)];

        // Make sure looping is off
        audioSource.loop = false;

        // Play it
        audioSource.clip = clip;
        audioSource.Play();
    }
}
