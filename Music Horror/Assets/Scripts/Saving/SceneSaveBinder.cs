using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneSaveBinder : MonoBehaviour
{
    [System.Serializable]
    public class CheckpointObjectSettings
    {
        [Header("Checkpoint")]
        public string checkpointName;

        [Header("Objects Enabled On Load")]
        public List<GameObject> objectsToEnable = new List<GameObject>();

        [Header("Objects Disabled On Load")]
        public List<GameObject> objectsToDisable = new List<GameObject>();
    }

    [Header("Scene References")]
    [SerializeField] private Transform player;
    [SerializeField] private EnemyController enemy;
    [SerializeField] private Transform drawingsRoot;
    [SerializeField] private MonoBehaviour playerController;

    [Header("Checkpoint Object Settings")]
    [SerializeField] private List<CheckpointObjectSettings> checkpoints =
        new List<CheckpointObjectSettings>();

    private bool initialized;

    private void Start()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError(
                "SceneSaveBinder: No SaveManager instance exists."
            );

            return;
        }

        SaveManager.Instance.BindScene(
            player,
            enemy,
            drawingsRoot,
            playerController,
            this
        );

        StartCoroutine(LoadPending());
    }

    public void Initialize()
    {
        initialized = true;
    }

    private IEnumerator LoadPending()
    {
        // Ensures the scene has finished initializing.
        yield return null;

        string pendingSave = PlayerPrefs.GetString(
            "PendingSaveToLoad",
            ""
        );

        if (!string.IsNullOrEmpty(pendingSave))
        {
            PlayerPrefs.DeleteKey("PendingSaveToLoad");

            SaveManager.Instance.LoadGame(pendingSave);
        }
    }

    public void ApplyCheckpoint(string checkpointName)
    {
        if (string.IsNullOrEmpty(checkpointName))
        {
            Debug.LogWarning(
                "SceneSaveBinder: Attempted to apply an empty checkpoint name."
            );

            return;
        }

        CheckpointObjectSettings checkpoint = GetCheckpoint(checkpointName);

        if (checkpoint == null)
        {
            Debug.LogWarning(
                "SceneSaveBinder: No checkpoint configuration found for '" +
                checkpointName +
                "'."
            );

            return;
        }

        // Enable objects
        foreach (GameObject obj in checkpoint.objectsToEnable)
        {
            if (obj == null)
                continue;

            obj.SetActive(true);
        }

        // Disable objects
        foreach (GameObject obj in checkpoint.objectsToDisable)
        {
            if (obj == null)
                continue;

            obj.SetActive(false);
        }
    }

    private CheckpointObjectSettings GetCheckpoint(string checkpointName)
    {
        foreach (CheckpointObjectSettings checkpoint in checkpoints)
        {
            if (checkpoint == null)
                continue;

            if (checkpoint.checkpointName == checkpointName)
                return checkpoint;
        }

        return null;
    }
}