using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SaveFolder => Application.persistentDataPath + "/saves/";

    public static void Init()
    {
        if (!Directory.Exists(SaveFolder))
            Directory.CreateDirectory(SaveFolder);
    }

    public static void SaveToFile(string fileName, SaveData data)
    {
        Init();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(fileName), json);
    }

    public static SaveData LoadFromFile(string fileName)
    {
        string path = GetPath(fileName);

        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void DeleteSave(string fileName)
    {
        string path = GetPath(fileName);

        if (File.Exists(path))
            File.Delete(path);
    }

    public static List<SaveEntry> GetAllSaves()
    {
        Init();

        List<SaveEntry> saves = new List<SaveEntry>();

        string[] files = Directory.GetFiles(SaveFolder, "*.json");

        foreach (string file in files)
        {
            try
            {
                string json = File.ReadAllText(file);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                if (data == null)
                    continue;

                saves.Add(new SaveEntry
                {
                    fileName = Path.GetFileNameWithoutExtension(file),
                    data = data
                });
            }
            catch
            {
                continue;
            }
        }

        return saves;
    }

    public static SaveEntry GetLatestSave()
    {
        var saves = GetAllSaves();

        SaveEntry latest = null;
        DateTime latestTime = DateTime.MinValue;

        foreach (var save in saves)
        {
            if (DateTime.TryParse(save.data.dateTime, out DateTime t))
            {
                if (t > latestTime)
                {
                    latestTime = t;
                    latest = save;
                }
            }
        }

        return latest;
    }

    private static string GetPath(string fileName)
    {
        return SaveFolder + fileName + ".json";
    }
}

[Serializable]
public class SaveEntry
{
    public string fileName;
    public SaveData data;
}