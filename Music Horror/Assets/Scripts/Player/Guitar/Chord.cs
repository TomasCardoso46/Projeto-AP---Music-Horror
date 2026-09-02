using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
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

    [Header("Chord Animations")]
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private List<string> chordAnimationNames = new();

    private int currentIndex = 0;
    private int currentMode = 0;
    private const int MAX_MODES = 2;
    private string controllerInput;

    void Start()
    {
        if (guitarRenderer == null)
            guitarRenderer = GetComponentInChildren<Renderer>();

        if (targetPositions.Count == 0 || objectToMove == null)
            return;

        objectToMove.position = targetPositions[currentIndex].position;
    }

    void Update()
    {
        if (GameState.IsPaused)
        return;
        //UpdateGuitarMaterial();

        HandleModeSwitch();

        /*if (!CanUseGuitar())
            return;*/

        HandleChordSelection();

        HandleNumberShortcuts();
    }

    void HandleModeSwitch()
    {
        if (Input.GetKeyDown(KeyCode.R) || Gamepad.current.leftShoulder.wasPressedThisFrame)
        {
            modeSwitch.PlayReverse();
            modeSwitch.SphereSwitcher();
            currentMode++;

            if (audioSourceForSwitch != null && switchSound != null)
            {
                audioSourceForSwitch.PlayOneShot(switchSound);
            }


            if (currentMode >= MAX_MODES)
                currentMode = 0;

            sequenceManager.SetMode(currentMode);

            Debug.Log($"Switched Guitar Mode: {currentMode + 1}");
        }
    }

    void HandleChordSelection()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
            MoveToIndex(currentIndex - 1);

        else if (scroll < 0f)
            MoveToIndex(currentIndex + 1);
    }

    void HandleNumberShortcuts()
    {
        if(Gamepad.current.buttonWest.wasPressedThisFrame|| Input.GetKeyDown(KeyCode.Alpha1))
        {
            MoveToIndex(0);
            PlayCurrentSound();
        }
        else if (Gamepad.current.buttonSouth.wasPressedThisFrame || Input.GetKeyDown(KeyCode.Alpha2))
        {
            MoveToIndex(1);
            PlayCurrentSound();
        }
        else if (Gamepad.current.buttonEast.wasPressedThisFrame || Input.GetKeyDown(KeyCode.Alpha3))
        {
            MoveToIndex(2);
            PlayCurrentSound();
        }
        else if (Gamepad.current.buttonNorth.wasPressedThisFrame || Input.GetKeyDown(KeyCode.Alpha4))
        {
            MoveToIndex(3);
            PlayCurrentSound();
        }
        

        /*for (int i = 0; i < targetPositions.Count && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {

                MoveToIndex(i);
                PlayCurrentSound();
            }
        }*/
    }

    /*bool CanUseGuitar()
    {
        return !FindObjectOfType<SoundBait>();
    }

    void UpdateGuitarMaterial()
    {
        if (guitarRenderer == null)
            return;

        guitarRenderer.material =
            FindObjectOfType<SoundBait>()
            ? disabledMaterial
            : usableMaterial;
    }*/

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
            objectToMove.position =
                targetPositions[currentIndex].position;
        }
    }

    void PlayCurrentAnimation()
    {
        if (targetAnimator == null)
            return;

        if (currentIndex >= chordAnimationNames.Count)
            return;

        string animationName = chordAnimationNames[currentIndex];

        if (!string.IsNullOrEmpty(animationName))
        {
            targetAnimator.Play(animationName, 0, 0f);
        }
    }

    void PlayCurrentSound()
    {
        enemyAudioEmitter.EmitSound(SoundLevel.High, 1);

        if (currentMode >= modeSounds.Count)
            return;

        var activeModeSounds = modeSounds[currentMode].clips;

        if (currentIndex >= activeModeSounds.Count)
            return;

        if (sequenceManager != null)
            sequenceManager.RegisterChord(currentIndex + 1);

        AudioSource sourceInstance =
            Instantiate(audioSourcePrefab,
            transform.position,
            Quaternion.identity,
            transform);

        sourceInstance.clip =
            activeModeSounds[currentIndex];

        sourceInstance.Play();

        PlayCurrentAnimation();

        emitter.PlaySound(5);

        if (vfxController != null)
            vfxController.Pulse();

        Destroy(sourceInstance.gameObject,
            sourceInstance.clip.length);
    }
}

[System.Serializable]
public class ChordModeAudio
{
    public List<AudioClip> clips = new();
}