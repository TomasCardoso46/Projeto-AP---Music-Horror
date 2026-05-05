using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

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

    public void AddItem(string itemID)
    {
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
        return items.Contains(itemID);
    }
}