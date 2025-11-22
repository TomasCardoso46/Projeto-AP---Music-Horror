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

    [Header("Footstep Sound Speeds")]
    [SerializeField] private float roamFootstepRate = 0.55f;   // seconds per step
    [SerializeField] private float chaseFootstepRate = 0.35f;  // faster cadence
    [SerializeField] private float roamFootstepPitch = 1.0f;
    [SerializeField] private float chaseFootstepPitch = 1.3f;

    private Vector3 spawnPosition;
    private Coroutine roamCoroutine;
    private float stepTimer = 0f;

    public void Initialize(EnemySettings s, IEnemy owner)
    {
        settings = s;
        controller = owner as EnemyController;
        agent ??= GetComponent<NavMeshAgent>();

        spawnPosition = transform.position;

        // ========= breathing audio setup =========
        if (breathingSource != null && breathingClip != null)
        {
            breathingSource.clip = breathingClip;
            breathingSource.loop = true;

            // 3D sound
            breathingSource.spatialBlend = 1f;
            breathingSource.rolloffMode = AudioRolloffMode.Logarithmic;
            breathingSource.Play();
        }

        // ========= footstep audio setup =========
        if (footstepSource != null)
        {
            footstepSource.spatialBlend = 1f;
            footstepSource.rolloffMode = AudioRolloffMode.Logarithmic;
        }
    }

    public void Idle()
    {
        agent.isStopped = true;
        stepTimer = 0f;
    }

    public void Patrol()
    {
        agent.isStopped = false;
        agent.speed = settings.PatrolSpeed;

        var p = GetComponent<EnemyPatrol>();
        if (p != null && p.HasPatrol) return;

        if (settings.RandomRoam && roamCoroutine == null)
            roamCoroutine = StartCoroutine(RandomRoam());

        // Switch to roaming footstep pitch
        footstepSource.pitch = roamFootstepPitch;
    }

    public void MoveTo(Vector3 worldPos)
    {
        if (!agent.isOnNavMesh) return;

        StopRoam();
        agent.isStopped = false;
        agent.speed = settings.ChaseSpeed;
        agent.SetDestination(worldPos);

        // Switch to chasing footstep pitch
        footstepSource.pitch = chaseFootstepPitch;
    }

    public void Chase(Transform t)
    {
        if (!agent.isOnNavMesh) return;

        StopRoam();
        agent.isStopped = false;
        agent.speed = settings.ChaseSpeed;
        agent.SetDestination(t.position);

        footstepSource.pitch = chaseFootstepPitch;
    }

    public void DisableMovement()
    {
        StopRoam();
        agent.isStopped = true;
        agent.enabled = false;
        stepTimer = 0f;
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

                while (Vector3.Distance(transform.position, hit.position) > settings.PatrolPointTolerance &&
                       timer < 12f)
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

    private void Update()
    {
        HandleFootsteps();
    }

    // ===========================================================
    // FOOTSTEP TIMING SYSTEM
    // ===========================================================
    private void HandleFootsteps()
    {
        // If not moving → reset timer and stop
        if (agent.velocity.sqrMagnitude < 0.1f || agent.isStopped)
        {
            stepTimer = 0f;
            return;
        }

        // Add time
        stepTimer += Time.deltaTime;

        // Pick the correct cadence
        float stepRate =
            agent.speed == settings.ChaseSpeed ? chaseFootstepRate : roamFootstepRate;

        // Time for next step?
        if (stepTimer >= stepRate)
        {
            if (footstepClip != null)
                footstepSource.PlayOneShot(footstepClip);

            stepTimer = 0f;
        }
    }
}
