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

    [Header("Root Motion")]
    [SerializeField] private bool transferRootMotionToParent = true;
    [SerializeField] private bool synchronizeNavMeshAgent = true;

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

        bool originalAgentIsStopped = agent.isStopped;
        bool originalUpdatePosition = agent.updatePosition;
        bool originalUpdateRotation = agent.updateRotation;
        bool originalRootMotion = animator.applyRootMotion;
        bool originalMovementEnabled = movement.enabled;

        Log("Starting doorway animation.");

        enemy.SetMovementLocked(true);

        /*
         * Completely stop the systems that can fight the forced
         * movement during Traverse.
         */
        movement.enabled = false;

        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();

            agent.updatePosition = false;
            agent.updateRotation = false;

            agent.nextPosition = enemyTransform.position;
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

            if (agent.enabled)
                agent.nextPosition = enemyTransform.position;
        }

        /*
         * Create the forced root-motion driver.
         */
        DoorwayRootMotionDriver rootMotionDriver =
            animator.gameObject.GetComponent<DoorwayRootMotionDriver>();

        if (rootMotionDriver == null)
        {
            rootMotionDriver =
                animator.gameObject.AddComponent<DoorwayRootMotionDriver>();
        }

        rootMotionDriver.Initialize(
            enemyTransform,
            agent,
            transferRootMotionToParent,
            synchronizeNavMeshAgent
        );

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

        int doorwayStateHash =
            Animator.StringToHash(doorwayAnimationName);

        /*
         * Force the animation directly.
         */
        animator.Play(
            doorwayStateHash,
            animationLayer,
            0f
        );

        /*
         * Force Animator evaluation immediately.
         */
        animator.Update(0f);

        Log("Traverse animation forced.");

        /*
         * Wait until Traverse is actually the active state.
         */
        float stateWaitTimer = 0f;

        while (stateWaitTimer < 1f)
        {
            if (enemy == null || !enemy.IsAlive)
                break;

            AnimatorStateInfo currentState =
                animator.GetCurrentAnimatorStateInfo(animationLayer);

            if (currentState.shortNameHash == doorwayStateHash)
                break;

            stateWaitTimer += Time.deltaTime;

            yield return null;
        }

        if (enemy == null)
        {
            if (rootMotionDriver != null)
                Destroy(rootMotionDriver);

            animationPlaying = false;
            yield break;
        }

        /*
         * Wait for the actual Traverse animation to finish.
         */
        while (true)
        {
            if (enemy == null || !enemy.IsAlive)
                break;

            if (enemy.currentState == EnemyController.State.Attack)
            {
                Log("Enemy entered Attack. Doorway animation interrupted.");
                break;
            }

            AnimatorStateInfo currentState =
                animator.GetCurrentAnimatorStateInfo(animationLayer);

            if (currentState.shortNameHash != doorwayStateHash)
            {
                Log("Traverse stopped being the active state.");
                break;
            }

            if (currentState.normalizedTime >= 1f)
                break;

            yield return null;
        }

        if (enemy == null)
        {
            if (rootMotionDriver != null)
                Destroy(rootMotionDriver);

            animationPlaying = false;
            yield break;
        }

        /*
         * At this exact point the animation has finished.
         *
         * Capture the actual position of the Animator object.
         */
        Vector3 finalAnimatorPosition =
            animator.transform.position;

        Quaternion finalAnimatorRotation =
            animator.transform.rotation;

        Log(
            "Traverse finished. Animator position: " +
            finalAnimatorPosition
        );

        /*
         * Because the Animator is a child of the enemy root,
         * calculate the root position required to keep the
         * Animator exactly where it currently is.
         */
        Transform animatorParent =
            animator.transform.parent;

        if (animatorParent != null)
        {
            Vector3 localPosition =
                animator.transform.localPosition;

            Vector3 requiredParentPosition =
                finalAnimatorPosition -
                animatorParent.rotation * localPosition;

            enemyTransform.position =
                requiredParentPosition;

            Vector3 forward =
                animator.transform.forward;

            forward.y = 0f;

            if (forward.sqrMagnitude > 0.001f)
            {
                enemyTransform.rotation =
                    Quaternion.LookRotation(forward);
            }
        }
        else
        {
            enemyTransform.position =
                finalAnimatorPosition;

            enemyTransform.rotation =
                finalAnimatorRotation;
        }

        /*
         * Force NavMeshAgent to accept the new position.
         */
        if (agent != null && agent.enabled)
        {
            agent.Warp(enemyTransform.position);
            agent.nextPosition = enemyTransform.position;
        }

        Log(
            "Enemy root forcibly synchronized to: " +
            enemyTransform.position
        );

        /*
         * Remove the root-motion driver.
         */
        if (rootMotionDriver != null)
            Destroy(rootMotionDriver);

        animator.applyRootMotion = originalRootMotion;

        /*
         * Restore NavMesh settings.
         */
        if (agent != null && agent.enabled)
        {
            agent.nextPosition =
                enemyTransform.position;

            agent.updatePosition =
                originalUpdatePosition;

            agent.updateRotation =
                originalUpdateRotation;

            agent.isStopped =
                originalAgentIsStopped;

            if (hadOriginalPath)
                agent.SetDestination(originalDestination);
        }

        /*
         * Restore movement.
         */
        movement.enabled = originalMovementEnabled;

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

        /*
         * Immediately switch to walking.
         */
        animator.Play(
            walkingAnimationName,
            animationLayer,
            0f
        );

        animationPlaying = false;

        if (agent != null && agent.enabled)
        {
            agent.nextPosition =
                enemyTransform.position;
        }

        Log("Doorway sequence completely finished.");

        if (doorwayCooldown > 0f)
            yield return StartCoroutine(
                CooldownRoutine()
            );
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
            float t =
                elapsed / snapDuration;

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            enemy.position =
                Vector3.Lerp(
                    startingPosition,
                    targetPosition,
                    t
                );

            enemy.rotation =
                Quaternion.Slerp(
                    startingRotation,
                    targetRotation,
                    t
                );

            elapsed +=
                Time.deltaTime;

            yield return null;
        }

        enemy.position =
            targetPosition;

        enemy.rotation =
            targetRotation;
    }

    private IEnumerator CooldownRoutine()
    {
        cooldownActive = true;

        Log(
            "Doorway cooldown started: " +
            doorwayCooldown +
            " seconds."
        );

        yield return new WaitForSeconds(
            doorwayCooldown
        );

        cooldownActive = false;

        Log("Doorway cooldown finished.");
    }

    private Transform GetClosestStartPoint(
        Transform enemy)
    {
        if (startPointA == null)
            return startPointB;

        if (startPointB == null)
            return startPointA;

        float distanceA =
            Vector3.SqrMagnitude(
                enemy.position -
                startPointA.position
            );

        float distanceB =
            Vector3.SqrMagnitude(
                enemy.position -
                startPointB.position
            );

        return distanceA <= distanceB
            ? startPointA
            : startPointB;
    }

    private Animator FindEnemyAnimator(
        Transform enemy)
    {
        Transform[] children =
            enemy.GetComponentsInChildren<Transform>(
                true
            );

        foreach (Transform child in children)
        {
            Component target =
                child.GetComponent(
                    "animatorTarget"
                );

            if (target == null)
                continue;

            Animator animator =
                child.GetComponent<Animator>();

            if (animator == null)
            {
                animator =
                    child.GetComponentInChildren<Animator>(
                        true
                    );
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
            enemy.GetComponentInChildren<Animator>(
                true
            );

        return fallback;
    }

    private bool HasAnimatorState(
        Animator animator,
        string stateName)
    {
        if (
            animator == null ||
            animator.runtimeAnimatorController == null
        )
        {
            return false;
        }

        int hash =
            Animator.StringToHash(
                stateName
            );

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
            if (
                parameter.name ==
                parameterName &&
                parameter.type ==
                type
            )
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
        private NavMeshAgent agent;
        private Animator animator;

        private bool transferRootMotion;
        private bool synchronizeAgent;

        private Vector3 previousAnimatorRootPosition;
        private Quaternion previousAnimatorRootRotation;

        public void Initialize(
            Transform root,
            NavMeshAgent navMeshAgent,
            bool shouldTransferRootMotion,
            bool shouldSynchronizeAgent)
        {
            targetRoot = root;
            agent = navMeshAgent;

            animator =
                GetComponent<Animator>();

            transferRootMotion =
                shouldTransferRootMotion;

            synchronizeAgent =
                shouldSynchronizeAgent;

            previousAnimatorRootPosition =
                transform.position;

            previousAnimatorRootRotation =
                transform.rotation;
        }

        private void LateUpdate()
        {
            if (
                targetRoot == null ||
                animator == null
            )
            {
                return;
            }

            if (!transferRootMotion)
                return;

            /*
             * We deliberately use LateUpdate instead of relying
             * on OnAnimatorMove.
             *
             * This makes this movement happen AFTER normal Update
             * logic, making it much harder for EnemyMovement or
             * other controller code to override it.
             */
            Vector3 deltaPosition =
                animator.deltaPosition;

            Quaternion deltaRotation =
                animator.deltaRotation;

            /*
             * Force the actual enemy root to move.
             */
            targetRoot.position +=
                deltaPosition;

            targetRoot.rotation *=
                deltaRotation;

            /*
             * Force NavMesh to follow the root.
             */
            if (
                agent != null &&
                agent.enabled &&
                synchronizeAgent
            )
            {
                agent.nextPosition =
                    targetRoot.position;
            }

            previousAnimatorRootPosition =
                transform.position;

            previousAnimatorRootRotation =
                transform.rotation;
        }
    }
}