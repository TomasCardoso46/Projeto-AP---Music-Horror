using System.Collections.Generic;
using UnityEngine;
using static EnemyAudioEmitter;

public class Chord : MonoBehaviour
{
    [Header("Melody Tracker")]
    [SerializeField] private ChordSequenceManager sequenceManager;

    [Header("Target Positions")]
    [SerializeField] private List<Transform> targetPositions = new();

    [Header("Object To Move")]
    [SerializeField] private Transform objectToMove;

    [Header("Mode Audio")]
    [SerializeField] private List<ChordModeAudio> modeSounds = new();

    [Header("Audio")]
    [SerializeField] private AudioSource audioSourcePrefab;
    [SerializeField] private AudioSource audioSourceForSwitch;
    [SerializeField] private AudioClip switchSound;
    [SerializeField] private SoundEmitter emitter;
    [SerializeField] private EnemyAudioEmitter enemyAudioEmitter;

    [Header("Guitar Visuals")]
    [SerializeField] private Material usableMaterial;
    [SerializeField] private Material disabledMaterial;
    [SerializeField] private Renderer guitarRenderer;

    [SerializeField] private ModeSwitch modeSwitch;
    [SerializeField] private VFXIntensityController vfxController;

    [Header("Animations")]
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private List<string> chordAnimationNames = new();

    private int currentChordIndex = 0;
    private int currentMode = 0;

    private const int MAX_MODES = 2;

    void Start()
    {
        if (guitarRenderer == null)
            guitarRenderer = GetComponentInChildren<Renderer>();

        if (objectToMove != null &&
            targetPositions.Count > 0)
        {
            objectToMove.position = targetPositions[0].position;
        }
    }

    void Update()
    {
        if (GameState.IsPaused)
            return;

        HandleModeSwitch();
    }

    void HandleModeSwitch()
    {
        if (!Input.GetKeyDown(KeyCode.R))
            return;

        modeSwitch.PlayReverse();
        modeSwitch.SphereSwitcher();

        currentMode++;

        if (currentMode >= MAX_MODES)
            currentMode = 0;

        if (sequenceManager != null)
            sequenceManager.SetMode(currentMode);

        if (audioSourceForSwitch != null &&
            switchSound != null)
        {
            audioSourceForSwitch.PlayOneShot(switchSound);
        }

        Debug.Log($"Switched Guitar Mode: {currentMode + 1}");
    }

    /// <summary>
    /// Plays a chord.
    /// chordNumber is 1-5.
    /// </summary>
    public void PlayChord(int chordNumber)
    {
        int index = chordNumber - 1;

        if (index < 0)
            return;

        if (index >= targetPositions.Count)
            return;

        currentChordIndex = index;

        if (objectToMove != null)
        {
            objectToMove.position =
                targetPositions[currentChordIndex].position;
        }

        PlayCurrentSound();
    }

    void PlayCurrentAnimation()
    {
        if (targetAnimator == null)
            return;

        if (currentChordIndex >= chordAnimationNames.Count)
            return;

        string animationName =
            chordAnimationNames[currentChordIndex];

        if (string.IsNullOrWhiteSpace(animationName))
            return;

        targetAnimator.Play(animationName, 0, 0f);
    }

    void PlayCurrentSound()
    {
        enemyAudioEmitter.EmitSound(
            SoundLevel.High,
            3);

        if (currentMode >= modeSounds.Count)
            return;

        List<AudioClip> clips =
            modeSounds[currentMode].clips;

        if (currentChordIndex >= clips.Count)
            return;

        if (sequenceManager != null)
        {
            sequenceManager.RegisterChord(
                currentChordIndex + 1);
        }

        AudioSource source =
            Instantiate(
                audioSourcePrefab,
                transform.position,
                Quaternion.identity,
                transform);

        source.clip = clips[currentChordIndex];
        source.Play();

        PlayCurrentAnimation();

        emitter.PlaySound(5);

        if (vfxController != null)
            vfxController.Pulse();

        Destroy(
            source.gameObject,
            source.clip.length);
    }

    public int GetCurrentMode()
    {
        return currentMode;
    }
}

[System.Serializable]
public class ChordModeAudio
{
    public List<AudioClip> clips = new();
}