using System.Collections.Generic;
using UnityEngine;

public class StatueInteraction : MonoBehaviour, IInteractable
{
    [Header("GameObjects to Enable")]
    [SerializeField] private List<GameObject> objectsToEnable = new();

    [Header("GameObjects to Disable")]
    [SerializeField] private List<GameObject> objectsToDisable = new();

    [Header("Scripts to Enable")]
    [SerializeField] private List<MonoBehaviour> scriptsToEnable = new();

    public void Interact()
    {
        // Enable GameObjects
        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        // Disable GameObjects
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // Enable scripts
        foreach (MonoBehaviour script in scriptsToEnable)
        {
            if (script != null)
                script.enabled = true;
        }
    }
}
