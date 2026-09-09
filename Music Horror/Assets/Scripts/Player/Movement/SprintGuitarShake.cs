using UnityEngine;

public class SprintGuitarShake : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FirstPersonRigidbodyController playerController;

    [Header("Sprint Shake")]
    [SerializeField] private float positionAmount = 0.025f;
    [SerializeField] private float rotationAmount = 2f;
    [SerializeField] private float shakeSpeed = 12f;
    [SerializeField] private float smoothSpeed = 10f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float shakeTime;

    void Awake()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;

        if (playerController == null)
            playerController = GetComponentInParent<FirstPersonRigidbodyController>();
    }

    void Update()
    {
        if (playerController == null)
            return;

        if (playerController.isSprinting)
        {
            shakeTime += Time.deltaTime * shakeSpeed;

            float vertical = Mathf.Sin(shakeTime) * positionAmount;
            float horizontal = Mathf.Cos(shakeTime * 0.5f) * positionAmount * 0.5f;

            Vector3 targetPosition = initialPosition +
                                     new Vector3(horizontal, vertical, 0f);

            float rotation = Mathf.Sin(shakeTime * 0.5f) * rotationAmount;

            Quaternion targetRotation =
                initialRotation * Quaternion.Euler(0f, 0f, rotation);

            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                targetPosition,
                Time.deltaTime * smoothSpeed
            );

            transform.localRotation = Quaternion.Lerp(
                transform.localRotation,
                targetRotation,
                Time.deltaTime * smoothSpeed
            );
        }
        else
        {
            shakeTime = 0f;

            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                initialPosition,
                Time.deltaTime * smoothSpeed
            );

            transform.localRotation = Quaternion.Lerp(
                transform.localRotation,
                initialRotation,
                Time.deltaTime * smoothSpeed
            );
        }
    }
}