using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cheats : MonoBehaviour
{
    public static bool EnemyDisabled;

    [SerializeField] private GameObject objectToDelete;

    [Header("Teleport Positions")]
    [SerializeField] private List<TeleportBinding> teleports = new List<TeleportBinding>();

    [System.Serializable]
    public class TeleportBinding
    {
        public KeyCode key;
        public Transform destination;

        [Header("Optional On Teleport")]
        public GameObject objectToEnable;
    }

    private FirstPersonRigidbodyController playerController;
    private Rigidbody playerRigidbody;

    void Start()
    {
        playerController = FindObjectOfType<FirstPersonRigidbodyController>();

        if (playerController != null)
        {
            playerRigidbody = playerController.GetComponent<Rigidbody>();
        }
        else
        {
            Debug.LogWarning("Cheats: No FirstPersonRigidbodyController found in scene.");
        }
    }

    void Update()
    {
        HandleEnemyDisableToggle();
        HandleObjectDelete();
        HandleTeleports();
        HandleRestart();
    }

    void HandleEnemyDisableToggle()
    {
        if (Input.inputString.Contains("ç") || Input.inputString.Contains("Ç"))
        {
            EnemyDisabled = !EnemyDisabled;
            Debug.Log("EnemyDisabled = " + EnemyDisabled);
        }
    }

    void HandleObjectDelete()
    {
        if (Input.inputString.Contains("º") || Input.inputString.Contains("Ç"))
        {
            if (objectToDelete != null)
                Destroy(objectToDelete);
        }
    }

    void HandleTeleports()
    {
        if (playerController == null || playerRigidbody == null)
            return;

        foreach (var t in teleports)
        {
            if (t.destination == null) continue;

            if (Input.GetKeyDown(t.key))
            {
                StartCoroutine(TeleportRoutine(t));
            }
        }
    }

    IEnumerator TeleportRoutine(TeleportBinding t)
    {
        playerController.enabled = false;

        playerRigidbody.linearVelocity = Vector3.zero;
        playerRigidbody.angularVelocity = Vector3.zero;

        playerRigidbody.position = t.destination.position;

        Physics.SyncTransforms();

        yield return new WaitForFixedUpdate();

        playerController.enabled = true;

        if (t.objectToEnable != null)
        {
            if (!t.objectToEnable.activeSelf)
            {
                t.objectToEnable.SetActive(true);
            }
        }
    }

    void HandleRestart()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}