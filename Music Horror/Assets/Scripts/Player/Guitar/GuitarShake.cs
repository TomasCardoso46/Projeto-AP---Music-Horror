using UnityEngine;

public class GuitarShake : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Chord chord;

    [Header("Position Shake")]
    [SerializeField] private float positionAmount = 0.08f;
    [SerializeField] private float positionRandomness = 0.5f;

    [Header("Rotation Shake")]
    [SerializeField] private float rotationAmount = 8f;
    [SerializeField] private float rotationRandomness = 0.5f;

    [Header("Shake")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float durationRandomness = 0.25f;
    [SerializeField] private float returnSpeed = 12f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private float shakeTimer;
    private float currentShakeDuration;

    private Vector3 randomPositionDirection;
    private Vector3 randomRotationDirection;
    private float randomStrength;

    private void Awake()
    {
        if (chord == null)
            chord = GetComponentInParent<Chord>();

        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        if (chord != null)
            chord.OnChordPlayed += PlayChordShake;
    }

    private void OnDisable()
    {
        if (chord != null)
            chord.OnChordPlayed -= PlayChordShake;
    }

    private void Update()
    {
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;

            float progress = 1f - (shakeTimer / currentShakeDuration);
            float strength = Mathf.Sin(progress * Mathf.PI) * randomStrength;

            Vector3 positionOffset =
                randomPositionDirection *
                positionAmount *
                strength;

            Vector3 rotationOffset =
                randomRotationDirection *
                rotationAmount *
                strength;

            transform.localPosition = initialPosition + positionOffset;

            transform.localRotation =
                initialRotation *
                Quaternion.Euler(rotationOffset);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                initialPosition,
                Time.deltaTime * returnSpeed
            );

            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                initialRotation,
                Time.deltaTime * returnSpeed
            );
        }
    }

    private void PlayChordShake()
    {
        currentShakeDuration = shakeDuration *
                                Random.Range(
                                    1f - durationRandomness,
                                    1f + durationRandomness
                                );

        shakeTimer = currentShakeDuration;

        randomStrength = Random.Range(
            1f - positionRandomness,
            1f + positionRandomness
        );

        randomPositionDirection = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(0.5f, 1f),
            Random.Range(-1f, 0f)
        ).normalized;

        randomRotationDirection = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;
    }
}