using UnityEngine;

public class ForcedSaveTrigger : MonoBehaviour
{
    public string locationName;
    public string imageKey;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        SaveManager.Instance.CreateForcedSave(locationName, imageKey);
    }
}