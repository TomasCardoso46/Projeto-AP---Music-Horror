using UnityEngine;

public abstract class CanPickUp : MonoBehaviour, IInteractable
{
    public string itemID;

    public abstract void OnPickUp();

    public void Interact()
    {
        OnPickUp();
    }
}