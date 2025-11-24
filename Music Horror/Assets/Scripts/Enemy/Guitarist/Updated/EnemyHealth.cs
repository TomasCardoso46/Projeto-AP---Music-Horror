using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private bool destroyOnDeath = false;
    [SerializeField] private GameObject ragdollRoot; 
    private int currentHealth;
    private IEnemy owner;

    public bool IsAlive => currentHealth > 0;

    public void Initialize(IEnemy owningEnemy)
    {
        owner = owningEnemy;
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount, Vector3 hitPoint)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die(hitPoint);
        }
    }

    private void Die(Vector3 hitPoint)
    {
        currentHealth = 0;
        // notify owner if needed - owner may react to death
        var controller = owner as EnemyController;
        if (controller != null)
        {
            // disable navmesh, colliders etc.
            var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.enabled = false;
            var animator = GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
        }

        if (ragdollRoot != null)
        {
            EnableRagdoll();
        }

        if (destroyOnDeath)
        {
            Destroy(gameObject, 5f);
        }
    }

    private void EnableRagdoll()
    {
        // enable Rigidbodies and colliders under ragdollRoot
        var rbs = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        var cols = ragdollRoot.GetComponentsInChildren<Collider>();
        foreach (var c in cols) c.enabled = true;
        foreach (var rb in rbs) rb.isKinematic = false;
    }
}
