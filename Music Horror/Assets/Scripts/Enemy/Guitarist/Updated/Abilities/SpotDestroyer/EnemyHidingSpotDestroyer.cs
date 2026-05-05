using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
[RequireComponent(typeof(EnemyAttack))]
public class EnemyHidingSpotDestroyer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float alertRadius = 10f;     // Radius to search for hiding spots
    [SerializeField] private LayerMask hideSpotLayer;     // Layer for HideSpot objects
    [SerializeField] private float attackDelay = 0.5f;    // Delay before destroying spot after attack

    private EnemyController enemyController;
    private EnemyAttack attack;
    private Transform player;

    private Queue<HideSpot> hideSpotQueue = new Queue<HideSpot>();
    private bool hunting = false;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        attack = GetComponent<EnemyAttack>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
            Debug.LogWarning("[HideSpotHunter] Player not found with tag 'Player'.");
    }

    private void Update()
    {
        if (player == null) return;

        // Manual test
        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("[TEST] Manual hide spot hunt triggered.");
            StartHunt();
        }

        // Trigger automatically if enemy is investigating and player is hiding
        HideSpot playerSpot = player.GetComponentInParent<HideSpot>();
        bool playerIsHiding = playerSpot != null && playerSpot.IsPlayerHiding;

        if (enemyController.currentState == EnemyController.State.Investigate && playerIsHiding && !hunting)
        {
            Debug.Log("[AI] Player is hiding, starting hide spot hunt.");
            StartHunt();
        }
    }

    public void StartHunt()
    {
        hideSpotQueue.Clear();

        // Find all hiding spots within alert radius
        Collider[] hits = Physics.OverlapSphere(transform.position, alertRadius, hideSpotLayer);
        foreach (var hit in hits)
        {
            HideSpot spot = hit.GetComponent<HideSpot>();
            if (spot != null)
            {
                hideSpotQueue.Enqueue(spot);
                Debug.Log("[AI] Added hide spot to queue: " + spot.name);
            }
        }

        if (hideSpotQueue.Count > 0 && !hunting)
            StartCoroutine(HuntHideSpots());
    }

    private IEnumerator HuntHideSpots()
    {
        hunting = true;

        while (hideSpotQueue.Count > 0)
        {
            HideSpot targetSpot = hideSpotQueue.Dequeue();
            if (targetSpot == null)
                continue;

            Debug.Log("[AI] Moving to hide spot: " + targetSpot.name);

            // Alert enemy to this spot using existing system
            enemyController.AlertToPosition(targetSpot.transform.position);

            // Wait until enemy is close enough to attack
            while (Vector3.Distance(transform.position, targetSpot.transform.position) > attack.settings.AttackRange)
            {
                yield return null;
            }

            // Look at the spot
            Vector3 lookPos = new Vector3(targetSpot.transform.position.x, transform.position.y, targetSpot.transform.position.z);
            transform.LookAt(lookPos);

            // Attack
            Debug.Log("[AI] Attacking hide spot: " + targetSpot.name);
            attack.TryAttack(targetSpot.transform);

            yield return new WaitForSeconds(attackDelay);

            // Short pause before moving to next spot
            yield return new WaitForSeconds(0.5f);
        }

        hunting = false;
        Debug.Log("[AI] Hide spot hunt finished.");
    }
}
