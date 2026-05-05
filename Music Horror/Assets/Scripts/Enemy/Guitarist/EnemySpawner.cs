using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyObject;

    [Header("Detection")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool alertOnSpawn;

    public UnityEvent OnChaseTriggered;

    private EnemyController enemy;

    private void Awake()
    {
        if (enemyObject != null)
            enemy = enemyObject.GetComponent<EnemyController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider is on the Player layer
        if (((1 << other.gameObject.layer) & playerLayer) == 0)
            return;

        if (enemyObject == null || enemy == null)
        {
            Debug.LogWarning("EnemySpawner: Enemy object or EnemyController missing.");
            return;
        }

        // Teleport enemy to spawn position
        Transform targetSpawn = spawnPoint != null ? spawnPoint : transform;
        enemyObject.transform.SetPositionAndRotation(
            targetSpawn.position,
            targetSpawn.rotation
        );

        // Activate enemy
        enemyObject.SetActive(true);

        if (alertOnSpawn)
        {
            TriggerChase();
        }
        Destroy(this.gameObject.GetComponent<EnemySpawner>());
    }

    private void TriggerChase()
    {
        // Find the object with the Chord component
        FirstPersonRigidbodyController playerController = FindObjectOfType<FirstPersonRigidbodyController>();

        if (playerController == null)
        {
            Debug.LogWarning("EnemySpawner: No Chord component found.");
            return;
        }

        enemy.AlertToPosition(playerController.transform.position);
    }
}
