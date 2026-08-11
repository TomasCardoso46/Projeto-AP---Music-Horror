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

    [Header("Footstep Audio")]
    [SerializeField] private AudioClip[] roamFootstepClips;
    [SerializeField] private AudioClip[] chaseFootstepClips;

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

    private int lastRoamFootstepIndex = -1;
    private int lastChaseFootstepIndex = -1;

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

                if (moveTimer > 10f)
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

        bool chasing = Mathf.Approximately(agent.speed, settings.ChaseSpeed);

        float rate = chasing
            ? chaseFootstepRate
            : roamFootstepRate;

        if (stepTimer >= rate)
        {
            if (chasing)
                PlayRandomChaseFootstep();
            else
                PlayRandomRoamFootstep();

            stepTimer = 0f;
        }
    }

    private void PlayRandomRoamFootstep()
    {
        if (footstepSource == null || roamFootstepClips == null || roamFootstepClips.Length == 0)
            return;

        int index;

        if (roamFootstepClips.Length == 1)
        {
            index = 0;
        }
        else
        {
            do
            {
                index = Random.Range(0, roamFootstepClips.Length);
            }
            while (index == lastRoamFootstepIndex);
        }

        lastRoamFootstepIndex = index;
        footstepSource.PlayOneShot(roamFootstepClips[index]);
    }

    private void PlayRandomChaseFootstep()
    {
        if (footstepSource == null || chaseFootstepClips == null || chaseFootstepClips.Length == 0)
            return;

        int index;

        if (chaseFootstepClips.Length == 1)
        {
            index = 0;
        }
        else
        {
            do
            {
                index = Random.Range(0, chaseFootstepClips.Length);
            }
            while (index == lastChaseFootstepIndex);
        }

        lastChaseFootstepIndex = index;
        footstepSource.PlayOneShot(chaseFootstepClips[index]);
    }

    public IEnumerator ForceMoveTo(Vector3 position)
    {
        if (!agent.enabled)
            agent.enabled = true;

        if (!agent.isOnNavMesh)
            yield break;

        agent.isStopped = false;
        agent.speed = settings.ChaseSpeed;
        agent.SetDestination(position);

        if (footstepSource)
            footstepSource.pitch = chaseFootstepPitch;

        while (agent.enabled)
        {
            if (!agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance + 0.1f)
            {
                break;
            }
            gameObject.SetActive(false);
            yield return null;
        }

        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        
    }
}