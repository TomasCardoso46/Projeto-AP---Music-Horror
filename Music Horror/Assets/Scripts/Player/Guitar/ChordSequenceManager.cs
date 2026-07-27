using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpellEntry
{
    [Header("Sequence")]
    [Tooltip("Example: a1a2a3a4 or a1b2a3b4")]
    public string sequence;

    [Header("Spell")]
    public Spell spell;

    [Header("Unlock Object")]
    public GameObject objectToActivate;
}

public class ChordSequenceManager : MonoBehaviour
{
    [Header("Spells")]
    [SerializeField] private List<SpellEntry> spells = new();

    [Header("Sequence Lock")]
    [SerializeField] private SequenceLockController sequenceLockController;

    [Header("Emission")]
    [SerializeField] private GuitarEmissionController guitarEmission;

    [SerializeField] private float emissionHoldTime = 0.2f;
    [SerializeField] private float emissionFadeTime = 0.3f;

    [Header("Timing")]
    [SerializeField] private float sequenceTimeout = 3f;

    [Header("Spell Cast VFX")]
    [SerializeField] private GameObject spellCastPrefab;
    [SerializeField] private Transform spellCastParent;
    [SerializeField] private float spellCastLifetime = 2f;

    [Header("Unlock Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip unlockSound;

    [Header("Voice Lines")]
    [SerializeField] private AudioSource voiceSource;

    [SerializeField, Range(0f, 100f)]
    private float correctVoiceChance = 30f;

    [SerializeField, Range(0f, 100f)]
    private float incorrectVoiceChance = 30f;

    [SerializeField] private List<AudioClip> correctVoiceLines = new();
    [SerializeField] private List<AudioClip> incorrectVoiceLines = new();

    private readonly List<string> playedChords = new();

    private float lastChordTime;

    private const int REQUIRED_CHORDS = 4;

    private void Update()
    {
        if (playedChords.Count > 0 &&
            Time.time - lastChordTime > sequenceTimeout)
        {
            ResetSequence();
        }
    }

    // Kept for compatibility with Chord.cs.
    // We no longer separate spells by mode.
    public void SetMode(int mode)
    {
    }

    public void RegisterChord(int chordIndex, int mode)
    {
        string modePrefix = mode == 0 ? "a" : "b";

        playedChords.Add($"{modePrefix}{chordIndex}");

        lastChordTime = Time.time;

        if (playedChords.Count == REQUIRED_CHORDS)
        {
            CheckForSpellMatch();
        }
    }

    private void CheckForSpellMatch()
    {
        string sequence = string.Concat(playedChords);

        for (int i = 0; i < spells.Count; i++)
        {
            SpellEntry entry = spells[i];

            if (sequence != entry.sequence)
                continue;

            if (sequenceLockController != null &&
                sequenceLockController.IsSequenceLocked(sequence))
            {
                ResetSequence();
                return;
            }

            CastSpell(entry);

            TryPlayCorrectVoice();

            ResetSequence();

            return;
        }

        TryPlayIncorrectVoice();

        ResetSequence();
    }
        private void CastSpell(SpellEntry entry)
    {
        if (entry.spell != null)
        {
            if (guitarEmission != null)
            {
                guitarEmission.TriggerSpellGlow(
                    entry.spell.spellColor,
                    emissionHoldTime,
                    emissionFadeTime
                );
            }

            entry.spell.Cast(Camera.main.transform);

            SpawnSpellPrefab();
        }

        if (entry.objectToActivate != null)
        {
            bool wasInactive = !entry.objectToActivate.activeSelf;

            entry.objectToActivate.SetActive(true);

            if (wasInactive &&
                audioSource != null &&
                unlockSound != null)
            {
                audioSource.PlayOneShot(unlockSound);
            }
        }
    }

    private void SpawnSpellPrefab()
    {
        if (spellCastPrefab == null)
            return;

        Transform parent = spellCastParent != null
            ? spellCastParent
            : transform;

        GameObject instance = Instantiate(
            spellCastPrefab,
            parent
        );

        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        Destroy(instance, spellCastLifetime);
    }

    private void TryPlayCorrectVoice()
    {
        if (voiceSource == null)
            return;

        if (voiceSource.isPlaying)
            return;

        if (correctVoiceLines.Count == 0)
            return;

        if (Random.Range(0f, 100f) > correctVoiceChance)
            return;

        AudioClip clip = correctVoiceLines[
            Random.Range(0, correctVoiceLines.Count)
        ];

        voiceSource.PlayOneShot(clip);
    }

    private void TryPlayIncorrectVoice()
    {
        if (voiceSource == null)
            return;

        if (voiceSource.isPlaying)
            return;

        if (incorrectVoiceLines.Count == 0)
            return;

        if (Random.Range(0f, 100f) > incorrectVoiceChance)
            return;

        AudioClip clip = incorrectVoiceLines[
            Random.Range(0, incorrectVoiceLines.Count)
        ];

        voiceSource.PlayOneShot(clip);
    }

    public void ResetCurrentSequence()
    {
        ResetSequence();
    }

    private void ResetSequence()
    {
        playedChords.Clear();
    }
}