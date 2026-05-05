using UnityEngine;

public class PlacePainting : CanPickUp
{
    public InventoryObjectSwitcher switcher;
    public override void OnPickUp()
    {
        switcher.RevertAndConsumeItem();
    }
}