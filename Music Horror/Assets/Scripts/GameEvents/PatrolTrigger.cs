using UnityEngine;

public class PatrolTrigger : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private int patrolIndex;

    [Header("Target Script")]
    [SerializeField] private EnemyPatrol target;

    //private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        //if (hasTriggered) return;

        // Check if the entering object is the player
        if (other.GetComponent<FirstPersonRigidbodyController>() == null)
            return;

        //hasTriggered = true;

        target.SwitchPatrol(patrolIndex);
    }
}