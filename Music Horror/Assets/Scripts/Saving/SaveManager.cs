using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private Transform player;
    private EnemyController enemy;
    private Transform drawingsRoot;
    private MonoBehaviour playerController;

    private string lastCheckpointName = "Unknown Area";
    private string lastCheckpointImage = "default";

    public bool IsLoading { get; private set; }

    public int manualSaveLimit = 5;
    private List<string> manualSaves = new List<string>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SaveSystem.Init();
    }

    public void BindScene(Transform playerT, EnemyController enemyController, Transform drawings, MonoBehaviour playerCtrl)
    {
        player = playerT;
        enemy = enemyController;
        drawingsRoot = drawings;
        playerController = playerCtrl;
    }

    public void CreateManualSave()
    {
        if (manualSaves.Count >= manualSaveLimit)
        {
            SaveSystem.DeleteSave(manualSaves[0]);
            manualSaves.RemoveAt(0);
        }

        string id = "manual_" + Guid.NewGuid();
        SaveSystem.SaveToFile(id, BuildSave("Manual", lastCheckpointName, lastCheckpointImage));
        manualSaves.Add(id);
    }

    public void CreateAutoSave()
    {
        SaveSystem.SaveToFile("auto", BuildSave("Auto", lastCheckpointName, lastCheckpointImage));
    }

    public void CreateForcedSave(string locationName, string imageKey)
    {
        lastCheckpointName = locationName;
        lastCheckpointImage = imageKey;

        string id = "forced_" + locationName + "_" + Guid.NewGuid();
        SaveSystem.SaveToFile(id, BuildSave("Forced", locationName, imageKey));
    }

    public void LoadGame(string fileName)
    {
        SaveData data = SaveSystem.LoadFromFile(fileName);
        if (data == null) return;

        StartCoroutine(LoadRoutine(data));
    }

    private IEnumerator LoadRoutine(SaveData data)
    {
        IsLoading = true;

        var controller = playerController as FirstPersonRigidbodyController;
        if (controller != null)
            controller.isLoading = true;

        Time.timeScale = 0f;

        DisableGameplaySystems();

        yield return null;
        yield return new WaitForEndOfFrame();

        // ✅ CRITICAL: wait for deterministic player restore BEFORE anything else
        yield return ApplyPlayerTransform(data.player);

        RestoreEnemy(data.enemy);
        RestoreSpells(data.unlockedSpells);

        yield return new WaitForEndOfFrame();

        EnableGameplaySystems();

        Time.timeScale = 1f;

        if (controller != null)
        {
            controller.ResetAfterLoad();
            controller.isLoading = false;
        }

        IsLoading = false;
    }

    private IEnumerator ApplyPlayerTransform(PlayerData data)
    {
        yield return new WaitForEndOfFrame();

        Vector3 pos = ToVector3(data.position);
        Vector3 rot = ToVector3(data.rotation);

        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // ✅ fully freeze physics so NOTHING can override position
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.position = pos;
            rb.rotation = Quaternion.Euler(rot);

            Physics.SyncTransforms();

            yield return null;

            rb.isKinematic = false;
        }
        else
        {
            player.position = pos;
            player.rotation = Quaternion.Euler(rot);
        }
    }

    private void RestoreEnemy(EnemyData data)
    {
        enemy.transform.position = ToVector3(data.position);
        enemy.transform.eulerAngles = ToVector3(data.rotation);
        enemy.currentState = (EnemyController.State)data.state;
        enemy.ResetStateAfterLoad();
    }

    private void RestoreSpells(List<string> spells)
    {
        foreach (Transform child in drawingsRoot)
            child.gameObject.SetActive(spells.Contains(child.name));
    }

    private void DisableGameplaySystems()
    {
        if (playerController != null)
            playerController.enabled = false;

        if (enemy != null)
            enemy.enabled = false;

        Physics.autoSimulation = false;
    }

    private void EnableGameplaySystems()
    {
        if (playerController != null)
            playerController.enabled = true;

        if (enemy != null)
            enemy.enabled = true;

        Physics.autoSimulation = true;
    }

    private SaveData BuildSave(string type, string locationName, string imageKey)
    {
        return new SaveData
        {
            saveType = type,
            locationName = locationName,
            locationImageKey = imageKey,
            dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),

            player = new PlayerData
            {
                position = ToArray(player.position),
                rotation = ToArray(player.eulerAngles)
            },

            enemy = new EnemyData
            {
                position = ToArray(enemy.transform.position),
                rotation = ToArray(enemy.transform.eulerAngles),
                state = (int)enemy.currentState
            },

            unlockedSpells = GetUnlockedSpells()
        };
    }

    private float[] ToArray(Vector3 v)
    {
        return new float[]
        {
            float.IsNaN(v.x) ? 0 : v.x,
            float.IsNaN(v.y) ? 0 : v.y,
            float.IsNaN(v.z) ? 0 : v.z
        };
    }

    private Vector3 ToVector3(float[] v)
    {
        return new Vector3(v[0], v[1], v[2]);
    }

    private List<string> GetUnlockedSpells()
    {
        List<string> result = new List<string>();

        foreach (Transform child in drawingsRoot)
            if (child.gameObject.activeSelf)
                result.Add(child.name);

        return result;
    }

    public string GetLastCheckpointName() => lastCheckpointName;
    public string GetLastCheckpointImage() => lastCheckpointImage;
}