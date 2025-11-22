using System.Collections.Generic;
using UnityEngine;
using static EnemyAudioEmitter;

public class Chord : MonoBehaviour
{
    [Header("Melody Tracker")]
    [SerializeField] private ChordSequenceManager sequenceManager;

    [Header("List of Target Transforms")]
    [SerializeField] private List<Transform> targetPositions = new List<Transform>();

    [Header("Object to Move")]
    [SerializeField] private Transform objectToMove;

    [Header("Audio Clips Per Position")]
    [SerializeField] private List<AudioClip> positionSounds = new List<AudioClip>();

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private SoundEmitter emitter;
    [SerializeField] private EnemyAudioEmitter enemyAudioEmitter;

    [Header("Cooldown Visual Objects (Match Indices)")]
    [SerializeField] private List<GameObject> cooldownObjects = new List<GameObject>();

    [Header("Cooldown Timer Settings")]
    [SerializeField] private float cooldownDuration = 2f;

    private int currentIndex = 0;

    void Start()
    {
        if (targetPositions.Count == 0)
        {
            Debug.LogWarning("Target positions list is empty!");
            return;
        }

        if (objectToMove == null)
        {
            Debug.LogWarning("No object assigned to move!");
            return;
        }

        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource assigned! Please assign one.");
            return;
        }

        // Instantly move to the first position
        currentIndex = 0;
        objectToMove.position = targetPositions[currentIndex].position;

        // Disable all cooldown visual objects at start
        foreach (var obj in cooldownObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    void Update()
    {
        if (targetPositions.Count == 0 || objectToMove == null)
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f) 
        {
            MoveToIndex(currentIndex - 1);
        }
        else if (scroll < 0f) 
        {
            MoveToIndex(currentIndex + 1);
        }

        if (Input.GetMouseButtonDown(0)) 
        {
            PlayCurrentSound();
        }
    }

    void MoveToIndex(int newIndex)
    {
        int count = targetPositions.Count;

        if (newIndex < 0)
            newIndex = count - 1;
        else if (newIndex >= count)
            newIndex = 0;

        if (newIndex != currentIndex)
        {
            currentIndex = newIndex;
            objectToMove.position = targetPositions[currentIndex].position;
        }
    }

    void PlayCurrentSound()
    {
        enemyAudioEmitter.EmitSound(SoundLevel.High, 3);

        if (audioSource == null || positionSounds.Count == 0)
            return;

        if (sequenceManager != null)
        {
            sequenceManager.RegisterChord(currentIndex + 1);
        }

        // Play Audio
        if (currentIndex < positionSounds.Count && positionSounds[currentIndex] != null)
        {
            audioSource.clip = positionSounds[currentIndex];
            audioSource.Play();
            emitter.PlaySound(5);
        }
        else
        {
            Debug.LogWarning($"No audio clip assigned for position index {currentIndex}!");
        }

        // Trigger cooldown object fade-out
        ActivateCooldownObject(currentIndex);
    }

    void ActivateCooldownObject(int index)
    {
        if (index >= cooldownObjects.Count || cooldownObjects[index] == null)
            return;

        GameObject cooldownObj = cooldownObjects[index];

        // Turn object on and fully visible
        cooldownObj.SetActive(true);

        // Reset alpha to full
        SetObjectAlpha(cooldownObj, 1f);

        // Start fade coroutine
        StartCoroutine(FadeOutCooldownObject(cooldownObj));
    }

    System.Collections.IEnumerator FadeOutCooldownObject(GameObject obj)
    {
        float timer = 0f;

        Renderer rend = obj.GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning("Cooldown object has no Renderer! Cannot fade.");
            yield break;
        }

        Material mat = rend.material;

        while (timer < cooldownDuration)
        {
            timer += Time.deltaTime;

            float t = 1f - (timer / cooldownDuration);
            Color c = mat.color;
            c.a = t;
            mat.color = c;

            yield return null;
        }

        // Fully invisible → cooldown ended
        obj.SetActive(false);
    }

    void SetObjectAlpha(GameObject obj, float alpha)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend == null) return;

        Material mat = rend.material;
        Color c = mat.color;
        c.a = alpha;
        mat.color = c;
    }
}
