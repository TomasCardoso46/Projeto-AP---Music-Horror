using System.Collections.Generic;
using UnityEngine;

public class Chord : MonoBehaviour
{
    [Header("List of Target Transforms")]
    [SerializeField] private List<Transform> targetPositions = new List<Transform>();

    [Header("Object to Move")]
    [SerializeField] private Transform objectToMove;

    [Header("Audio Clips Per Position")]
    [SerializeField] private List<AudioClip> positionSounds = new List<AudioClip>();

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    private int currentIndex = 0;

    void Start()
    {
        if (targetPositions.Count == 0)
        {
            Debug.LogWarning("Target positions list is empty!");
            return;
        }

        if (objectToMove == null)
        {
            Debug.LogWarning("No object assigned to move!");
            return;
        }

        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource assigned! Please assign one.");
            return;
        }

        // Instantly move to the first position
        currentIndex = 0;
        objectToMove.position = targetPositions[currentIndex].position;
    }

    void Update()
    {
        if (targetPositions.Count == 0 || objectToMove == null)
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f) // Scroll up
        {
            MoveToIndex(currentIndex - 1);
        }
        else if (scroll < 0f) // Scroll down
        {
            MoveToIndex(currentIndex + 1);
        }

        if (Input.GetMouseButtonDown(0)) // Left mouse click
        {
            PlayCurrentSound();
        }
    }

    void MoveToIndex(int newIndex)
    {
        int count = targetPositions.Count;

        // Wrap-around behavior
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
        if (audioSource == null || positionSounds.Count == 0)
            return;

        if (currentIndex < positionSounds.Count && positionSounds[currentIndex] != null)
        {
            audioSource.clip = positionSounds[currentIndex];
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"No audio clip assigned for position index {currentIndex}!");
        }
    }
}
