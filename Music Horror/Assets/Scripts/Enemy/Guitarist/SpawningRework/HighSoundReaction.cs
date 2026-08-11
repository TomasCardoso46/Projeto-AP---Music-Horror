using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighSoundReaction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyAudioEmitter soundEmitter;
    [SerializeField] private AudioSource audioSource;

    [Header("High Sound Audio")]
    [SerializeField] private List<AudioClip> highSoundClips = new();

    [SerializeField] private float cooldown = 2f;

    [Header("Teleport")]
    [SerializeField] private GameObject objectToTeleport;
    [SerializeField] private List<Transform> spawnPoints = new();

    private int currentClipIndex = 0;
    private bool onCooldown = false;

    private void Awake()
    {
        if (soundEmitter == null)
            soundEmitter = GetComponent<EnemyAudioEmitter>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        if (soundEmitter != null)
            soundEmitter.OnSoundEmitted += HandleSoundEmitted;
    }

    private void OnDisable()
    {
        if (soundEmitter != null)
            soundEmitter.OnSoundEmitted -= HandleSoundEmitted;
    }

    private void HandleSoundEmitted(EnemyAudioEmitter.SoundLevel level)
    {
        if (level != EnemyAudioEmitter.SoundLevel.High)
            return;

        if (onCooldown)
            return;

        PlayNextSound();
    }

    private void PlayNextSound()
    {
        if (highSoundClips.Count == 0)
        {
            Debug.LogWarning($"{name}: No high sound clips have been assigned.");
            return;
        }

        if (currentClipIndex >= highSoundClips.Count)
            return;

        AudioClip clip = highSoundClips[currentClipIndex];

        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }

        currentClipIndex++;

        StartCoroutine(CooldownRoutine());

        if (currentClipIndex >= highSoundClips.Count)
        {
            TeleportObjectToClosestSpawnPoint();
        }
    }

    private IEnumerator CooldownRoutine()
    {
        onCooldown = true;

        yield return new WaitForSeconds(cooldown);

        onCooldown = false;
    }

    private void TeleportObjectToClosestSpawnPoint()
    {
        if (objectToTeleport == null)
        {
            Debug.LogWarning($"{name}: No object has been assigned to teleport.");
            return;
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning($"{name}: No spawn points have been assigned.");
            return;
        }

        Transform closestSpawn = FindClosestSpawnPoint();

        if (closestSpawn == null)
            return;

        objectToTeleport.transform.position = closestSpawn.position;

        objectToTeleport.SetActive(true);
    }

    private Transform FindClosestSpawnPoint()
    {
        if (soundEmitter == null)
            return null;

        return GetClosestSpawnPoint(soundEmitter.transform.position);
    }

    public Transform GetClosestSpawnPoint(Vector3 position)
    {
        Transform closestSpawn = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint == null)
                continue;

            float distanceSqr =
                (spawnPoint.position - position).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestSpawn = spawnPoint;
            }
        }

        return closestSpawn;
    }

    public void ResetReaction()
    {
        // Stop any active cooldown
        //StopAllCoroutines();

        currentClipIndex = 0;
        onCooldown = false;
    }
}