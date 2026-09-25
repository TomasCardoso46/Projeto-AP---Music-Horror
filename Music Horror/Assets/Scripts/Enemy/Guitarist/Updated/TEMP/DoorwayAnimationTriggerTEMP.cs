using System.Collections.Generic;
using UnityEngine;

public class doorwayAnimationTriggerTEMP : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private string doorwayAnimationName = "Traverse";
    [SerializeField] private int animationLayer = 0;

    [Header("Cooldown")]
    [SerializeField] private float animationCooldown = 3f;

    private readonly Dictionary<EnemyController, float> enemyCooldowns = new Dictionary<EnemyController, float>();

    private void Awake()
    {
        Collider triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyController enemy = other.GetComponentInParent<EnemyController>();

        if (enemy == null)
            return;

        if (enemyCooldowns.TryGetValue(enemy, out float lastTriggerTime))
        {
            if (Time.time < lastTriggerTime + animationCooldown)
                return;
        }

        Animator animator = enemy.GetComponentInChildren<Animator>(true);

        if (animator == null)
            return;

        enemyCooldowns[enemy] = Time.time;

        animator.Play(doorwayAnimationName, animationLayer, 0f);
    }
}