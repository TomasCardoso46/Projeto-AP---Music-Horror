using UnityEngine;
using System.Collections;

public class EnemyAttack : MonoBehaviour
{
    public EnemySettings settings;

    public bool canAttack = true;

    private bool onCooldown = false;
    private bool isAttacking = false;

    private IEnemy owner;

    [SerializeField] private Animator animator;
    [SerializeField] private Jumpscare jumpscare;

    [Header("Post Attack Movement")]
    [SerializeField] private EnemyMovement enemyMovement;
    [SerializeField] private HighSoundReaction highSoundReaction;

    private EnemyController enemyController;
    private EnemyPatrol enemyPatrol;

    private float attackLockDuration = 1.5f;

    private float cachedPatrolSpeed;
    private float cachedChaseSpeed;

    private void Awake()
    {
        enemyController = GetComponentInParent<EnemyController>();
        enemyPatrol = GetComponentInParent<EnemyPatrol>();

        if (enemyMovement == null)
            enemyMovement = GetComponentInParent<EnemyMovement>();

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

        // ============================================
        // POST-ATTACK SEQUENCE
        // ============================================

        yield return StartCoroutine(MoveToClosestSpawnPoint());

        // Enable HighSoundReaction after reaching
        // the destination.
        if (highSoundReaction != null)
        {
            highSoundReaction.enabled = true;
        }

        // ============================================

        float remaining = Mathf.Max(
            0f,
            settings.AttackCooldown - attackLockDuration
        );

        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        onCooldown = false;
        isAttacking = false;
    }

    private IEnumerator MoveToClosestSpawnPoint()
    {
        // Disable the normal enemy behaviour.
        if (enemyController != null)
            enemyController.enabled = false;

        if (enemyPatrol != null)
            enemyPatrol.enabled = false;

        if (enemyMovement == null)
        {
            Debug.LogWarning(
                $"{name}: EnemyMovement reference is missing."
            );

            yield break;
        }

        if (highSoundReaction == null)
        {
            Debug.LogWarning(
                $"{name}: HighSoundReaction reference is missing."
            );

            yield break;
        }

        // Ask HighSoundReaction for the spawn point
        // closest to the enemy's current position.
        Transform closestSpawn =
            highSoundReaction.GetClosestSpawnPoint(transform.position);

        if (closestSpawn == null)
        {
            Debug.LogWarning(
                $"{name}: No valid spawn points were found."
            );

            yield break;
        }

        // Force the EnemyMovement component to navigate
        // to the spawn point.
        yield return StartCoroutine(
            enemyMovement.ForceMoveTo(closestSpawn.position)
        );
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