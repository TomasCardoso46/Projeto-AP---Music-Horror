using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TriggerPedal : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Jumpscare jumpscare;

    [Header("Shake Settings")]
    [SerializeField] private GameObject objectToShake;
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeMagnitude = 15f;

    private enum ShakeAxis { X, Y, Z }
    [SerializeField] private ShakeAxis shakeAxis = ShakeAxis.Y;

    [Header("Audio")]
    [SerializeField] private AudioSource openSoundSource;
    [SerializeField] private AudioClip pedalSoundClip;

    [Header("Post Processing")]
    [SerializeField] private Volume volume;

    [Header("Post Processing Values (Triggered)")]
    [SerializeField] private float bloomIntensity = 15f;
    [SerializeField] private float chromaticAberrationIntensity = 0.4f;
    [SerializeField] private float filmGrainIntensity = 0.6f;

    private Bloom bloom;
    private ChromaticAberration chromaticAberration;
    private FilmGrain filmGrain;

    private float originalBloomIntensity;
    private float originalChromaticIntensity;
    private float originalFilmGrainIntensity;

    private Quaternion originalRotation;
    private bool triggered = false;

    private void Awake()
    {
        if (objectToShake != null)
            originalRotation = objectToShake.transform.localRotation;

        if (volume != null && volume.profile != null)
        {
            volume.profile.TryGet(out bloom);
            volume.profile.TryGet(out chromaticAberration);
            volume.profile.TryGet(out filmGrain);

            if (bloom != null)
                originalBloomIntensity = bloom.intensity.value;

            if (chromaticAberration != null)
                originalChromaticIntensity = chromaticAberration.intensity.value;

            if (filmGrain != null)
                originalFilmGrainIntensity = filmGrain.intensity.value;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if (other.CompareTag(playerTag))
        {
            triggered = true;

            ApplyPostProcessing();
            PlayOpenSound();

            StartCoroutine(ShakeObjectRotation());
            StartCoroutine(TriggerPedalTimer());
        }
    }

    private void ApplyPostProcessing()
    {
        if (bloom != null)
        {
            bloom.active = true;
            bloom.intensity.value = bloomIntensity;
        }

        if (chromaticAberration != null)
        {
            chromaticAberration.active = true;
            chromaticAberration.intensity.value = chromaticAberrationIntensity;
        }

        if (filmGrain != null)
        {
            filmGrain.active = true;
            filmGrain.intensity.value = filmGrainIntensity;
        }
    }

    private void PlayOpenSound()
    {
        if (openSoundSource != null && pedalSoundClip != null)
            openSoundSource.PlayOneShot(pedalSoundClip);
    }

    private IEnumerator TriggerPedalTimer()
    {
        yield return new WaitForSeconds(10f);
        jumpscare?.TriggerJumpscare();
    }

    private IEnumerator ShakeObjectRotation()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float angleOffset = Random.Range(-shakeMagnitude, shakeMagnitude);
            Vector3 rotation = originalRotation.eulerAngles;

            switch (shakeAxis)
            {
                case ShakeAxis.X:
                    rotation.x += angleOffset;
                    break;
                case ShakeAxis.Y:
                    rotation.y += angleOffset;
                    break;
                case ShakeAxis.Z:
                    rotation.z += angleOffset;
                    break;
            }

            objectToShake.transform.localRotation = Quaternion.Euler(rotation);
            elapsed += Time.deltaTime;
            yield return null;
        }

        objectToShake.transform.localRotation = originalRotation;
    }
}
