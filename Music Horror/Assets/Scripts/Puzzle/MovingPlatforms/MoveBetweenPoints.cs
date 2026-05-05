using UnityEngine;

public class MoveBetweenPoints : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float moveSpeed = 3f;

    [Header("Identification")]
    [SerializeField] private int id;

    private bool moveObject;
    private Transform targetPoint;

    public int ID => id;

    private void Start()
    {
        float distanceToA = Vector3.Distance(transform.position, pointA.position);
        float distanceToB = Vector3.Distance(transform.position, pointB.position);

        targetPoint = distanceToA < distanceToB ? pointB : pointA;
    }

    private void Update()
    {
        if (!moveObject)
            return;

        Move();
    }

    private void Move()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.01f)
        {
            targetPoint = targetPoint == pointA ? pointB : pointA;
            moveObject = false;
        }
    }

    public void ToggleMove()
    {
        moveObject = true;
    }
}
