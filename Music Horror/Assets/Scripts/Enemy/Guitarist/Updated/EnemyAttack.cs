using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class EnemyAttack : MonoBehaviour
{
    public EnemySettings settings;
    [SerializeField] private bool canAttack = true;
    private bool onCooldown = false;
    private IEnemy owner;
    public Animator animator;

    [Header("Jumpscare Settings")]
    [SerializeField] private Jumpscare jumpscare; 

    public void Initialize(EnemySettings s, IEnemy enemyOwner)
    {
        settings = s;
        owner = enemyOwner;
    }

    public void TryAttack(Transform target)
    {
        animator.SetTrigger("animAttack");
        if (!canAttack || onCooldown || target == null) return;

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= settings.AttackRange)
        {
            // Apply damage if applicable
            var healthComp = target.GetComponent<EnemyHealth>() ?? target.GetComponentInChildren<EnemyHealth>();
            if (healthComp != null)
            {
                Debug.Log("Attack");
                healthComp.TakeDamage(settings.AttackDamage, transform.position);
            }
            else
            {
                target.SendMessage("TakeDamage", settings.AttackDamage, SendMessageOptions.DontRequireReceiver);
                Debug.Log("Attack");
            }

            // Layer based behavior
            int playerLayer = LayerMask.NameToLayer("Player");
            int hidingLayer = LayerMask.NameToLayer("HidingSpot");

            if (target.gameObject.layer == playerLayer)
            {
                // Player directly
                if (jumpscare != null)
                    jumpscare.TriggerJumpscare();
            }
            /*else if (target.gameObject.layer == hidingLayer)
            {
                // Get the HideSpot script
                HideSpot hideSpot = target.GetComponent<HideSpot>();
                if (hideSpot != null)
                {
                    // If the player is hiding, force them out first
                    if (hideSpot.IsPlayerHiding)
                    {
                        Debug.Log("Player Inside");
                        //hideSpot.ToggleHide();
                        if (jumpscare != null)
                            jumpscare.TriggerJumpscare();
                    }
                    else
                    {
                        
                        Destroy(target.gameObject);
                    }
                }

                
            }*/

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

    public void TriggerAttackAnimationOnly()
{
    if (animator != null)
    {
        animator.SetTrigger("animAttack");
    }
}
}
