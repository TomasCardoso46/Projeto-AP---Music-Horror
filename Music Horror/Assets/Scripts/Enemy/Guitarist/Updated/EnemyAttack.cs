using UnityEngine;
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

    private EnemyController enemyController;

    private float attackLockDuration = 1.5f;

    private float cachedPatrolSpeed;
    private float cachedChaseSpeed;

    private void Awake()
    {
        enemyController = GetComponentInParent<EnemyController>();
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

    private IEnumerator PerformAttack(Transform target)
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
            healthComp.TakeDamage(settings.AttackDamage, transform.position);
        }
        else
        {
            target.SendMessage("TakeDamage", settings.AttackDamage, SendMessageOptions.DontRequireReceiver);
        }

        if (target.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            jumpscare?.TriggerJumpscare();
        }

        yield return new WaitForSeconds(attackLockDuration);

        SetMovementLock(false);

        float remaining = Mathf.Max(0f, settings.AttackCooldown - attackLockDuration);

        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        onCooldown = false;
        isAttacking = false;
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