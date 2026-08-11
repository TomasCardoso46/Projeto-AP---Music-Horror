using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAttack : MonoBehaviour
{
    public EnemySettings settings;

    [SerializeField] private bool canAttack = true;

    private bool onCooldown = false;
    private bool isAttacking = false;

    private IEnemy owner;

    [SerializeField] private Animator animator;
    [SerializeField] private Jumpscare jumpscare;

    [Header("Post Attack Movement")]
    [SerializeField] private HighSoundReaction highSoundReaction;

    private EnemyController enemyController;
    private EnemyPatrol enemyPatrol;
    private NavMeshAgent navMeshAgent;

    private float attackLockDuration = 1.5f;

    private float cachedPatrolSpeed;
    private float cachedChaseSpeed;

    private void Awake()
    {
        enemyController = GetComponentInParent<EnemyController>();
        enemyPatrol = GetComponentInParent<EnemyPatrol>();
        navMeshAgent = GetComponentInParent<NavMeshAgent>();

        if (highSoundReaction == null)
            highSoundReaction = GetComponentInParent<HighSoundReaction>();
    }

    public void Initialize(EnemySettings s, IEnemy enemyOwner)
    {
        settings = s;
        owner = enemyOwner;

        cachedPatrolSpeed = settings.PatrolSpeed;
        cachedChaseSpeed = settings.ChaseSpeed;
    }

    public void TryAttack(Transform target)
    {
        if (!canAttack || onCooldown || isAttacking || target == null)
            return;

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist <= settings.AttackRange)
        {
            StartCoroutine(PerformAttack(target));
        }
    }

    public IEnumerator PerformAttack(Transform target)
    {
        isAttacking = true;
        onCooldown = true;

        SetMovementLock(true);

        if (animator != null)
            animator.SetTrigger("animAttack");

        var healthComp = target.GetComponent<EnemyHealth>() ??
                         target.GetComponentInChildren<EnemyHealth>();

        if (healthComp != null)
        {
            healthComp.TakeDamage(
                settings.AttackDamage,
                transform.position
            );
        }
        else
        {
            target.SendMessage(
                "TakeDamage",
                settings.AttackDamage,
                SendMessageOptions.DontRequireReceiver
            );
        }

        if (target.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            jumpscare?.TriggerJumpscare();
        }

        yield return new WaitForSeconds(attackLockDuration);


        yield return StartCoroutine(MoveToHighSoundSpawnPoint());

        if (highSoundReaction != null)
        {
            highSoundReaction.enabled = true;
        }


        float remaining = Mathf.Max(
            0f,
            settings.AttackCooldown - attackLockDuration
        );

        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        onCooldown = false;
        isAttacking = false;
    }

    private IEnumerator MoveToHighSoundSpawnPoint()
    {
        if (enemyController != null)
            enemyController.enabled = false;

        if (enemyPatrol != null)
            enemyPatrol.enabled = false;

        if (navMeshAgent == null)
        {
            Debug.LogWarning(
                $"{name}: No NavMeshAgent found for post-attack movement."
            );

            yield break;
        }

        if (highSoundReaction == null)
        {
            Debug.LogWarning(
                $"{name}: No HighSoundReaction assigned."
            );

            yield break;
        }

        Transform targetSpawn =
            highSoundReaction.GetClosestSpawnPoint(transform.position);

        if (targetSpawn == null)
        {
            Debug.LogWarning(
                $"{name}: HighSoundReaction has no valid spawn points."
            );

            yield break;
        }

        navMeshAgent.enabled = true;

        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(targetSpawn.position);

        while (true)
        {
            if (!navMeshAgent.enabled)
                yield break;

            if (navMeshAgent.pathPending)
            {
                yield return null;
                continue;
            }

            if (navMeshAgent.remainingDistance <=
                navMeshAgent.stoppingDistance + 0.1f)
            {
                break;
            }

            yield return null;
        }

        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
    }

    private void SetMovementLock(bool locked)
    {
        if (enemyController == null)
            return;

        enemyController.enabled = !locked;
    }

    public void DisableAttack()
    {
        canAttack = false;
        onCooldown = true;
    }

    public void TriggerAttackAnimationOnly()
    {
        if (animator != null)
            animator.SetTrigger("animAttack");
    }
}