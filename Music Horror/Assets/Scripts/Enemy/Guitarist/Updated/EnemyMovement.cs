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

    [Header("Roam Timing")]
    [SerializeField] private float minRoamWaitTime = 1.5f;
    [SerializeField] private float maxRoamWaitTime = 4f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource breathingSource;
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip breathingClip;
    [SerializeField] private AudioClip footstepClip;

    [Header("Footstep Sound Speeds")]
    [SerializeField] private float roamFootstepRate = 0.55f;
    [SerializeField] private float chaseFootstepRate = 0.35f;
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

        if (breathingSource != null && breathingClip != null)
        {
            breathingSource.clip = breathingClip;
            breathingSource.loop = true;
            breathingSource.spatialBlend = 1f;
            breathingSource.rolloffMode = AudioRolloffMode.Logarithmic;
            breathingSource.Play();
        }

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

        // Start random roaming if enabled
        if (settings.RandomRoam && roamCoroutine == null)
        {
            roamCoroutine = StartCoroutine(RandomRoam());
        }

        footstepSource.pitch = roamFootstepPitch;
    }

    public void MoveTo(Vector3 worldPos)
    {
        if (!agent.isOnNavMesh) return;

        StopRoam();
        agent.isStopped = false;
        agent.speed = settings.ChaseSpeed;
        agent.SetDestination(worldPos);

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
            Vector3 target = spawnPosition; // safe default
            int attempts = 0;

            // Try up to 10 times to find a valid NavMesh point
            while (attempts < 10)
            {
                if (EnemyUtilities.RandomNavSphere(spawnPosition, settings.RoamRadius, out target))
                {
                    break; // valid point found
                }
                attempts++;
            }

            // If no valid point found after 10 attempts, skip this iteration
            if (attempts == 10)
            {
                Debug.LogWarning("EnemyMovement: Could not find valid roam point!");
                yield return new WaitForSeconds(1f);
                continue;
            }

            // Move to the target
            agent.SetDestination(target);

            // Timeout handling
            float timer = 0f;
            float maxTime = 45f; // max seconds to reach the target

            while (Vector3.Distance(transform.position, target) > settings.PatrolPointTolerance && timer < maxTime)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            // Pick a new point if timeout reached
            if (timer >= maxTime)
            {
                Debug.Log("EnemyMovement: Timeout reached, picking a new roam point.");
            }

            // Wait a customizable random time before picking next point
            float wait = Random.Range(minRoamWaitTime, maxRoamWaitTime);
            yield return new WaitForSeconds(wait);
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

    private void HandleFootsteps()
    {
        if (agent.velocity.sqrMagnitude < 0.1f || agent.isStopped)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer += Time.deltaTime;

        float stepRate = agent.speed == settings.ChaseSpeed ? chaseFootstepRate : roamFootstepRate;

        if (stepTimer >= stepRate)
        {
            if (footstepClip != null)
                footstepSource.PlayOneShot(footstepClip);

            stepTimer = 0f;
        }
    }
}
