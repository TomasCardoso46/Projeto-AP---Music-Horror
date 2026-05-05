using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class TriggerActivateRigidbodies : MonoBehaviour
{
    [Header("Target Objects")]
    [SerializeField] private List<GameObject> objectsToActivate = new List<GameObject>();

    [Header("Audio (Index Matched)")]
    [SerializeField] private List<AudioClip> activationSounds = new List<AudioClip>();
    [SerializeField] private List<AudioSource> audioSources = new List<AudioSource>();

    private bool hasTriggered = false;

    private void Awake()
    {
        // Ensure collider is trigger
        GetComponent<Collider>().isTrigger = true;

        // Disable play on awake for all assigned sources
        foreach (var source in audioSources)
        {
            if (source != null)
                source.playOnAwake = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.GetComponent<FirstPersonRigidbodyController>() != null)
        {
            hasTriggered = true;

            ActivateRigidbodies();

            if (activationSounds.Count == 0 || audioSources.Count == 0)
            {
                Debug.LogWarning("[TriggerActivateRigidbodies] Sounds or AudioSources not assigned.");
                return;
            }

            if (activationSounds.Count != audioSources.Count)
            {
                Debug.LogWarning("[TriggerActivateRigidbodies] Sounds and AudioSources count mismatch.");
            }

            StartCoroutine(PlaySoundsSequentially());
        }
    }

    private void ActivateRigidbodies()
    {
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj == null) continue;

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
            else
            {
                Debug.LogWarning($"[TriggerActivateRigidbodies] No Rigidbody found on {obj.name}");
            }
        }

        Debug.Log("[TriggerActivateRigidbodies] Rigidbodies activated.");
    }

    private IEnumerator PlaySoundsSequentially()
    {
        int count = Mathf.Min(activationSounds.Count, audioSources.Count);

        for (int i = 0; i < count; i++)
        {
            AudioClip clip = activationSounds[i];
            AudioSource source = audioSources[i];

            if (clip == null || source == null)
                continue;

            source.clip = clip;
            source.Play();

            yield return new WaitWhile(() => source.isPlaying);
        }
    }
}