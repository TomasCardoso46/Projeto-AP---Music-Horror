using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private Transform player;
    private EnemyController enemy;
    private Transform drawingsRoot;

    private MonoBehaviour playerController;

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

    public void BindScene(
        Transform playerT,
        EnemyController enemyController,
        Transform drawings,
        MonoBehaviour playerCtrl)
    {
        player = playerT;
        enemy = enemyController;
        drawingsRoot = drawings;
        playerController = playerCtrl;
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

            enemy = BuildEnemyData(),

            unlockedSpells = GetUnlockedSpells()
        };
    }

    private EnemyData BuildEnemyData()
    {
        return new EnemyData
        {
            position = ToArray(enemy.transform.position),
            rotation = ToArray(enemy.transform.eulerAngles),

            state = (int)enemy.currentState,

            lastKnownPosition = ToArray(enemy.transform.position),

            timeSinceSeen = 0f,
            investigateTimer = 0f
        };
    }

    public void CreateManualSave(string locationName, string imageKey)
    {
        if (manualSaves.Count >= manualSaveLimit)
        {
            SaveSystem.DeleteSave(manualSaves[0]);
            manualSaves.RemoveAt(0);
        }

        string id = "manual_" + Guid.NewGuid();
        SaveSystem.SaveToFile(id, BuildSave("Manual", locationName, imageKey));
        manualSaves.Add(id);
    }

    public void CreateAutoSave(string locationName, string imageKey)
    {
        SaveSystem.SaveToFile("auto", BuildSave("Auto", locationName, imageKey));
    }

    public void CreateForcedSave(string locationName, string imageKey)
    {
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
        Time.timeScale = 0f;

        DisableGameplaySystems();

        yield return null;

        RestorePlayer(data.player);
        RestoreEnemy(data.enemy);
        RestoreSpells(data.unlockedSpells);

        yield return null;

        EnableGameplaySystems();

        Time.timeScale = 1f;
    }

    private void RestorePlayer(PlayerData data)
    {
        player.position = ToVector3(data.position);
        player.eulerAngles = ToVector3(data.rotation);
    }

    private void RestoreEnemy(EnemyData data)
    {
        enemy.transform.position = ToVector3(data.position);
        enemy.transform.eulerAngles = ToVector3(data.rotation);

        enemy.currentState = (EnemyController.State)data.state;

        Vector3 last = ToVector3(data.lastKnownPosition);
        enemy.SendMessage("SetLastKnownPosition", last, SendMessageOptions.DontRequireReceiver);

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

    private float[] ToArray(Vector3 v) => new float[] { v.x, v.y, v.z };

    private Vector3 ToVector3(float[] v) => new Vector3(v[0], v[1], v[2]);

    private List<string> GetUnlockedSpells()
    {
        List<string> result = new List<string>();

        foreach (Transform child in drawingsRoot)
            if (child.gameObject.activeSelf)
                result.Add(child.name);

        return result;
    }
    public bool IsEnemyInvestigating()
    {
        if (enemy == null) return false;
        return enemy.currentState == EnemyController.State.Investigate;
    }
}