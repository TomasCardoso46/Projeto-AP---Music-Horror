using UnityEngine;

public class TriggerToggle : MonoBehaviour
{
    public enum ActionType
    {
        Activate,
        Deactivate
    }

    [Header("Settings")]
    [SerializeField] private ActionType action = ActionType.Activate;
    [SerializeField] private GameObject targetObject;

    [Header("Safety")]
    [SerializeField] private string requiredTag = "Player";
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered) return;

        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
            return;

        if (targetObject == null)
        {
            Debug.LogWarning("TriggerToggle: No target object assigned.", this);
            return;
        }

        switch (action)
        {
            case ActionType.Activate:
                targetObject.SetActive(true);
                break;

            case ActionType.Deactivate:
                targetObject.SetActive(false);
                break;
        }

        hasTriggered = true;
    }
}