using UnityEngine;

public class VisuaGuitarMode : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Chord chord;

    [Header("Positions")]
    [SerializeField] private Vector3 normalPosition;
    [SerializeField] private Vector3 loweredPosition;

    [Header("Transition")]
    [SerializeField] private float moveSpeed = 10f;

    private void Awake()
    {
        if (chord == null)
            chord = GetComponentInParent<Chord>();

        normalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (chord == null)
            return;

        Vector3 targetPosition = chord.GuitarMode
            ? normalPosition
            : loweredPosition;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * moveSpeed
        );
    }
}

