using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideSpot : MonoBehaviour, IInteractable
{
    [Header("Camera Positions")]
    [SerializeField] private Transform cameraHidePosition;
    [SerializeField] private Transform cameraExitPosition;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("References")]
    [SerializeField] private Camera mainCamera;

    [Header("Disable While Hiding")]
    [SerializeField] private List<MonoBehaviour> scriptsToDisable = new List<MonoBehaviour>();

    public bool IsPlayerHiding => isHiding;

    private bool isHiding = false;
    private Coroutine moveRoutine;

    private Vector3 originalCamPos;
    private Quaternion originalCamRot;

    public void Interact()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("HideSpot: No camera assigned!");
                return;
            }
        }

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(isHiding ? MoveToExit() : MoveToHide());
    }

    private IEnumerator MoveToHide()
    {
        isHiding = true;

        // Store original camera transform
        originalCamPos = mainCamera.transform.position;
        originalCamRot = mainCamera.transform.rotation;

        // Disable scripts (movement, look, etc.)
        foreach (var script in scriptsToDisable)
        {
            if (script != null)
                script.enabled = false;
        }

        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        Vector3 targetPos = cameraHidePosition.position;
        Quaternion targetRot = cameraHidePosition.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;

            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        moveRoutine = null;
    }

    private IEnumerator MoveToExit()
    {
        isHiding = false;

        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        Vector3 targetPos = cameraExitPosition != null ? cameraExitPosition.position : originalCamPos;
        Quaternion targetRot = cameraExitPosition != null ? cameraExitPosition.rotation : originalCamRot;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;

            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        // Re-enable scripts
        foreach (var script in scriptsToDisable)
        {
            if (script != null)
                script.enabled = true;
        }

        moveRoutine = null;
    }
}