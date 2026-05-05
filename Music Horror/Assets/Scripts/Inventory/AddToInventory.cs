using UnityEngine;

public class AddToInventory : CanPickUp
{
    public override void OnPickUp()
    {
        PlayerInventory.Instance.AddItem(itemID);
        Destroy(gameObject);
    }
}