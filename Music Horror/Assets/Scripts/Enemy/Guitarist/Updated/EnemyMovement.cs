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
    [SerializeField] private Animator animator;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource breathingSource;
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip breathingClip;
    [SerializeField] private AudioClip footstepClip;

    [Header("Footstep")]
    [SerializeField] private float roamFootstepRate = 0.55f;
    [SerializeField] private float chaseFootstepRate = 0.35f;
    [SerializeField] private float roamFootstepPitch = 1.0f;
    [SerializeField] private float chaseFootstepPitch = 1.3f;

    [Header("Roam Behavior")]
    [SerializeField] private float minRoamPointWait = 1.0f;
    [SerializeField] private float maxRoamPointWait = 2.5f;

    private Coroutine roamCoroutine;
    private float stepTimer = 0f;

    public void Initialize(EnemySettings s, IEnemy owner)
    {
        settings = s;
        controller = owner as EnemyController;
        agent ??= GetComponent<NavMeshAgent>();

        if (breathingSource && breathingClip)
        {
            breathingSource.clip = breathingClip;
            breathingSource.loop = true;
            breathingSource.spatialBlend = 1f;
            breathingSource.rolloffMode = AudioRolloffMode.Logarithmic;
            breathingSource.Play();
        }

        if (footstepSource)
        {
            footstepSource.spatialBlend = 1f;
            footstepSource.rolloffMode = AudioRolloffMode.Logarithmic;
        }
    }

    public void Idle()
    {
        StopRoam();
        agent.isStopped = true;
        stepTimer = 0f;
    }

    public void Patrol()
    {
        StopRoam();
        agent.isStopped = false;
        agent.speed = settings.PatrolSpeed;

        if (footstepSource)
            footstepSource.pitch = roamFootstepPitch;
    }

    public void MoveTo(Vector3 pos)
    {
        if (!agent.isOnNavMesh) return;

        StopRoam();
        agent.isStopped = false;
        agent.speed = settings.ChaseSpeed;
        agent.SetDestination(pos);

        if (footstepSource)
            footstepSource.pitch = chaseFootstepPitch;
    }

    public void Chase(Transform t)
    {
        if (!agent.isOnNavMesh) return;

        StopRoam();
        agent.isStopped = false;
        agent.speed = settings.ChaseSpeed;
        agent.SetDestination(t.position);

        if (footstepSource)
            footstepSource.pitch = chaseFootstepPitch;
    }

    public void DisableMovement()
    {
        StopRoam();
        agent.isStopped = true;
        agent.enabled = false;
    }


    public IEnumerator RoamAroundPoint(Vector3 center, float duration)
    {
        StopRoam();
        yield return StartCoroutine(RoamRoutine(center, duration));
    }

    private IEnumerator RoamRoutine(Vector3 center, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector3 target;

            if (!EnemyUtilities.RandomNavSphere(center, settings.RoamRadius, out target))
            {
                yield return null;
                elapsed += Time.deltaTime;
                continue;
            }

            agent.isStopped = false;
            agent.SetDestination(target);

            float moveTimer = 0f;

            // Move toward roam point
            while (true)
            {
                if (elapsed >= duration)
                    break;

                if (agent.isStopped)
                    break;

                if (!agent.pathPending &&
                    agent.remainingDistance <= settings.PatrolPointTolerance)
                    break;

                moveTimer += Time.deltaTime;
                elapsed += Time.deltaTime;

                if (moveTimer > 10f) // safety timeout
                    break;

                yield return null;
            }

            if (elapsed >= duration)
                break;

            agent.isStopped = true;

            float waitTime = Random.Range(minRoamPointWait, maxRoamPointWait);
            float waitTimer = 0f;

            while (waitTimer < waitTime)
            {
                if (elapsed >= duration)
                    break;

                waitTimer += Time.deltaTime;
                elapsed += Time.deltaTime;

                yield return null;
            }
        }

        agent.isStopped = true;
        agent.ResetPath();

        roamCoroutine = null;
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
        HandleAnimations();
    }

    private void HandleAnimations()
    {
        bool walking =
            agent.enabled &&
            !agent.isStopped &&
            agent.velocity.sqrMagnitude > 0.1f;

        animator.SetBool("animIsWalking", walking);
    }

    private void HandleFootsteps()
    {
        if (agent.velocity.sqrMagnitude < 0.1f || agent.isStopped)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer += Time.deltaTime;

        float rate = agent.speed == settings.ChaseSpeed
            ? chaseFootstepRate
            : roamFootstepRate;

        if (stepTimer >= rate)
        {
            if (footstepClip)
                footstepSource.PlayOneShot(footstepClip);

            stepTimer = 0f;
        }
    }
}