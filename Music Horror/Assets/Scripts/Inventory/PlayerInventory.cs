using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    [Serializable]
    public class InventoryPrefab
    {
        public string itemID;
        public GameObject prefab;
    }

    [Header("Item Prefabs")]
    [SerializeField] private List<InventoryPrefab> itemPrefabs = new List<InventoryPrefab>();

    private HashSet<string> items = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ---------------------------------------------------------
    // INVENTORY
    // ---------------------------------------------------------

    public void AddItem(string itemID)
    {
        if (string.IsNullOrEmpty(itemID))
            return;

        items.Add(itemID);

        Debug.Log("Picked up: " + itemID);
    }

    public void RemoveItem(string itemID)
    {
        if (items.Remove(itemID))
        {
            Debug.Log("Removed: " + itemID);
        }
    }

    public bool HasItem(string itemID)
    {
        return !string.IsNullOrEmpty(itemID) && items.Contains(itemID);
    }

    // ---------------------------------------------------------
    // PREFAB LOOKUP
    // ---------------------------------------------------------

    public GameObject GetPrefab(string itemID)
    {
        foreach (InventoryPrefab item in itemPrefabs)
        {
            if (item.itemID == itemID)
            {
                return item.prefab;
            }
        }

        Debug.LogWarning("No prefab registered for item ID: " + itemID);
        return null;
    }

    // ---------------------------------------------------------
    // SPAWN ITEM
    // ---------------------------------------------------------

    public GameObject SpawnItem(string itemID, Transform parent)
    {
        GameObject prefab = GetPrefab(itemID);

        if (prefab == null)
            return null;

        GameObject spawnedObject = Instantiate(prefab, parent);

        spawnedObject.transform.localPosition = Vector3.zero;
        spawnedObject.transform.localRotation = Quaternion.identity;
        spawnedObject.transform.localScale = prefab.transform.localScale;

        return spawnedObject;
    }
}