using UnityEngine;
using UnityEngine.AI;

public class DoorInteraction : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private Transform door;
    [SerializeField] private Transform sigilsParent;
    [SerializeField] private string playerTag = "Player";

    [Header("Door Rotation Settings")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float rotationSpeed = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSource openSoundSource;
    [SerializeField] private AudioClip openSoundClip;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private NavMeshObstacle navMeshObstacle;

    private bool isPlayerInRange = false;
    private bool hasUnlocked = false;
    private bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotationA;
    private Quaternion openRotationB;
    private Quaternion targetRotation;

    private Transform player;
    private EnemyAudioEmitter enemyAudioEmitter;

    private void Start()
    {
        Log("Initializing door...");

        enemyAudioEmitter = FindObjectOfType<EnemyAudioEmitter>();

        if (door != null)
        {
            closedRotation = door.localRotation;
            navMeshObstacle = door.GetComponent<NavMeshObstacle>();

            Vector3 baseEuler = door.localEulerAngles;
            openRotationA = Quaternion.Euler(baseEuler + new Vector3(0, openAngle, 0));
            openRotationB = Quaternion.Euler(baseEuler + new Vector3(0, -openAngle, 0));

            targetRotation = closedRotation;
        }

        if (AreAllSigilsInactive())
        {
            Log("All sigils inactive at start. Unlocking door.");
            UnlockDoor();
        }
    }

    private void Update()
    {
        if (!isPlayerInRange)
            return;

        if (AreAllSigilsInactive() && !hasUnlocked)
        {
            Log("Sigils deactivated. Unlocking door.");
            UnlockDoor();
        }
    }

    private void FixedUpdate()
    {
        if (door != null)
        {
            door.localRotation = Quaternion.Lerp(
                door.localRotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }

    public void Interact()
    {
        if (!hasUnlocked)
        {
            Log("Tried to interact but door is locked.");
            return;
        }

        if (!isPlayerInRange)
        {
            Log("Player not in range.");
            return;
        }

        ToggleDoor();
    }

    private void UnlockDoor()
    {
        hasUnlocked = true;

        if (navMeshObstacle != null && navMeshObstacle.enabled)
        {
            navMeshObstacle.enabled = false;
            Log("NavMeshObstacle disabled. AI can pass.");
        }

        Log("Door unlocked.");
    }

    private void ToggleDoor()
    {
        if (!isOpen)
        {
            targetRotation = DetermineOpenDirection();
            isOpen = true;

            Log("Door opening.");
            PlayOpenSound();
            EmitNormalSoundForPlayer();
        }
        else
        {
            targetRotation = closedRotation;
            isOpen = false;

            if (navMeshObstacle != null)
                navMeshObstacle.enabled = true;

            Log("Door closing. NavMeshObstacle re-enabled.");
        }
    }

    private Quaternion DetermineOpenDirection()
    {
        if (player == null)
        {
            Log("Player reference missing. Default open direction used.");
            return openRotationA;
        }

        Vector3 playerDirection = (player.position - transform.position).normalized;
        float side = Vector3.Dot(transform.forward, playerDirection);

        Quaternion result = side > 0 ? openRotationB : openRotationA;

        Log("Calculated open direction for player.");
        return result;
    }

    private void OpenDoorForEnemy(Transform enemy)
    {
        Debug.LogWarning("tried to open door full");
        if (!isOpen)
        {
            targetRotation = DetermineOpenDirectionForEnemy(enemy);
            isOpen = true;

            if (navMeshObstacle != null)
                navMeshObstacle.enabled = false;

            Log($"Door opened automatically for enemy: {enemy.name}");
            PlayOpenSound();
        }
    }

    private Quaternion DetermineOpenDirectionForEnemy(Transform enemy)
    {
        Vector3 direction = (enemy.position - transform.position).normalized;
        float side = Vector3.Dot(transform.forward, direction);

        Log($"Calculated open direction for enemy: {enemy.name}");
        return side > 0 ? openRotationB : openRotationA;
    }

    private void PlayOpenSound()
    {
        if (openSoundSource != null && openSoundClip != null)
        {
            openSoundSource.PlayOneShot(openSoundClip);
            Log("Open sound played.");
        }
    }

    private void EmitNormalSoundForPlayer()
    {
        if (enemyAudioEmitter != null && player != null)
        {
            enemyAudioEmitter.EmitSound(EnemyAudioEmitter.SoundLevel.Normal);
            Log("Enemy sound emitted (Normal level).");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (AreAllSigilsInactive() && !hasUnlocked)
        {
            Log("Trigger entered and sigils inactive. Unlocking door.");
            UnlockDoor();
        }

        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            player = other.transform;
            Log("Player entered door trigger.");
        }

        if (other.CompareTag("Enemy") && hasUnlocked)
        {
            Log($"Enemy entered trigger: {other.name}");
            OpenDoorForEnemy(other.transform);
            Debug.LogWarning("tried to open door");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
            Log("Player exited door trigger.");
        }
    }

    private bool AreAllSigilsInactive()
    {
        if (sigilsParent == null)
            return true;

        foreach (Transform sigil in sigilsParent)
            if (sigil.gameObject.activeSelf)
                return false;

        return true;
    }

    private void Log(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[DoorInteraction] {message}", this);
    }

    private void LogWarning(string message)
    {
        if (enableDebugLogs)
            Debug.LogWarning($"[DoorInteraction] {message}", this);
    }
}