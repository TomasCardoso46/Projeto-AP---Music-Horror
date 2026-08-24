using UnityEngine;

public class ChandelierSwing : MonoBehaviour
{
    [Header("Swing Point")]
    [Tooltip("The transform the chandelier swings around.")]
    [SerializeField] private Transform swingPoint;

    [Header("Swing")]
    [Tooltip("Maximum angle of the swing.")]
    [SerializeField] private float swingAngle = 8f;

    [Tooltip("How many complete swings happen per second.")]
    [SerializeField] private float swingSpeed = 0.15f;

    [Tooltip("Axis the chandelier swings around.")]
    [SerializeField] private Vector3 swingAxis = Vector3.forward;

    [Header("Starting Position")]
    [Tooltip("Starting point in the swing cycle.")]
    [Range(0f, 1f)]
    [SerializeField] private float startingPhase = 0f;

    private float time;

    private void Start()
    {
        if (swingPoint == null)
        {
            Debug.LogError($"{name}: No Swing Point assigned.");
            enabled = false;
            return;
        }

        // Put the chandelier at the swing point position.
        transform.position = swingPoint.position;

        time = startingPhase;
    }

    private void Update()
    {
        // Advance the swing.
        time += Time.deltaTime * swingSpeed;

        // Smooth sine wave between -1 and 1.
        float swing = Mathf.Sin(time * Mathf.PI * 2f);

        float angle = swing * swingAngle;

        // Rotate around the chosen local axis of the swing point.
        Quaternion rotation =
            swingPoint.rotation *
            Quaternion.AngleAxis(angle, swingAxis.normalized);

        transform.rotation = rotation;
    }
}