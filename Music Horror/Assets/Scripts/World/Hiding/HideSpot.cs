using System.Collections;
using UnityEngine;

public class HideSpot : MonoBehaviour
{
    [Header("Positions")]
    [SerializeField] private Transform playerHidePosition;   // Where the player hides
    [SerializeField] private Transform playerExitPosition;   // Where the player exits
    [SerializeField] private Transform cameraHidePosition;   // Where the camera should be while hiding

    [Header("Settings")]
    [SerializeField] private KeyCode hideKey = KeyCode.E;
    [SerializeField] private float moveSpeed = 5f;

    private bool playerNearby = false;
    private bool isHiding = false;

    private Transform player;
    private FirstPersonRigidbodyController playerMovement;
    private Collider[] playerColliders;
    private Rigidbody playerRigidbody;
    private Camera mainCamera;
    private Coroutine moveRoutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            player = other.transform;
            playerMovement = player.GetComponent<FirstPersonRigidbodyController>();
            playerColliders = player.GetComponentsInChildren<Collider>();
            playerRigidbody = player.GetComponent<Rigidbody>();
            mainCamera = Camera.main;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (isHiding)
                ForceExitHide();
        }
    }

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(hideKey))
        {
            ToggleHide();
        }
    }

    private void ToggleHide()
    {
        if (player == null) return;

        if (!isHiding)
        {
            // Begin hiding
            isHiding = true;

            if (playerMovement != null)
                playerMovement.enabled = false;

            if (playerRigidbody != null)
            {
                playerRigidbody.linearVelocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
                playerRigidbody.isKinematic = true;
            }

            if (playerColliders != null)
            {
                foreach (var c in playerColliders)
                    c.enabled = false;
            }

            if (moveRoutine != null) StopCoroutine(moveRoutine);
            moveRoutine = StartCoroutine(MoveToHide());
        }
        else
        {
            // Exit hiding
            if (moveRoutine != null) StopCoroutine(moveRoutine);
            StartCoroutine(MoveToExit());
        }
    }

    private IEnumerator MoveToHide()
    {
        Vector3 startPlayerPos = player.position;
        Quaternion startPlayerRot = player.rotation;
        Vector3 startCamPos = mainCamera.transform.position;
        Quaternion startCamRot = mainCamera.transform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;

            if (playerHidePosition != null)
            {
                player.position = Vector3.Lerp(startPlayerPos, playerHidePosition.position, t);
                player.rotation = Quaternion.Slerp(startPlayerRot, playerHidePosition.rotation, t);
            }

            if (cameraHidePosition != null)
            {
                mainCamera.transform.position = Vector3.Lerp(startCamPos, cameraHidePosition.position, t);
                mainCamera.transform.rotation = Quaternion.Slerp(startCamRot, cameraHidePosition.rotation, t);
            }

            yield return null;
        }

        moveRoutine = null;
    }

    private IEnumerator MoveToExit()
    {
        isHiding = false;

        Vector3 startCamPos = mainCamera.transform.position;
        Quaternion startCamRot = mainCamera.transform.rotation;

        Vector3 targetCamPos = playerExitPosition != null ? playerExitPosition.position : transform.position + transform.forward * 1.5f;
        Quaternion targetCamRot = playerExitPosition != null ? playerExitPosition.rotation : Quaternion.LookRotation(transform.forward);

        // Move player instantly to exit (so they don't clip inside)
        if (playerExitPosition != null)
        {
            player.position = playerExitPosition.position;
            player.rotation = playerExitPosition.rotation;
        }

        // Reactivate player control
        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerRigidbody != null)
            playerRigidbody.isKinematic = false;

        if (playerColliders != null)
        {
            foreach (var c in playerColliders)
                c.enabled = true;
        }

        // Smoothly move camera back to player exit area
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            mainCamera.transform.position = Vector3.Lerp(startCamPos, targetCamPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startCamRot, targetCamRot, t);
            yield return null;
        }

        moveRoutine = null;
    }

    private void ForceExitHide()
    {
        if (player == null) return;

        isHiding = false;

        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        // Safety reposition
        player.position = playerExitPosition != null ? playerExitPosition.position : transform.position + transform.forward * 1.5f;
        player.rotation = playerExitPosition != null ? playerExitPosition.rotation : Quaternion.LookRotation(transform.forward);

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerRigidbody != null)
            playerRigidbody.isKinematic = false;

        if (playerColliders != null)
        {
            foreach (var c in playerColliders)
                c.enabled = true;
        }
    }
}
