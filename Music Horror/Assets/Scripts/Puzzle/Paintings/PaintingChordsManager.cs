using System.Collections.Generic;
using UnityEngine;

public class PaintingChordsManager : MonoBehaviour
{
    [SerializeField] private List<PaintingChordsDetector> paintings = new();

    private int currentQueuePosition = 0;

    private void Awake()
    {
        foreach (PaintingChordsDetector painting in paintings)
        {
            painting.Initialize(this);
        }

        ResetPuzzle();
    }

    public bool TryInteract(PaintingChordsDetector painting)
    {
        if (painting.QueuePosition == currentQueuePosition)
        {
            painting.CorrectInteraction();

            currentQueuePosition++;

            return true;
        }

        ResetPuzzle();
        return false;
    }

    public void ResetPuzzle()
    {
        currentQueuePosition = 0;

        foreach (PaintingChordsDetector painting in paintings)
        {
            painting.ResetPainting();
        }
    }
}