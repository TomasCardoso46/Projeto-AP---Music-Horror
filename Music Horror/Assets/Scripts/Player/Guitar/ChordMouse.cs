using System.Collections.Generic;
using UnityEngine;

public class ChordMouse : MonoBehaviour
{
    [Header("List of Target Transforms")]
    [SerializeField] private List<Transform> targetPositions = new List<Transform>();

    [Header("Object to Move")]
    [SerializeField] private Transform objectToMove;

    [Header("Audio Clips Per Position")]
    [SerializeField] private List<AudioClip> positionSounds = new List<AudioClip>();

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Mouse Sensitivity")]
    [SerializeField] private float moveThreshold = 50f; // How far you need to move before switching positions

    private int currentIndex = 0;
    private float accumulatedMouseMovement = 0f;

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

        float mouseX = Input.GetAxis("Mouse X");
        accumulatedMouseMovement += mouseX;

        // Check if accumulated mouse movement passes the threshold
        if (accumulatedMouseMovement >= moveThreshold)
        {
            MoveToIndex(currentIndex + 1);
            accumulatedMouseMovement = 0f;
        }
        else if (accumulatedMouseMovement <= -moveThreshold)
        {
            MoveToIndex(currentIndex - 1);
            accumulatedMouseMovement = 0f;
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
