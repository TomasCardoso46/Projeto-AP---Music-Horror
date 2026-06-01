using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string saveName;
    public string saveType;

    public string locationName;
    public string locationImageKey;

    public string dateTime;

    public PlayerData player;
    public EnemyData enemy;

    public List<string> unlockedSpells = new List<string>();
}

[Serializable]
public class PlayerData
{
    public float[] position;
    public float[] rotation;
}

[Serializable]
public class EnemyData
{
    public float[] position;
    public float[] rotation;

    public int state;

    public float[] lastKnownPosition;
    public float timeSinceSeen;
    public float investigateTimer;
}