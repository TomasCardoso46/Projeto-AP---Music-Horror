using System.Collections.Generic;
using UnityEngine;

public class SaveIconDatabase : MonoBehaviour
{
    public static SaveIconDatabase Instance;

    [System.Serializable]
    public class Entry
    {
        public string key;
        public Sprite sprite;
    }

    public List<Entry> entries;

    private Dictionary<string, Sprite> dict;

    private void Awake()
    {
        Instance = this;
        dict = new Dictionary<string, Sprite>();

        foreach (var e in entries)
            dict[e.key] = e.sprite;
    }

    public Sprite Get(string key)
    {
        if (dict.TryGetValue(key, out Sprite s))
            return s;

        return null;
    }
}