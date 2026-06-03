using UnityEngine;

[DisallowMultipleComponent]
public class Muffle : MonoBehaviour
{
    private AudioSource[] audioSources;
    private AudioLowPassFilter lowPass;

    private float[] originalVolumes;

    private float targetOcclusion;

    private float minVolumeMultiplier;
    private float clearCutoff;
    private float muffledCutoff;

    [SerializeField] private float transitionSpeed = 8f;

    private void Awake()
    {
        audioSources = GetComponents<AudioSource>();

        if (audioSources.Length == 0)
        {
            enabled = false;
            return;
        }

        lowPass = GetComponent<AudioLowPassFilter>();
        if (lowPass == null)
            lowPass = gameObject.AddComponent<AudioLowPassFilter>();

        originalVolumes = new float[audioSources.Length];

        for (int i = 0; i < audioSources.Length; i++)
            originalVolumes[i] = audioSources[i].volume;

        lowPass.cutoffFrequency = 22000f;
    }

    public void ApplyMuffleSettings(
        float occlusion,
        float minVol,
        float clearFreq,
        float muffledFreq)
    {
        targetOcclusion = occlusion;
        minVolumeMultiplier = minVol;
        clearCutoff = clearFreq;
        muffledCutoff = muffledFreq;
    }

    private void Update()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            float targetVol = Mathf.Lerp(
                originalVolumes[i],
                originalVolumes[i] * minVolumeMultiplier,
                targetOcclusion);

            audioSources[i].volume = Mathf.Lerp(
                audioSources[i].volume,
                targetVol,
                Time.deltaTime * transitionSpeed);
        }

        float targetCutoff = Mathf.Lerp(
            clearCutoff,
            muffledCutoff,
            targetOcclusion);

        lowPass.cutoffFrequency = Mathf.Lerp(
            lowPass.cutoffFrequency,
            targetCutoff,
            Time.deltaTime * transitionSpeed);
    }
}