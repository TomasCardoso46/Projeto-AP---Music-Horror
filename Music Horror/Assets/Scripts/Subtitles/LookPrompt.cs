using UnityEngine;

public class LookPrompt : MonoBehaviour
{
    [Header("Prompt")]
    [TextArea(3, 5)]
    public string promptText;

    [Header("Voice")]
    public AudioClip voiceClip;

    [Header("Detection")]
    public float maxDistance = 5f;

    [Header("Display")]
    public float displayDuration = 3f;

    [Tooltip("-1 = Unlimited")]
    public int maxAppearances = 1;

    [Header("Cooldown")]
    [Tooltip("Seconds before this prompt can appear again.")]
    public float reappearCooldown = 10f;

    private int appearanceCount = 0;
    private float nextAllowedAppearanceTime = 0f;

    public bool CanAppear()
    {
        bool hasAppearancesLeft =
            maxAppearances < 0 || appearanceCount < maxAppearances;

        bool cooldownFinished =
            Time.time >= nextAllowedAppearanceTime;

        return hasAppearancesLeft && cooldownFinished;
    }

    public void RegisterAppearance()
    {
        appearanceCount++;
        nextAllowedAppearanceTime = Time.time + reappearCooldown;
    }
}