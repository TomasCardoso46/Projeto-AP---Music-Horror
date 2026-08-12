using UnityEngine;

public class InventoryObjectSwitcher : MonoBehaviour
{
    [Header("Item Requirement")]
    [SerializeField] private string requiredItemID;

    [Header("Held Item Parent")]
    [SerializeField] private Transform itemParent;

    [Header("Objects to Toggle")]
    [SerializeField] private GameObject objectToDisable;
    [SerializeField] private Chord disabledChords;
    [SerializeField] private GameObject objectToEnable;

    [Header("Final State")]
    [SerializeField] private GameObject finalStateObject;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip applySound;
    [SerializeField] private AudioClip revertSound;

    private GameObject spawnedItem;
    private bool hasSwitched = false;

    private void Update()
    {
        if (hasSwitched)
            return;

        if (PlayerInventory.Instance == null)
            return;

        if (PlayerInventory.Instance.HasItem(requiredItemID))
        {
            ApplySwitch();
        }
    }

    private void ApplySwitch()
    {
        if (objectToDisable != null)
            objectToDisable.SetActive(false);

        if (disabledChords != null)
            disabledChords.enabled = false;

        if (objectToEnable != null)
            objectToEnable.SetActive(true);

        // Spawn the correct prefab based on the ID.
        if (itemParent != null)
        {
            spawnedItem = PlayerInventory.Instance.SpawnItem(
                requiredItemID,
                itemParent
            );
        }

        PlaySound(applySound);

        hasSwitched = true;
    }

    public void RevertAndConsumeItem()
    {
        if (!hasSwitched)
            return;

        if (objectToDisable != null)
            objectToDisable.SetActive(true);

        if (objectToEnable != null)
            objectToEnable.SetActive(false);

        if (disabledChords != null)
            disabledChords.enabled = true;

        if (finalStateObject != null)
            finalStateObject.SetActive(true);

        // Destroy the currently displayed inventory object.
        if (spawnedItem != null)
        {
            Destroy(spawnedItem);
            spawnedItem = null;
        }

        // Consume the item.
        PlayerInventory.Instance.RemoveItem(requiredItemID);

        PlaySound(revertSound);

        hasSwitched = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}