using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class EnemyPatrol : MonoBehaviour
{
    [Tooltip("Assign in inspector a list of patrol waypoints (optional).")]
    [SerializeField] private List<Transform> patrolPoints = new List<Transform>();

    [Tooltip("If true, patrol loops. If false, it will ping-pong.")]
    [SerializeField] private bool loop = true;

    private int currentIndex = 0;
    private bool forward = true;
    public bool HasPatrol => patrolPoints != null && patrolPoints.Count > 0;

    private UnityEngine.AI.NavMeshAgent agent;
    private EnemySettings settings;

    private void Awake()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        var movement = GetComponent<EnemyMovement>();
        // movement will initialize settings; but for patience, we fetch them from movement if available
        settings = movement != null ? (movement as EnemyMovement)?.GetType().GetField("settings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(movement) as EnemySettings : null;
    }

    private void Update()
    {
        if (!HasPatrol || agent == null || !agent.isOnNavMesh) return;
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            AdvanceToNext();
        }
    }

    public void AdvanceToNext()
    {
        if (!HasPatrol) return;
        currentIndex = GetNextIndex();
        agent.SetDestination(patrolPoints[currentIndex].position);
    }

    private int GetNextIndex()
    {
        if (loop)
        {
            return (currentIndex + 1) % patrolPoints.Count;
        }
        else
        {
            if (forward)
            {
                if (currentIndex + 1 >= patrolPoints.Count) { forward = false; return currentIndex - 1; }
                return currentIndex + 1;
            }
            else
            {
                if (currentIndex - 1 < 0) { forward = true; return 1; }
                return currentIndex - 1;
            }
        }
    }
}
