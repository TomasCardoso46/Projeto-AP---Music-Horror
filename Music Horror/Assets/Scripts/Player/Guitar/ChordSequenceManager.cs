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

    private float lastChordTime;
    private int currentMode = 0;

    private List<string> playedChords = new List<string>();

    private const int REQUIRED_CHORDS = 4;

    void Update()
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

    void CheckForSpellMatch()
    {
        if (currentMode >= spellModes.Count)
        {
            ResetSequence();
            return;
        }

        string sequence = string.Join("", playedChords);
        var modeSet = spellModes[currentMode];

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
                ResetSequence();
                return;
            }
        }

        // no match
        ResetSequence();
    }

    void CastSpell(int index)
    {
        var modeSet = spellModes[currentMode];

        if (index < modeSet.spells.Count &&
            modeSet.spells[index] != null)
        {
            guitarEmission.TriggerSpellGlow(
                modeSet.spells[index].spellColor,
                emissionHoldTime,
                emissionFadeTime
            );

            modeSet.spells[index]
                .Cast(Camera.main.transform);

            SpawnModePrefab(modeSet);
        }

        if (index < modeSet.objectsToActivate.Count &&
            modeSet.objectsToActivate[index] != null)
        {
            modeSet.objectsToActivate[index]
                .SetActive(true);
        }
    }

    void SpawnModePrefab(SpellModeSet modeSet)
    {
        if (modeSet.spawnPrefab == null)
            return;

        Transform parent = modeSet.spawnParent != null
            ? modeSet.spawnParent
            : transform;

        GameObject instance = Instantiate(modeSet.spawnPrefab, parent);

        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        Destroy(instance, modeSet.prefabLifetime);
    }

    void ResetSequence()
    {
        playedChords.Clear();
    }
}