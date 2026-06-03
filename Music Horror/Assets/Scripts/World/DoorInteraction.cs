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
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSoundClip;
    [SerializeField] private AudioClip autoOpenSoundClip;
    [SerializeField] private AudioClip lockedSoundClip;

    [Header("Auto Behaviour")]
    [SerializeField] private bool autoOpenWhenUnlocked = false;

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
        if (!hasUnlocked && AreAllSigilsInactive())
        {
            UnlockDoor();
        }
    }

    private void FixedUpdate()
    {
        if (door == null) return;

        float speed = rotationSpeed;

        if (autoOpenWhenUnlocked)
            speed *= 2f;

        door.localRotation = Quaternion.Lerp(
            door.localRotation,
            targetRotation,
            speed * Time.fixedDeltaTime
        );
    }

    public void Interact()
    {
        if (!hasUnlocked)
        {
            Log("Tried to interact but door is locked.");
            PlayLockedSound();
            return;
        }

        OpenDoor();
    }

    private void UnlockDoor()
    {
        if (hasUnlocked) return;

        hasUnlocked = true;

        if (navMeshObstacle != null)
        {
            navMeshObstacle.enabled = false;
            Log("NavMeshObstacle disabled. AI can pass.");
        }

        Log("Door unlocked.");

        if (autoOpenWhenUnlocked)
        {
            Log("Auto-open enabled. Opening door.");
            AutoOpenDoor();
        }
    }

    private void OpenDoor()
    {
        if (isOpen) return;

        targetRotation = DetermineOpenDirection();
        isOpen = true;

        Log("Door opening.");

        PlayOpenSound();
        EmitNormalSoundForPlayer();

        SetDoorToDefaultLayer();
    }

    private void AutoOpenDoor()
    {
        if (isOpen) return;

        Vector3 reference = transform.forward;
        float side = Vector3.Dot(door.forward, reference);

        targetRotation = (side > 0) ? openRotationB : openRotationA;
        isOpen = true;

        Log("Auto-opening door (opposite direction).");

        PlayAutoOpenSound();
        EmitNormalSoundForPlayer();

        SetDoorToDefaultLayer();
    }

    public void TriggerAutoOpenFromSpell()
    {
        if (!autoOpenWhenUnlocked)
            return;

        AutoOpenDoor();
    }

    private Quaternion DetermineOpenDirection()
    {
        if (player == null)
            return openRotationA;

        Vector3 dir = (player.position - transform.position).normalized;
        float side = Vector3.Dot(transform.forward, dir);

        return side > 0 ? openRotationB : openRotationA;
    }

    private void OpenDoorForEnemy(Transform enemy)
    {
        if (isOpen) return;

        targetRotation = DetermineOpenDirectionForEnemy(enemy);
        isOpen = true;

        if (navMeshObstacle != null)
            navMeshObstacle.enabled = false;

        Log($"Door opened for enemy: {enemy.name}");

        PlayOpenSound();

        SetDoorToDefaultLayer();
    }

    private Quaternion DetermineOpenDirectionForEnemy(Transform enemy)
    {
        Vector3 dir = (enemy.position - transform.position).normalized;
        float side = Vector3.Dot(transform.forward, dir);

        return side > 0 ? openRotationB : openRotationA;
    }

    private void PlayOpenSound() => PlayClip(openSoundClip);
    private void PlayAutoOpenSound() => PlayClip(autoOpenSoundClip);
    private void PlayLockedSound() => PlayClip(lockedSoundClip);

    private void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void EmitNormalSoundForPlayer()
    {
        if (enemyAudioEmitter != null && player != null)
            enemyAudioEmitter.EmitSound(EnemyAudioEmitter.SoundLevel.Normal);
    }

    private void SetDoorToDefaultLayer()
    {
        if (door == null) return;

        int defaultLayer = LayerMask.NameToLayer("Default");
        SetLayerRecursively(door, defaultLayer);

        Log("Door and children set to Default layer.");
    }

    private void SetLayerRecursively(Transform root, int layer)
    {
        root.gameObject.layer = layer;

        foreach (Transform child in root)
            SetLayerRecursively(child, layer);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            player = other.transform;
        }

        if (other.CompareTag("Enemy") && hasUnlocked)
        {
            OpenDoorForEnemy(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            isPlayerInRange = false;
    }

    private bool AreAllSigilsInactive()
    {
        if (sigilsParent == null)
            return true;

        foreach (Transform s in sigilsParent)
            if (s.gameObject.activeSelf)
                return false;

        return true;
    }

    private void Log(string msg)
    {
        if (enableDebugLogs)
            Debug.Log($"[DoorInteraction] {msg}", this);
    }
}