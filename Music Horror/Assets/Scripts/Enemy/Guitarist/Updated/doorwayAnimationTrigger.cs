using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class doorwayAnimationTrigger : MonoBehaviour
{
    [Header("Doorway")]
    [SerializeField] private Transform startPointA;
    [SerializeField] private Transform startPointB;

    [Header("Animation")]
    [SerializeField] private string doorwayAnimationName = "Traverse";
    [SerializeField] private string walkingAnimationName = "GuitaristWalk";
    [SerializeField] private string attackAnimationName = "Attack";
    [SerializeField] private int animationLayer = 0;
    [SerializeField] private float animationCrossFadeTime = 0.05f;

    [Header("Start Position")]
    [SerializeField] private float snapDuration = 0.2f;

    [Header("Cooldown")]
    [SerializeField] private float doorwayCooldown = 3f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private bool animationPlaying;
    private bool cooldownActive;

    private Collider triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (animationPlaying || cooldownActive)
            return;

        EnemyController enemy = other.GetComponentInParent<EnemyController>();

        if (enemy == null)
            return;

        if (!enemy.IsAlive)
            return;

        if (enemy.currentState == EnemyController.State.Attack)
        {
            Log("Enemy is attacking. Doorway animation ignored.");
            return;
        }

        Transform enemyTransform = enemy.transform;

        if (!IsEnemyMovingThroughDoorway(enemyTransform))
            return;

        StartCoroutine(PlayDoorwayAnimation(enemy));
    }

    private bool IsEnemyMovingThroughDoorway(Transform enemy)
    {
        if (startPointA == null && startPointB == null)
            return false;

        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

        if (agent == null)
            return true;

        if (!agent.enabled)
            return false;

        if (agent.velocity.sqrMagnitude <= 0.01f)
            return false;

        Vector3 movementDirection = agent.velocity.normalized;

        Vector3 toDoorway = transform.position - enemy.position;
        toDoorway.y = 0f;

        if (toDoorway.sqrMagnitude <= 0.01f)
            return true;

        toDoorway.Normalize();

        return Vector3.Dot(movementDirection, toDoorway) > 0f;
    }

    private IEnumerator PlayDoorwayAnimation(EnemyController enemy)
    {
        animationPlaying = true;

        Transform enemyTransform = enemy.transform;

        Animator animator = FindEnemyAnimator(enemyTransform);
        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            Log("Could not find enemy Animator.");
            animationPlaying = false;
            yield break;
        }

        if (movement == null)
        {
            Log("Could not find EnemyMovement.");
            animationPlaying = false;
            yield break;
        }

        if (agent == null)
        {
            Log("Could not find NavMeshAgent.");
            animationPlaying = false;
            yield break;
        }

        if (!HasAnimatorState(animator, doorwayAnimationName))
        {
            Log("Could not find Animator state: " + doorwayAnimationName);
            animationPlaying = false;
            yield break;
        }

        if (!HasAnimatorState(animator, walkingAnimationName))
        {
            Log("Could not find Animator state: " + walkingAnimationName);
            animationPlaying = false;
            yield break;
        }

        Vector3 originalDestination = agent.hasPath
            ? agent.destination
            : enemyTransform.position;

        bool hadOriginalPath = agent.hasPath;

        bool movementWasEnabled = movement.enabled;
        bool agentWasEnabled = agent.enabled;
        bool originalUpdatePosition = agent.updatePosition;
        bool originalUpdateRotation = agent.updateRotation;
        bool originalRootMotion = animator.applyRootMotion;

        Log("Starting doorway animation.");

        enemy.SetMovementLocked(true);

        movement.enabled = false;

        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.updatePosition = false;
            agent.updateRotation = false;
        }

        Transform targetStartPoint = GetClosestStartPoint(enemyTransform);

        if (targetStartPoint != null)
        {
            Vector3 targetPosition = new Vector3(
                targetStartPoint.position.x,
                enemyTransform.position.y,
                targetStartPoint.position.z
            );

            Quaternion targetRotation = targetStartPoint.rotation;

            yield return StartCoroutine(
                MoveToAnimationStart(
                    enemyTransform,
                    targetPosition,
                    targetRotation
                )
            );
        }

        DoorwayRootMotionDriver rootMotionDriver =
            animator.gameObject.GetComponent<DoorwayRootMotionDriver>();

        if (rootMotionDriver == null)
        {
            rootMotionDriver =
                animator.gameObject.AddComponent<DoorwayRootMotionDriver>();
        }

        rootMotionDriver.Initialize(enemyTransform);

        animator.applyRootMotion = true;

        if (HasParameterOfType(
            animator,
            "State",
            AnimatorControllerParameterType.Int))
        {
            animator.SetInteger(
                "State",
                (int)EnemyController.State.Idle
            );
        }

        if (HasParameterOfType(
            animator,
            "animIsWalking",
            AnimatorControllerParameterType.Bool))
        {
            animator.SetBool(
                "animIsWalking",
                false
            );
        }

        animator.CrossFadeInFixedTime(
            doorwayAnimationName,
            animationCrossFadeTime,
            animationLayer,
            0f
        );

        yield return null;

        AnimatorStateInfo stateInfo =
            animator.GetCurrentAnimatorStateInfo(animationLayer);

        float animationLength = stateInfo.length;

        if (animationLength <= 0f)
            animationLength = 1f;

        float elapsed = 0f;

        while (elapsed < animationLength)
        {
            if (enemy == null || !enemy.IsAlive)
                break;

            if (enemy.currentState == EnemyController.State.Attack)
            {
                Log("Enemy entered Attack. Doorway animation interrupted.");
                break;
            }

            float normalizedTime =
                Mathf.Clamp01(elapsed / animationLength);

            animator.Play(
                doorwayAnimationName,
                animationLayer,
                normalizedTime
            );

            if (HasParameterOfType(
                animator,
                "State",
                AnimatorControllerParameterType.Int))
            {
                animator.SetInteger(
                    "State",
                    (int)EnemyController.State.Idle
                );
            }

            if (HasParameterOfType(
                animator,
                "animIsWalking",
                AnimatorControllerParameterType.Bool))
            {
                animator.SetBool(
                    "animIsWalking",
                    false
                );
            }

            elapsed += Time.deltaTime;

            yield return null;
        }

        if (enemy == null)
        {
            if (rootMotionDriver != null)
                Destroy(rootMotionDriver);

            animationPlaying = false;
            yield break;
        }

        Log("Doorway animation finished. Returning control to enemy AI.");

        if (rootMotionDriver != null)
            Destroy(rootMotionDriver);

        animator.applyRootMotion = originalRootMotion;

        if (agent != null && agent.enabled)
        {
            agent.Warp(enemyTransform.position);

            agent.updatePosition = originalUpdatePosition;
            agent.updateRotation = originalUpdateRotation;

            agent.isStopped = false;

            if (hadOriginalPath)
                agent.SetDestination(originalDestination);
        }

        movement.enabled = movementWasEnabled;

        enemy.SetMovementLocked(false);

        if (HasParameterOfType(
            animator,
            "animIsWalking",
            AnimatorControllerParameterType.Bool))
        {
            animator.SetBool(
                "animIsWalking",
                true
            );
        }

        animator.Play(
            walkingAnimationName,
            animationLayer,
            0f
        );

        animationPlaying = false;

        if (agent != null)
        {
            if (!agent.enabled && agentWasEnabled)
                agent.enabled = true;

            if (agent.enabled)
                agent.nextPosition = enemyTransform.position;
        }

        if (doorwayCooldown > 0f)
            yield return StartCoroutine(CooldownRoutine());
    }

    private IEnumerator MoveToAnimationStart(
        Transform enemy,
        Vector3 targetPosition,
        Quaternion targetRotation)
    {
        if (snapDuration <= 0f)
        {
            enemy.position = targetPosition;
            enemy.rotation = targetRotation;
            yield break;
        }

        Vector3 startingPosition = enemy.position;
        Quaternion startingRotation = enemy.rotation;

        float elapsed = 0f;

        while (elapsed < snapDuration)
        {
            float t = elapsed / snapDuration;

            t = Mathf.SmoothStep(0f, 1f, t);

            enemy.position = Vector3.Lerp(
                startingPosition,
                targetPosition,
                t
            );

            enemy.rotation = Quaternion.Slerp(
                startingRotation,
                targetRotation,
                t
            );

            elapsed += Time.deltaTime;

            yield return null;
        }

        enemy.position = targetPosition;
        enemy.rotation = targetRotation;
    }

    private IEnumerator CooldownRoutine()
    {
        cooldownActive = true;

        Log(
            "Doorway cooldown started: " +
            doorwayCooldown +
            " seconds."
        );

        yield return new WaitForSeconds(doorwayCooldown);

        cooldownActive = false;

        Log("Doorway cooldown finished.");
    }

    private Transform GetClosestStartPoint(Transform enemy)
    {
        if (startPointA == null)
            return startPointB;

        if (startPointB == null)
            return startPointA;

        float distanceA = Vector3.SqrMagnitude(
            enemy.position - startPointA.position
        );

        float distanceB = Vector3.SqrMagnitude(
            enemy.position - startPointB.position
        );

        return distanceA <= distanceB
            ? startPointA
            : startPointB;
    }

    private Animator FindEnemyAnimator(Transform enemy)
    {
        Transform[] children =
            enemy.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            Component target =
                child.GetComponent("animatorTarget");

            if (target == null)
                continue;

            Animator animator =
                child.GetComponent<Animator>();

            if (animator == null)
            {
                animator =
                    child.GetComponentInChildren<Animator>(true);
            }

            if (animator == null)
            {
                animator =
                    child.GetComponentInParent<Animator>();
            }

            if (animator != null)
                return animator;
        }

        Animator fallback =
            enemy.GetComponentInChildren<Animator>(true);

        return fallback;
    }

    private bool HasAnimatorState(
        Animator animator,
        string stateName)
    {
        if (animator == null ||
            animator.runtimeAnimatorController == null)
        {
            return false;
        }

        int hash = Animator.StringToHash(stateName);

        return animator.HasState(
            animationLayer,
            hash
        );
    }

    private bool HasParameterOfType(
        Animator animator,
        string parameterName,
        AnimatorControllerParameterType type)
    {
        if (animator == null)
            return false;

        foreach (
            AnimatorControllerParameter parameter
            in animator.parameters)
        {
            if (parameter.name == parameterName &&
                parameter.type == type)
            {
                return true;
            }
        }

        return false;
    }

    private void Log(string message)
    {
        if (!debugLogs)
            return;

        Debug.Log(
            "[DoorwayAnimationTrigger] " +
            gameObject.name +
            " - " +
            message
        );
    }

    private class DoorwayRootMotionDriver : MonoBehaviour
    {
        private Transform targetRoot;
        private Animator animator;

        public void Initialize(Transform root)
        {
            targetRoot = root;
            animator = GetComponent<Animator>();
        }

        private void OnAnimatorMove()
        {
            if (targetRoot == null || animator == null)
                return;

            Vector3 deltaPosition = animator.deltaPosition;
            Quaternion deltaRotation = animator.deltaRotation;

            targetRoot.position += deltaPosition;
            targetRoot.rotation *= deltaRotation;
        }
    }
}