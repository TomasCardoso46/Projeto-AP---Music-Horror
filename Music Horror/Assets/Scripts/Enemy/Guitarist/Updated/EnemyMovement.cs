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

    [Header("Animation Speed")]
    [SerializeField] private float animationReferenceSpeed = 2.0f;
    [SerializeField] private float minimumAnimationSpeed = 0.0f;
    [SerializeField] private float maximumAnimationSpeed = 3.0f;
    [SerializeField] private float animationSpeedSmoothTime = 0.08f;
    [SerializeField] private float idleAnimationSpeed = 1.0f;

    [Header("Rotation Settings")]
    [SerializeField] private float turnSnapAngle = 70f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource breathingSource;
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip breathingClip;

    [Header("Footstep Audio")]
    [SerializeField] private AudioClip[] roamFootstepClips;
    [SerializeField] private AudioClip[] chaseFootstepClips;

    [Header("Footstep Pitch")]
    [SerializeField] private float roamFootstepPitch = 1.0f;
    [SerializeField] private float chaseFootstepPitch = 1.3f;

    [Header("Roam Behavior")]
    [SerializeField] private float minRoamPointWait = 1.0f;
    [SerializeField] private float maxRoamPointWait = 2.5f;

    private Coroutine roamCoroutine;

    private int lastRoamFootstepIndex = -1;
    private int lastChaseFootstepIndex = -1;

    private float currentAnimationSpeed;
    private float animationSpeedVelocity;

    public void Initialize(EnemySettings s, IEnemy owner)
    {
        settings = s;
        controller = owner as EnemyController;
        agent ??= GetComponent<NavMeshAgent>();

        agent.updateRotation = true;

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

        if (animator)
        {
            animator.speed = idleAnimationSpeed;
            currentAnimationSpeed = idleAnimationSpeed;
        }
    }

    public void Idle()
    {
        StopRoam();

        agent.isStopped = true;
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
        if (!agent.isOnNavMesh)
            return;

        StopRoam();

        agent.isStopped = false;
        agent.speed = settings.ChaseSpeed;
        agent.SetDestination(pos);

        if (footstepSource)
            footstepSource.pitch = chaseFootstepPitch;
    }

    public void Chase(Transform t)
    {
        if (!agent.isOnNavMesh)
            return;

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

            if (!EnemyUtilities.RandomNavSphere(
                    center,
                    settings.RoamRadius,
                    out target))
            {
                yield return null;
                elapsed += Time.deltaTime;
                continue;
            }

            agent.isStopped = false;
            agent.speed = settings.PatrolSpeed;
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

            float waitTime = Random.Range(
                minRoamPointWait,
                maxRoamPointWait
            );

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
        HandleRotationSnap();
        HandleAnimations();
        HandleAnimationSpeed();
    }

    private void HandleAnimationSpeed()
    {
        if (animator == null || !agent.enabled)
            return;

        bool moving = agent.velocity.sqrMagnitude > 0.001f;

        if (!moving)
        {
            currentAnimationSpeed = Mathf.SmoothDamp(
                currentAnimationSpeed,
                idleAnimationSpeed,
                ref animationSpeedVelocity,
                animationSpeedSmoothTime
            );

            animator.speed = currentAnimationSpeed;
            return;
        }

        float actualSpeed = agent.velocity.magnitude;

        float targetAnimationSpeed = actualSpeed / animationReferenceSpeed;

        targetAnimationSpeed = Mathf.Clamp(
            targetAnimationSpeed,
            minimumAnimationSpeed,
            maximumAnimationSpeed
        );

        currentAnimationSpeed = Mathf.SmoothDamp(
            currentAnimationSpeed,
            targetAnimationSpeed,
            ref animationSpeedVelocity,
            animationSpeedSmoothTime
        );

        animator.speed = currentAnimationSpeed;
    }

    private void HandleRotationSnap()
    {
        if (!agent.enabled)
            return;

        if (agent.isStopped)
            return;

        if (agent.desiredVelocity.sqrMagnitude < 0.01f)
            return;

        Vector3 desiredDirection = agent.desiredVelocity;
        desiredDirection.y = 0f;

        if (desiredDirection.sqrMagnitude < 0.01f)
            return;

        desiredDirection.Normalize();

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        float angle = Vector3.Angle(
            forward,
            desiredDirection
        );

        if (angle >= turnSnapAngle)
        {
            transform.rotation = Quaternion.LookRotation(
                desiredDirection,
                Vector3.up
            );
        }
    }

    private void HandleAnimations()
    {
        bool walking =
            agent.enabled &&
            !agent.isStopped &&
            agent.velocity.sqrMagnitude > 0.1f;

        if (animator)
            animator.SetBool("animIsWalking", walking);
    }

    public void PlayRoamFootstep()
    {
        if (footstepSource == null ||
            roamFootstepClips == null ||
            roamFootstepClips.Length == 0)
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
                index = Random.Range(
                    0,
                    roamFootstepClips.Length
                );
            }
            while (index == lastRoamFootstepIndex);
        }

        lastRoamFootstepIndex = index;

        footstepSource.pitch = roamFootstepPitch;

        footstepSource.PlayOneShot(
            roamFootstepClips[index]
        );
    }

    public void PlayChaseFootstep()
    {
        if (footstepSource == null ||
            chaseFootstepClips == null ||
            chaseFootstepClips.Length == 0)
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
                index = Random.Range(
                    0,
                    chaseFootstepClips.Length
                );
            }
            while (index == lastChaseFootstepIndex);
        }

        lastChaseFootstepIndex = index;

        footstepSource.pitch = chaseFootstepPitch;

        footstepSource.PlayOneShot(
            chaseFootstepClips[index]
        );
    }

    public void PlayFootstep()
    {
        if (controller == null)
        {
            PlayRoamFootstep();
            return;
        }

        switch (controller.currentState)
        {
            case EnemyController.State.Investigate:
            case EnemyController.State.Chase:
                PlayChaseFootstep();
                break;

            default:
                PlayRoamFootstep();
                break;
        }
    }
}