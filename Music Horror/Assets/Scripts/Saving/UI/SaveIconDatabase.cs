using System.Collections.Generic;
using UnityEngine;

public class SaveIconDatabase : MonoBehaviour
{
    public static SaveIconDatabase Instance;

    [System.Serializable]
    public class IconEntry
    {
        public string key;
        public Sprite sprite;
    }

    [Header("Registered Icons")]
    [SerializeField] private List<IconEntry> icons = new List<IconEntry>();

    private Dictionary<string, Sprite> iconMap = new Dictionary<string, Sprite>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildDictionary();
    }

    private void BuildDictionary()
    {
        iconMap.Clear();

        foreach (var entry in icons)
        {
            if (entry == null || string.IsNullOrEmpty(entry.key))
                continue;

            if (iconMap.ContainsKey(entry.key))
            {
                Debug.LogWarning($"Duplicate icon key detected: {entry.key}");
                continue;
            }

            iconMap.Add(entry.key, entry.sprite);
        }

        Debug.Log($"SaveIconDatabase loaded {iconMap.Count} icons.");
    }

    public Sprite Get(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("SaveIconDatabase.Get called with null/empty key");
            return null;
        }

        if (iconMap.TryGetValue(key, out Sprite sprite))
            return sprite;

        Debug.LogWarning($"SaveIconDatabase missing icon for key: {key}");
        return null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Helps catch missing references early in editor
        if (icons == null) return;

        foreach (var entry in icons)
        {
            if (entry != null && entry.sprite == null)
            {
                Debug.LogWarning($"Icon entry '{entry.key}' has no sprite assigned");
            }
        }
    }
#endif
}