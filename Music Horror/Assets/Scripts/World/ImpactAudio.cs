using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class ImpactAudio : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip[] impactClips;

    [Header("Impact Settings")]
    [SerializeField] private float minImpactSpeed = 3f;
    [SerializeField] private float maxImpactSpeed = 20f;

    [Header("Audio Response")]
    [SerializeField] private float minVolume = 0.1f;
    [SerializeField] private float maxVolume = 1f;

    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.1f;

    [Header("Anti-Spam")]
    [SerializeField] private float postSoundCooldown = 0.15f;

    private AudioSource audioSource;
    private bool canPlay = true;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!canPlay)
            return;

        float impactSpeed = collision.relativeVelocity.magnitude;

        if (impactSpeed < minImpactSpeed)
            return;

        if (impactClips == null || impactClips.Length == 0)
            return;

        float t = Mathf.InverseLerp(minImpactSpeed, maxImpactSpeed, impactSpeed);

        float volume = Mathf.Lerp(minVolume, maxVolume, t);
        float pitch = Mathf.Lerp(minPitch, maxPitch, t);

        AudioClip clip = impactClips[Random.Range(0, impactClips.Length)];

        audioSource.pitch = pitch;
        audioSource.PlayOneShot(clip, volume);

        StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        canPlay = false;
        yield return new WaitForSeconds(postSoundCooldown);
        canPlay = true;
    }
}