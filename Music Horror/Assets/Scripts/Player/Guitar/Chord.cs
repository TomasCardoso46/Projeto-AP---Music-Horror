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

    [Header("Cooldown Visual Objects")]
    [SerializeField] private List<GameObject> cooldownObjects = new List<GameObject>();

    [Header("Cooldown Timer Settings")]
    [SerializeField] private float cooldownDuration = 2f;

    [Header("Guitar Usability Visuals")]
    [SerializeField] private Material usableMaterial;
    [SerializeField] private Material disabledMaterial;
    [SerializeField] private Renderer guitarRenderer;

    private int currentIndex = 0;

    void Start()
    {
        if (guitarRenderer == null)
            guitarRenderer = GetComponentInChildren<Renderer>();

        if (targetPositions.Count == 0 || objectToMove == null || audioSource == null)
            return;

        currentIndex = 0;
        objectToMove.position = targetPositions[currentIndex].position;

        foreach (var obj in cooldownObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    void Update()
    {
        UpdateGuitarMaterial();

        if (!CanUseGuitar())
            return;

        // Mouse scroll input
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
            MoveToIndex(currentIndex - 1);
        else if (scroll < 0f)
            MoveToIndex(currentIndex + 1);

        // Mouse click
        if (Input.GetMouseButtonDown(0))
            PlayCurrentSound();

        // Keyboard input for chord selection
        for (int i = 0; i < targetPositions.Count && i < 9; i++)
        {
            // KeyCode.Alpha1 corresponds to "1" key, Alpha2 to "2", etc.
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                MoveToIndex(i);
                PlayCurrentSound();
            }
        }
    }

    bool CanUseGuitar()
    {
        return !IsSoundBaitPresent();
    }

    bool IsSoundBaitPresent()
    {
        return FindObjectOfType<SoundBait>() != null;
    }

    void UpdateGuitarMaterial()
    {
        if (guitarRenderer == null)
            return;

        if (IsSoundBaitPresent())
            guitarRenderer.material = disabledMaterial;
        else
            guitarRenderer.material = usableMaterial;
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
        if (!CanUseGuitar())
            return;

        enemyAudioEmitter.EmitSound(SoundLevel.High, 3);

        if (audioSource == null || positionSounds.Count == 0)
            return;

        if (sequenceManager != null)
            sequenceManager.RegisterChord(currentIndex + 1);

        if (currentIndex < positionSounds.Count && positionSounds[currentIndex] != null)
        {
            audioSource.clip = positionSounds[currentIndex];
            audioSource.Play();
            emitter.PlaySound(5);
        }

        ActivateCooldownObject(currentIndex);
    }

    void ActivateCooldownObject(int index)
    {
        if (index >= cooldownObjects.Count || cooldownObjects[index] == null)
            return;

        GameObject cooldownObj = cooldownObjects[index];

        cooldownObj.SetActive(true);
        SetObjectAlpha(cooldownObj, 1f);

        StartCoroutine(FadeOutCooldownObject(cooldownObj));
    }

    System.Collections.IEnumerator FadeOutCooldownObject(GameObject obj)
    {
        float timer = 0f;

        Renderer rend = obj.GetComponent<Renderer>();
        if (rend == null)
            yield break;

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
