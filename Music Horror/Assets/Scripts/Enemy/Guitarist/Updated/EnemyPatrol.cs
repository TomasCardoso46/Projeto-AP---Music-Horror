using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Routes")]
    [Tooltip("Each element is a full patrol route (list of waypoints).")]
    [SerializeField] private List<List<Transform>> patrolRoutes = new List<List<Transform>>();

    [Tooltip("Inspector helper: Unity does NOT serialize nested lists well.")]
    [SerializeField] private List<PatrolRouteWrapper> inspectorRoutes = new List<PatrolRouteWrapper>();

    [Header("Roam Settings")]
    [SerializeField] private float minRoamTime = 2f;
    [SerializeField] private float maxRoamTime = 5f;

    private List<Transform> activeRoute;
    private int currentIndex = 0;
    private bool forward = true;
    private bool isRoaming = false;

    private UnityEngine.AI.NavMeshAgent agent;
    private EnemyMovement movement;

    public bool HasPatrol => activeRoute != null && activeRoute.Count > 0;

    private void Awake()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        movement = GetComponent<EnemyMovement>();
        patrolRoutes = new List<List<Transform>>();
        foreach (var route in inspectorRoutes)
        {
            patrolRoutes.Add(route.points);
        }

        if (patrolRoutes.Count > 0)
            activeRoute = patrolRoutes[0];
    }

    private void Start()
    {
        if (HasPatrol)
        {
            agent.SetDestination(activeRoute[currentIndex].position);
        }
    }

    private void Update()
    {
        if (!HasPatrol || agent == null || !agent.isOnNavMesh || isRoaming)
            return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(HandlePatrolPoint());
        }
    }

    private IEnumerator HandlePatrolPoint()
    {
        isRoaming = true;

        agent.isStopped = true;
        agent.ResetPath();

        float roamTime = Random.Range(minRoamTime, maxRoamTime);

        if (movement != null && roamTime > 0f)
        {
            yield return movement.RoamAroundPoint(
                activeRoute[currentIndex].position,
                roamTime
            );
        }

        AdvanceToNext();

        agent.isStopped = false;

        isRoaming = false;
    }

    public void AdvanceToNext()
    {
        if (!HasPatrol) return;

        currentIndex = GetNextIndex();

        agent.isStopped = false;
        agent.SetDestination(activeRoute[currentIndex].position);
    }

    private int GetNextIndex()
    {
        if (forward)
        {
            if (currentIndex + 1 >= activeRoute.Count)
            {
                forward = false;
                return Mathf.Max(0, currentIndex - 1);
            }
            return currentIndex + 1;
        }
        else
        {
            if (currentIndex - 1 < 0)
            {
                forward = true;
                return 1;
            }
            return currentIndex - 1;
        }
    }

    public void SwitchPatrol(int index)
    {
        if (patrolRoutes == null || patrolRoutes.Count == 0)
        {
            Debug.LogWarning("No patrol routes defined.");
            return;
        }

        if (index < 0 || index >= patrolRoutes.Count)
        {
            Debug.LogWarning($"Invalid patrol index: {index}");
            return;
        }

        StopAllCoroutines();

        activeRoute = patrolRoutes[index];

        currentIndex = 0;
        forward = true;
        isRoaming = false;

        agent.isStopped = false;
        agent.ResetPath();

        if (HasPatrol)
        {
            agent.SetDestination(activeRoute[currentIndex].position);
        }
    }

    [System.Serializable]
    public class PatrolRouteWrapper
    {
        public List<Transform> points = new List<Transform>();
    }
}