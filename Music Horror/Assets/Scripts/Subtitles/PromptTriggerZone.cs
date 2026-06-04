using UnityEngine;

public class ZonePromptTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LookPromptManager promptManager;

    [Header("Prompt")]
    [SerializeField] private ZonePrompt zonePrompt;

    [Header("Options")]
    [SerializeField] private bool triggerOnce = false;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (zonePrompt == null || promptManager == null)
            return;

        if (!zonePrompt.CanAppear())
            return;

        zonePrompt.RegisterAppearance();
        promptManager.TriggerZonePrompt(zonePrompt);

        if (triggerOnce)
            hasTriggered = true;
    }
}