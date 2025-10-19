using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private EnemySettings settings;
    [SerializeField] private bool canAttack = true;
    private bool onCooldown = false;
    private IEnemy owner;

    public void Initialize(EnemySettings s, IEnemy enemyOwner)
    {
        settings = s;
        owner = enemyOwner;
    }

    public void TryAttack(Transform target)
    {
        if (!canAttack || onCooldown || target == null) return;

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= settings.AttackRange)
        {
            // attempt attack
            var healthComp = target.GetComponent<EnemyHealth>() ?? target.GetComponentInChildren<EnemyHealth>();
            // if target is player, they should have a script with a TakeDamage method. We call by component name "PlayerHealth" in many projects.
            // We'll try common names; if not present, we can use messages.
            if (healthComp != null)
            {
                healthComp.TakeDamage(settings.AttackDamage, transform.position);
            }
            else
            {
                // generic send message fallback
                target.SendMessage("TakeDamage", settings.AttackDamage, SendMessageOptions.DontRequireReceiver);
            }

            StartCoroutine(AttackCooldown());
        }
    }

    private IEnumerator AttackCooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(settings.AttackCooldown);
        onCooldown = false;
    }

    public void DisableAttack()
    {
        canAttack = false;
        onCooldown = true;
    }
}
