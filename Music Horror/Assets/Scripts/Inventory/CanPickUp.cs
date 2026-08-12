using UnityEngine;

public class CanPickUp : MonoBehaviour, IInteractable
{
    [Header("Item")]
    [SerializeField] private string itemID;

    [Header("Pickup Behaviour")]
    [SerializeField] private bool canTake = true;

    [Header("Return Position")]
    [Tooltip("Where the original world object should return when the held item is released.")]
    [SerializeField] private Transform returnPosition;

    [Header("Interaction")]
    [SerializeField] private Transform heldItemParent;

    public string ItemID => itemID;
    public bool CanTake => canTake;
    public Transform ReturnPosition => returnPosition;

    public void Interact()
    {
        PickUp(heldItemParent);
    }

    public void PickUp(Transform heldItemParent)
    {
        if (string.IsNullOrEmpty(itemID))
        {
            Debug.LogWarning(gameObject.name + " has no Item ID.");
            return;
        }

        if (heldItemParent == null)
        {
            Debug.LogWarning("No held item parent assigned.");
            return;
        }

        // Spawn the inventory version of this object.
        GameObject heldObject =
            PlayerInventory.Instance.SpawnItem(itemID, heldItemParent);

        if (heldObject == null)
        {
            Debug.LogWarning(
                "Could not spawn item with ID: " + itemID
            );

            return;
        }

        // -----------------------------------------------------
        // CAN TAKE
        // -----------------------------------------------------

        if (canTake)
        {
            PlayerInventory.Instance.AddItem(itemID);

            gameObject.SetActive(false);

            HeldItemInteraction interaction =
                heldObject.GetComponent<HeldItemInteraction>();

            if (interaction == null)
                interaction = heldObject.AddComponent<HeldItemInteraction>();

            interaction.Setup(
                this,
                heldObject,
                true,
                returnPosition
            );
        }

        // -----------------------------------------------------
        // CANNOT TAKE
        // -----------------------------------------------------

        else
        {
            // Set the X rotation of all PlayerInteract objects to 0.
            PlayerInteract[] playerInteracts =
                FindObjectsByType<PlayerInteract>(FindObjectsSortMode.None);

            foreach (PlayerInteract playerInteract in playerInteracts)
            {
                Vector3 rotation = playerInteract.transform.eulerAngles;
                rotation.x = 0f;
                playerInteract.transform.eulerAngles = rotation;
            }

            gameObject.SetActive(false);

            HeldItemInteraction interaction =
                heldObject.GetComponent<HeldItemInteraction>();

            if (interaction == null)
                interaction = heldObject.AddComponent<HeldItemInteraction>();

            interaction.Setup(
                this,
                heldObject,
                false,
                null
            );

            interaction.DisablePlayer();
        }
    }
}