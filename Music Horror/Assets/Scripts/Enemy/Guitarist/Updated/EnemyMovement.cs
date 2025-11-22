using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private EnemySettings settings;
    [SerializeField] private EnemyController controller;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource breathingSource;
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip breathingClip;
    [SerializeField] private AudioClip footstepClip;

    [Header("Footstep Settings")]
    [SerializeField] private float roamFootstepInterval = 0.6f;
    [SerializeField] private float chaseFootstepInterval = 0.3f;

    private Vector3 spawnPosition;
    private Coroutine roamCoroutine;
    private Coroutine footstepCoroutine;

    public void Initialize(EnemySettings s, IEnemy owner)
    {
        settings = s;
        controller = owner as EnemyController;
        agent ??= GetComponent<NavMeshAgent>();

        spawnPosition = transform.position;

        // Breathing setup
        if (breathingSource != null && breathingClip != null)
        {
            breathingSource.clip = breathingClip;
            breathingSource.loop = true;
            breathingSource.spatialBlend = 1f; // 3D sound
            breathingSource.rolloffMode = AudioRolloffMode.Logarithmic;
            breathingSource.minDistance = 2f;
            breathingSource.maxDistance = 20f;
            breathingSource.Play();
        }

        // Footsteps setup
        if (footstepSource != null)
        {
            footstepSource.spatialBlend = 1f;
            footstepSource.rolloffMode = AudioRolloffMode.Logarithmic;
            footstepSource.minDistance = 1f;
            footstepSource.maxDistance = 25f;
        }
    }

    public void Idle()
    {
        agent.isStopped = true;
        StopFootsteps();
    }

    public void Patrol()
    {
        agent.isStopped = false;
        agent.speed = settings.PatrolSpeed;

        var p = GetComponent<EnemyPatrol>();
        if (p != null && p.HasPatrol) return;

        if (settings.RandomRoam && roamCoroutine == null)
            roamCoroutine = StartCoroutine(RandomRoam());

        StartFootsteps(roamFootstepInterval);
    }

    public void MoveTo(Vector3 worldPos)
    {
        if (!agent.isOnNavMesh) return;
        StopRoam();
        agent.isStopped = false;
        agent.speed = settings.ChaseSpeed;
        agent.SetDestination(worldPos);

        StartFootsteps(chaseFootstepInterval);
    }

    public void Chase(Transform t)
    {
        if (!agent.isOnNavMesh) return;
        StopRoam();
        agent.isStopped = false;
        agent.speed = settings.ChaseSpeed;
        agent.SetDestination(t.position);

        StartFootsteps(chaseFootstepInterval);
    }

    public void DisableMovement()
    {
        StopRoam();
        StopFootsteps();
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

    // FOOTSTEP LOGIC
    private void StartFootsteps(float interval)
    {
        StopFootsteps(); // make sure only one coroutine runs
        if (footstepSource != null && footstepClip != null)
            footstepCoroutine = StartCoroutine(Footsteps(interval));
    }

    private void StopFootsteps()
    {
        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }
    }

    private IEnumerator Footsteps(float interval)
    {
        while (true)
        {
            if (agent.velocity.magnitude > 0.1f)
            {
                footstepSource.PlayOneShot(footstepClip);
            }
            else
            {
                // Stop coroutine immediately if agent stops
                StopFootsteps();
                yield break;
            }

            yield return new WaitForSeconds(interval / (agent.speed / settings.PatrolSpeed));
        }
    }
}
