using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpellModeSet
{
    [Header("Spell Data")]
    public List<string> spellSequences;
    public List<Spell> spells;
    public List<GameObject> objectsToActivate;

    [Header("VFX (Per Mode)")]
    public GameObject spawnPrefab;
    public Transform spawnParent;
    public float prefabLifetime = 2f;
}

public class ChordSequenceManager : MonoBehaviour
{
    [Header("Spell Modes")]
    [SerializeField] private List<SpellModeSet> spellModes;

    [Header("Sequence Lock")]
    [SerializeField] private SequenceLockController sequenceLockController;

    [Header("Emission")]
    [SerializeField] private GuitarEmissionController guitarEmission;

    [SerializeField] private float emissionHoldTime = 0.2f;
    [SerializeField] private float emissionFadeTime = 0.3f;

    [Header("Timing")]
    [SerializeField] private float sequenceTimeout = 3f;

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

    private float lastChordTime;
    private int currentMode = 0;

    private List<string> playedChords = new();

    private const int REQUIRED_CHORDS = 4;

    private void Update()
    {
        if (playedChords.Count > 0)
        {
            if (Time.time - lastChordTime > sequenceTimeout)
            {
                ResetSequence();
            }
        }
    }

    public void SetMode(int mode)
    {
        currentMode = mode;
        ResetSequence();
    }

    public void RegisterChord(int chordIndex)
    {
        playedChords.Add(chordIndex.ToString());
        lastChordTime = Time.time;

        if (playedChords.Count == REQUIRED_CHORDS)
        {
            CheckForSpellMatch();
        }
    }

    private void CheckForSpellMatch()
    {
        if (currentMode >= spellModes.Count)
        {
            ResetSequence();
            return;
        }

        string sequence = string.Join("", playedChords);
        SpellModeSet modeSet = spellModes[currentMode];

        for (int i = 0; i < modeSet.spellSequences.Count; i++)
        {
            if (sequence == modeSet.spellSequences[i])
            {
                if (sequenceLockController != null &&
                    sequenceLockController.IsSequenceLocked(sequence))
                {
                    ResetSequence();
                    return;
                }

                CastSpell(i);
                TryPlayCorrectVoice();
                ResetSequence();
                return;
            }
        }

        // No match
        TryPlayIncorrectVoice();
        ResetSequence();
    }

    private void CastSpell(int index)
    {
        SpellModeSet modeSet = spellModes[currentMode];

        if (index < modeSet.spells.Count &&
            modeSet.spells[index] != null)
        {
            guitarEmission.TriggerSpellGlow(
                modeSet.spells[index].spellColor,
                emissionHoldTime,
                emissionFadeTime
            );

            modeSet.spells[index].Cast(Camera.main.transform);

            SpawnModePrefab(modeSet);
        }

        if (index < modeSet.objectsToActivate.Count &&
            modeSet.objectsToActivate[index] != null)
        {
            GameObject unlockObject = modeSet.objectsToActivate[index];

            bool wasInactive = !unlockObject.activeSelf;

            unlockObject.SetActive(true);

            if (wasInactive &&
                audioSource != null &&
                unlockSound != null)
            {
                audioSource.PlayOneShot(unlockSound);
            }
        }
    }

    private void SpawnModePrefab(SpellModeSet modeSet)
    {
        if (modeSet.spawnPrefab == null)
            return;

        Transform parent = modeSet.spawnParent != null
            ? modeSet.spawnParent
            : transform;

        GameObject instance = Instantiate(
            modeSet.spawnPrefab,
            parent
        );

        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        Destroy(instance, modeSet.prefabLifetime);
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

    private void ResetSequence()
    {
        playedChords.Clear();
    }
}