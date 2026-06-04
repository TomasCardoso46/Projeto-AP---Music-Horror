using UnityEngine;

public class LookPrompt : MonoBehaviour
{
    [Header("Prompt")]
    [TextArea(3, 5)]
    public string promptText;

    [Header("Detection")]
    public float maxDistance = 5f;

    [Header("Display")]
    public float displayDuration = 3f;

    [Tooltip("-1 = Unlimited")]
    public int maxAppearances = 1;

    private int appearanceCount = 0;

    public bool CanAppear()
    {
        return maxAppearances < 0 || appearanceCount < maxAppearances;
    }

    public void RegisterAppearance()
    {
        appearanceCount++;
    }
}