using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static EnemyAudioEmitter;

public class Chord : MonoBehaviour
{
    [Header("Melody Tracker")]
    [SerializeField] private ChordSequenceManager sequenceManager;

    [Header("Target Positions")]
    [SerializeField] private List<Transform> targetPositions = new();

    [Header("Object to Move")]
    [SerializeField] private Transform objectToMove;

    [Header("Mode-based Audio Clips")]
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

    [Header("Guitar Status UI")]
    [SerializeField] private Image guitarStatus;
    [SerializeField] private Sprite guitarModeImage;
    [SerializeField] private Sprite normalModeImage;

    [Header("Chord Animations")]
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private List<string> chordAnimationNames = new();

    [Header("Guitar Mode")]
    public bool GuitarMode { get; private set; }

    [Header("Gamepad")]
    public bool IsGamepadConnected { get; private set; }

    public event Action OnChordPlayed;
    public event Action OnGuitarModeChanged;

    private int currentIndex = 0;
    private int currentMode = 0;
    private const int MAX_MODES = 2;

    private void Start()
    {
        if (guitarRenderer == null)
            guitarRenderer = GetComponentInChildren<Renderer>();

        if (targetPositions.Count > 0 && objectToMove != null)
            objectToMove.position = targetPositions[currentIndex].position;

        UpdateGamepadState();
        UpdateGuitarStatusUI();
    }

    private void Update()
    {
        if (GameState.IsPaused)
            return;

        UpdateGamepadState();
        UpdateGuitarMode();

        HandleModeSwitch();

        if (!GuitarMode)
            return;

        HandleChordSelection();
        HandleNumberShortcuts();
    }

    private void UpdateGamepadState()
    {
        IsGamepadConnected = Gamepad.current != null;

        if (!IsGamepadConnected)
        {
            SetGuitarMode(false);
        }
    }

    private void UpdateGuitarMode()
    {
        if (!IsGamepadConnected)
        {
            SetGuitarMode(false);
            return;
        }

        bool newGuitarMode =
            Gamepad.current.leftTrigger.isPressed ||
            Input.GetMouseButton(1);

        SetGuitarMode(newGuitarMode);
    }

    private void SetGuitarMode(bool newValue)
    {
        if (GuitarMode == newValue)
            return;

        GuitarMode = newValue;

        UpdateGuitarStatusUI();
        OnGuitarModeChanged?.Invoke();
    }

    private void UpdateGuitarStatusUI()
    {
        if (guitarStatus == null)
            return;

        guitarStatus.sprite = GuitarMode
            ? guitarModeImage
            : normalModeImage;
    }

    private void HandleModeSwitch()
    {
        if (Input.GetKeyDown(KeyCode.R) ||
            (Gamepad.current != null &&
             Gamepad.current.buttonEast.wasPressedThisFrame &&
             !GuitarMode))
        {
            modeSwitch.PlayReverse();
            modeSwitch.SphereSwitcher();

            currentMode++;

            if (audioSourceForSwitch != null && switchSound != null)
                audioSourceForSwitch.PlayOneShot(switchSound);

            if (currentMode >= MAX_MODES)
                currentMode = 0;

            if (sequenceManager != null)
                sequenceManager.SetMode(currentMode);

            OnGuitarModeChanged?.Invoke();

            Debug.Log($"Switched Guitar Mode: {currentMode + 1}");
        }
    }

    private void HandleChordSelection()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
            MoveToIndex(currentIndex - 1);
        else if (scroll < 0f)
            MoveToIndex(currentIndex + 1);
    }

    private void HandleNumberShortcuts()
    {
        if ((Gamepad.current != null &&
             Gamepad.current.buttonWest.wasPressedThisFrame) ||
            Input.GetKeyDown(KeyCode.Alpha1))
        {
            MoveToIndex(0);
            PlayCurrentSound();
        }
        else if ((Gamepad.current != null &&
                  Gamepad.current.buttonSouth.wasPressedThisFrame) ||
                 Input.GetKeyDown(KeyCode.Alpha2))
        {
            MoveToIndex(1);
            PlayCurrentSound();
        }
        else if ((Gamepad.current != null &&
                  Gamepad.current.buttonEast.wasPressedThisFrame) ||
                 Input.GetKeyDown(KeyCode.Alpha3))
        {
            MoveToIndex(2);
            PlayCurrentSound();
        }
        else if ((Gamepad.current != null &&
                  Gamepad.current.buttonNorth.wasPressedThisFrame) ||
                 Input.GetKeyDown(KeyCode.Alpha4))
        {
            MoveToIndex(3);
            PlayCurrentSound();
        }
    }

    private void MoveToIndex(int newIndex)
    {
        int count = targetPositions.Count;

        if (count == 0 || objectToMove == null)
            return;

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

    private void PlayCurrentAnimation()
    {
        if (targetAnimator == null)
            return;

        if (currentIndex >= chordAnimationNames.Count)
            return;

        string animationName = chordAnimationNames[currentIndex];

        if (!string.IsNullOrEmpty(animationName))
            targetAnimator.Play(animationName, 0, 0f);
    }

    private void PlayCurrentSound()
    {
        if (enemyAudioEmitter != null)
            enemyAudioEmitter.EmitSound(SoundLevel.High, 1);

        if (currentMode >= modeSounds.Count)
            return;

        var activeModeSounds = modeSounds[currentMode].clips;

        if (currentIndex >= activeModeSounds.Count)
            return;

        if (sequenceManager != null)
            sequenceManager.RegisterChord(currentIndex + 1);

        AudioSource sourceInstance = Instantiate(
            audioSourcePrefab,
            transform.position,
            Quaternion.identity,
            transform
        );

        sourceInstance.clip = activeModeSounds[currentIndex];
        sourceInstance.Play();

        OnChordPlayed?.Invoke();

        PlayCurrentAnimation();

        if (emitter != null)
            emitter.PlaySound(5);

        if (vfxController != null)
            vfxController.Pulse();

        Destroy(
            sourceInstance.gameObject,
            sourceInstance.clip.length
        );
    }
}

[System.Serializable]
public class ChordModeAudio
{
    public List<AudioClip> clips = new();
}