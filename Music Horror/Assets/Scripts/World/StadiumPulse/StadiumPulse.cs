using UnityEngine;

public class StadiumPulse : MonoBehaviour
{
    [Header("Audio Sampling")]
    public int sampleDataLength = 1024;
    public float sizeFactor = 1f;

    [Header("Smoothing")]
    public float attackSpeed = 8f;
    public float releaseSpeed = 2f;

    [Header("Pulse Limits")]
    [Tooltip("Minimum X size of the sprite")]
    public float minPulse = 0.1f;

    [Tooltip("Maximum X size of the sprite")]
    public float maxPulse = 5f;

    [Tooltip("Optional curve to shape response")]
    public AnimationCurve pulseCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;

    private float[] sampleData;
    private float currentLoudness;
    private float smoothedLoudness;

    private void Awake()
    {
        sampleData = new float[sampleDataLength];
        spriteRenderer.drawMode = SpriteDrawMode.Tiled;
    }

    private void Update()
    {
        // Get final mixed audio
        AudioListener.GetOutputData(sampleData, 0);

        currentLoudness = 0f;
        for (int i = 0; i < sampleData.Length; i++)
        {
            currentLoudness += Mathf.Abs(sampleData[i]);
        }

        currentLoudness /= sampleData.Length;
        currentLoudness *= sizeFactor;

        // Smooth attack / release
        float speed = currentLoudness > smoothedLoudness
            ? attackSpeed
            : releaseSpeed;

        smoothedLoudness = Mathf.Lerp(
            smoothedLoudness,
            currentLoudness,
            Time.deltaTime * speed
        );

        // Normalize to 0–1 range
        float normalized = Mathf.InverseLerp(0f, maxPulse, smoothedLoudness);

        // Apply curve shaping
        float curved = pulseCurve.Evaluate(normalized);

        // Clamp to limits
        float finalPulse = Mathf.Lerp(minPulse, maxPulse, curved);

        spriteRenderer.size = new Vector2(
            finalPulse,
            spriteRenderer.size.y
        );
    }
}
