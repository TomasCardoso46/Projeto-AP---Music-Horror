using System.Collections;
using UnityEngine;
using TMPro;

public class HideSpot : MonoBehaviour
{
    [Header("Positions")]
    [SerializeField] private Transform playerHidePosition;
    [SerializeField] private Transform playerExitPosition;
    [SerializeField] private Transform cameraHidePosition;

    [Header("Settings")]
    [SerializeField] private KeyCode hideKey = KeyCode.F;
    [SerializeField] private float moveSpeed = 5f;

    private bool playerNearby = false;
    private bool isHiding = false;

    private Transform player;
    private FirstPersonRigidbodyController playerMovement;
    private Collider[] playerColliders;
    private Rigidbody playerRigidbody;
    private Camera mainCamera;
    private Coroutine moveRoutine;

    [SerializeField] private TextMeshProUGUI hidePrompt; // found automatically

    private void Start()
    {
        if (hidePrompt == null)
        {
            TextMeshProUGUI[] allTMPs = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            foreach (var tmp in allTMPs)
            {
                if (tmp.name == "ClosetStatus")
                {
                    hidePrompt = tmp;
                    break;
                }
            }
        }
    }

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

            UpdatePrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            if (hidePrompt != null)
                hidePrompt.gameObject.SetActive(false);

            if (isHiding)
                ForceExitHide();
        }
    }

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(hideKey))
        {
            ToggleHide();
            UpdatePrompt();
        }
    }

    private void UpdatePrompt()
    {
        if (hidePrompt == null) return;

        if (!playerNearby)
        {
            hidePrompt.gameObject.SetActive(false);
            return;
        }

        hidePrompt.gameObject.SetActive(true);

        hidePrompt.text = !isHiding ? "Press F to Hide" : "Press F to Leave";
    }

    private void ToggleHide()
    {
        if (player == null) return;

        if (!isHiding)
        {
            isHiding = true;

            if (playerMovement != null)
                playerMovement.enabled = false;

            if (playerRigidbody != null)
            {
                playerRigidbody.linearVelocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
                playerRigidbody.isKinematic = true;
            }

            foreach (var c in playerColliders)
                c.enabled = false;

            if (moveRoutine != null) StopCoroutine(moveRoutine);
            moveRoutine = StartCoroutine(MoveToHide());
        }
        else
        {
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
        UpdatePrompt();
        playerNearby = true;
        hidePrompt.gameObject.SetActive(true);
        
    }

    private IEnumerator MoveToExit()
    {
        isHiding = false;

        Vector3 startCamPos = mainCamera.transform.position;
        Quaternion startCamRot = mainCamera.transform.rotation;

        Vector3 targetCamPos = playerExitPosition != null ? playerExitPosition.position : transform.position + transform.forward * 1.5f;
        Quaternion targetCamRot = playerExitPosition != null ? playerExitPosition.rotation : Quaternion.LookRotation(transform.forward);

        if (playerExitPosition != null)
        {
            player.position = playerExitPosition.position;
            player.rotation = playerExitPosition.rotation;
        }

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerRigidbody != null)
            playerRigidbody.isKinematic = false;

        foreach (var c in playerColliders)
            c.enabled = true;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            mainCamera.transform.position = Vector3.Lerp(startCamPos, targetCamPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startCamRot, targetCamRot, t);
            yield return null;
        }

        moveRoutine = null;
        UpdatePrompt();
        playerNearby = false;
        hidePrompt.gameObject.SetActive(false);
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

        player.position = playerExitPosition != null ? playerExitPosition.position : transform.position + transform.forward * 1.5f;
        player.rotation = playerExitPosition != null ? playerExitPosition.rotation : Quaternion.LookRotation(transform.forward);

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerRigidbody != null)
            playerRigidbody.isKinematic = false;

        foreach (var c in playerColliders)
            c.enabled = true;

        UpdatePrompt();
    }
}
