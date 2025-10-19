using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private EnemySettings settings;
    [SerializeField] private EnemyController controller;

    private Vector3 spawnPosition;
    private Coroutine roamCoroutine;

    public void Initialize(EnemySettings s, IEnemy owner)
    {
        settings = s;
        controller = owner as EnemyController;
        agent ??= GetComponent<NavMeshAgent>();
        agent.stoppingDistance = settings.StoppingDistance;
        spawnPosition = transform.position;
    }

    public void Idle()
    {
        agent.isStopped = true;
    }

    public void Patrol()
    {
        agent.isStopped = false;
        agent.speed = settings.PatrolSpeed;
        // Patrol logic is delegated to EnemyPatrol if present
        var p = GetComponent<EnemyPatrol>();
        if (p != null && p.HasPatrol) return; // patrol handles moving the agent
        // fallback random roam
        if (settings.RandomRoam)
        {
            if (roamCoroutine == null)
                roamCoroutine = StartCoroutine(RandomRoam());
        }
    }

    public void MoveTo(Vector3 worldPos)
    {
        if (!agent.isOnNavMesh) return;
        StopRoam();
        agent.isStopped = false;
        agent.speed = settings.ChaseSpeed;
        agent.SetDestination(worldPos);
    }

    public void Chase(Transform t)
    {
        if (!agent.isOnNavMesh) return;
        StopRoam();
        agent.isStopped = false;
        agent.speed = settings.ChaseSpeed;
        agent.SetDestination(t.position);
    }

    public void DisableMovement()
    {
        StopRoam();
        agent.isStopped = true;
        agent.enabled = false;
    }

    private IEnumerator RandomRoam()
    {
        while (true)
        {
            Vector3 target = spawnPosition + Random.insideUnitSphere * settings.RoamRadius;
            if (NavMesh.SamplePosition(target, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                // wait until we reach within stopping distance or timeout
                float timer = 0f;
                while (Vector3.Distance(transform.position, hit.position) > settings.PatrolPointTolerance && timer < 12f)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
            yield return new WaitForSeconds(Random.Range(1.5f, 4f));
        }
    }

    private void StopRoam()
    {
        if (roamCoroutine != null)
        {
            StopCoroutine(roamCoroutine);
            roamCoroutine = null;
        }
    }
}
