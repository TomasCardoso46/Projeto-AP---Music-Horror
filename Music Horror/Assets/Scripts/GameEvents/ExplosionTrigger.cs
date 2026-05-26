using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class ExplosionTrigger : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] private ExplosionActivator explosionActivator;

    [Header("Animation")]
    [SerializeField] private Animation targetAnimation;
    [SerializeField] private string animationClipName;

    [Header("Audio")]
    [SerializeField] private List<AudioSource> audioSources = new();
    [SerializeField] private List<AudioClip> audioClips = new();
    [SerializeField] private float audioVolume = 1f;

    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered = false;

    private void Reset()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnlyOnce)
            return;

        if (!other.CompareTag(playerTag))
            return;

        hasTriggered = true;

        // Explosion
        if (explosionActivator != null)
        {
            explosionActivator.TriggerExplosion();
        }

        // Audio system
        PlayAllAudio();

        // Animation and destroy
        if (targetAnimation != null && !string.IsNullOrEmpty(animationClipName))
        {
            StartCoroutine(PlayAnimationAndDestroy());
        }
    }

    private void PlayAllAudio()
    {
        // Stop everything first (hard cut)
        foreach (AudioSource source in audioSources)
        {
            if (source == null) continue;
            source.Stop();
        }

        // Play clips
        for (int i = 0; i < audioSources.Count; i++)
        {
            AudioSource source = audioSources[i];
            if (source == null) continue;

            AudioClip clip = (i < audioClips.Count) ? audioClips[i] : null;

            if (clip != null)
            {
                source.PlayOneShot(clip, audioVolume);
            }
        }
    }

    private IEnumerator PlayAnimationAndDestroy()
    {
        targetAnimation.Play(animationClipName);

        AnimationClip clip = targetAnimation.GetClip(animationClipName);

        if (clip != null)
        {
            yield return new WaitForSeconds(clip.length);
        }

        Destroy(targetAnimation.gameObject);
    }
}