using UnityEngine;
using UnityEngine.SceneManagement;

public class Cheats : MonoBehaviour
{
    [SerializeField] private GameObject objectToDelete;
    [Header("Teleport Positions")]
    [SerializeField] private Transform teleportPositionL;
    [SerializeField] private Transform teleportPositionK;

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
            Debug.LogWarning("Cheats: No FirstPersonRigidbodyController found in the scene.");
        }
    }

    void Update()
    {
        if (Input.inputString.Contains("ç") || Input.inputString.Contains("Ç"))
        {
            if (objectToDelete != null)
            {
                Destroy(objectToDelete);
            }
        }
        if (playerController == null)
            return;

        if (Input.GetKeyDown(KeyCode.L))
        {
            TeleportPlayer(teleportPositionL.position);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            TeleportPlayer(teleportPositionK.position);
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            RestartScene();
        }
    }

    void TeleportPlayer(Vector3 targetPosition)
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        playerController.transform.position = targetPosition;
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
