using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EnemySonar : MonoBehaviour
{
    [Header("Activation")]
    public KeyCode testKey = KeyCode.Y;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip abilitySound;

    [Header("Detection Object")]
    public GameObject detectionPrefab;
    public Vector3 growthSpeed = new Vector3(1f, 1f, 1f);

    [Header("Pulse Prefab (Spawns every second)")]
    public GameObject pulsePrefab;
    public float pulseInterval = 1f;

    [Header("Movement Detection")]
    public LayerMask playerLayer;
    public float movementSafetyLimit = 1.5f;

    [Header("Ability")]
    public float abilityDuration = 2f;

    [Header("Events")]
    public UnityEvent OnChaseTriggered;

    [Header("Enemy")]
    [SerializeField] private GameObject enemyObject;

    private GameObject spawnedDetectionObject;
    private Rigidbody playerRb;
    private bool playerInsideDetection;
    private bool abilityActive;

    private EnemyController enemy;

    private void Awake()
    {
        if (enemyObject != null)
            enemy = enemyObject.GetComponent<EnemyController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            ActivateAbility();
        }

        if (abilityActive && playerInsideDetection && playerRb != null)
        {
            if (playerRb.linearVelocity.magnitude > movementSafetyLimit)
            {
                TriggerChase();
            }
        }
    }

    public void ActivateAbility()
    {
        if (abilityActive) return;

        abilityActive = true;

        // Play audio
        if (audioSource && abilitySound)
        {
            audioSource.PlayOneShot(abilitySound);
        }

        // Find player by layer
        FindPlayerRigidbody();

        // Spawn main detection object
        spawnedDetectionObject = Instantiate(detectionPrefab, transform.position, Quaternion.identity);
        spawnedDetectionObject.transform.localScale = Vector3.zero;

        DetectionTrigger trigger = spawnedDetectionObject.AddComponent<DetectionTrigger>();
        trigger.Initialize(this, playerLayer);

        // Start routines
        StartCoroutine(GrowDetectionObject());
        StartCoroutine(PulseRoutine());
        StartCoroutine(AbilityDurationRoutine());
    }

    private void FindPlayerRigidbody()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (((1 << obj.layer) & playerLayer) != 0)
            {
                playerRb = obj.GetComponent<Rigidbody>();
                if (playerRb != null)
                    return;
            }
        }
    }

    private IEnumerator GrowDetectionObject()
    {
        float elapsed = 0f;

        while (elapsed < abilityDuration)
        {
            elapsed += Time.deltaTime;

            if (spawnedDetectionObject != null)
                spawnedDetectionObject.transform.localScale += growthSpeed * Time.deltaTime;

            yield return null;
        }
    }

    private IEnumerator PulseRoutine()
    {
        while (abilityActive)
        {
            if (pulsePrefab != null)
            {
                Instantiate(pulsePrefab, transform.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(pulseInterval);
        }
    }

    private IEnumerator AbilityDurationRoutine()
    {
        yield return new WaitForSeconds(abilityDuration);
        CleanupAbility();
    }

    private void CleanupAbility()
    {
        abilityActive = false;
        playerInsideDetection = false;

        if (spawnedDetectionObject)
            Destroy(spawnedDetectionObject);
    }

    private void TriggerChase()
    {
        FirstPersonRigidbodyController playerController = FindObjectOfType<FirstPersonRigidbodyController>();

        if (playerController == null)
        {
            Debug.LogWarning("EnemySonar: No player controller found.");
            return;
        }

        enemy.AlertToPosition(playerController.transform.position);
    }

    // Called by DetectionTrigger
    public void SetPlayerInside(bool inside)
    {
        playerInsideDetection = inside;
    }
}