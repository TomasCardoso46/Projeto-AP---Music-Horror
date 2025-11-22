using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private EnemySettings settings;
    [SerializeField] private bool canAttack = true;
    private bool onCooldown = false;
    private IEnemy owner;

    [Header("Jumpscare Settings")]
    [SerializeField] private Jumpscare jumpscare; // Drag your Jumpscare script here

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
            // Attempt attack
            var healthComp = target.GetComponent<EnemyHealth>() ?? target.GetComponentInChildren<EnemyHealth>();
            if (healthComp != null)
            {
                Debug.Log("Attack");
                healthComp.TakeDamage(settings.AttackDamage, transform.position);

                // Trigger jumpscare if assigned
                if (jumpscare != null)
                    jumpscare.TriggerJumpscare();
            }
            else
            {
                // Generic fallback
                target.SendMessage("TakeDamage", settings.AttackDamage, SendMessageOptions.DontRequireReceiver);
                Debug.Log("Attack");

                // Trigger jumpscare if assigned
                if (jumpscare != null)
                    jumpscare.TriggerJumpscare();
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
