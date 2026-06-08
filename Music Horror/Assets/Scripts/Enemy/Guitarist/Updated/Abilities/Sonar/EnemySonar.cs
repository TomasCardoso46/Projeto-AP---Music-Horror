using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EnemySonar : MonoBehaviour
{
    [Header("Activation")]
    public KeyCode testKey = KeyCode.Y;

    [Header("Pre-Activation")]
    public float staticEnableDuration = 2f;

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

    private void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            StartCoroutine(ActivateAbilitySequence());
        }

        if (abilityActive && playerInsideDetection && playerRb != null)
        {
            if (playerRb.linearVelocity.magnitude > movementSafetyLimit)
            {
                TriggerChase();
            }
        }
    }

    private IEnumerator ActivateAbilitySequence()
    {
        if (abilityActive)
            yield break;

        Static[] staticObjects = FindObjectsByType<Static>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Static obj in staticObjects)
        {
            if (obj != null)
                obj.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(staticEnableDuration);

        foreach (Static obj in staticObjects)
        {
            if (obj != null)
                obj.gameObject.SetActive(false);
        }

        ActivateAbility();
    }

    public void ActivateAbility()
    {
        if (abilityActive)
            return;

        abilityActive = true;

        if (audioSource != null && abilitySound != null)
        {
            audioSource.PlayOneShot(abilitySound);
        }

        FindPlayerRigidbody();

        spawnedDetectionObject = Instantiate(
            detectionPrefab,
            transform.position,
            Quaternion.identity
        );

        spawnedDetectionObject.transform.localScale = Vector3.zero;

        DetectionTrigger trigger = spawnedDetectionObject.AddComponent<DetectionTrigger>();
        trigger.Initialize(this, playerLayer);

        StartCoroutine(GrowDetectionObject());
        StartCoroutine(PulseRoutine());
        StartCoroutine(AbilityDurationRoutine());
    }

    private void FindPlayerRigidbody()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(
            FindObjectsSortMode.None
        );

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
            {
                spawnedDetectionObject.transform.localScale +=
                    growthSpeed * Time.deltaTime;
            }

            yield return null;
        }
    }

    private IEnumerator PulseRoutine()
    {
        while (abilityActive)
        {
            if (pulsePrefab != null)
            {
                Instantiate(
                    pulsePrefab,
                    transform.position,
                    Quaternion.identity
                );
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

        if (spawnedDetectionObject != null)
        {
            Destroy(spawnedDetectionObject);
        }
    }

    private void TriggerChase()
    {
        FirstPersonRigidbodyController playerController =
            FindFirstObjectByType<FirstPersonRigidbodyController>();

        if (playerController == null)
        {
            Debug.LogWarning("EnemySonar: No player controller found.");
            return;
        }

        if (enemy != null)
        {
            enemy.AlertToPosition(playerController.transform.position);
        }

        OnChaseTriggered?.Invoke();
    }

    public void SetPlayerInside(bool inside)
    {
        playerInsideDetection = inside;
    }
}