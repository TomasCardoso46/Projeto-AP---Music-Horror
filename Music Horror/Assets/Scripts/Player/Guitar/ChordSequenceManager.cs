using System.Collections.Generic;
using UnityEngine;

public class ChordSequenceManager : MonoBehaviour
{
    [Header("List of Possible Spell Patterns")]
    [SerializeField] private List<string> spellSequences = new List<string>();

    [Header("List of Spell ScriptableObjects (Matches index of Spell Pattern)")]
    [SerializeField] private List<Spell> spells = new List<Spell>();

    [Header("List of GameObjects to Activate on Match (Optional, Same Index as Patterns)")]
    [SerializeField] private List<GameObject> objectsToActivate = new List<GameObject>();

    [Header("Debug - Current Sequence")]
    [SerializeField] private string currentSequence = "";

    [Header("Reset Timer (seconds)")]
    [SerializeField] private float resetDelay = 3f;

    private List<string> playedChords = new List<string>();
    private float timer = 0f;
    private bool timerRunning = false;

    void Update()
    {
        if (timerRunning)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                ResetSequence();
            }
        }
    }

    public void RegisterChord(int chordIndex)
    {
        playedChords.Add(chordIndex.ToString());
        currentSequence = string.Join("", playedChords);

        timer = resetDelay;
        timerRunning = true;

        CheckForSpellMatch();
    }

    private void CheckForSpellMatch()
    {
        for (int i = 0; i < spellSequences.Count; i++)
        {
            if (currentSequence == spellSequences[i])
            {
                // CAST SPELL
                if (i < spells.Count && spells[i] != null)
                {
                    spells[i].Cast(Camera.main.transform);
                }

                // ACTIVATE ASSOCIATED GAMEOBJECT (if any)
                if (i < objectsToActivate.Count && objectsToActivate[i] != null)
                {
                    if (!objectsToActivate[i].activeSelf)
                    {
                        objectsToActivate[i].SetActive(true);
                        Debug.Log($"Activated object: {objectsToActivate[i].name}");
                    }
                }

                ResetSequence();
                return;
            }
        }
    }

    private void ResetSequence()
    {
        playedChords.Clear();
        currentSequence = "";
        timerRunning = false;
        timer = 0f;
        Debug.Log("Sequence reset after inactivity or match.");
    }
}
