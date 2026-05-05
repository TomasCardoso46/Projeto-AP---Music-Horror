using System.Collections.Generic;
using UnityEngine;

public class SequenceLockController : MonoBehaviour
{
    private HashSet<string> lockedSequences = new HashSet<string>();

    public void LockSequence(string sequence)
    {
        if (!lockedSequences.Contains(sequence))
        {
            lockedSequences.Add(sequence);
            Debug.Log("Sequence locked: " + sequence);
        }
    }

    public void UnlockSequence(string sequence)
    {
        if (lockedSequences.Contains(sequence))
        {
            lockedSequences.Remove(sequence);
            Debug.Log("Sequence unlocked: " + sequence);
        }
    }

    public bool IsSequenceLocked(string sequence)
    {
        return lockedSequences.Contains(sequence);
    }
}