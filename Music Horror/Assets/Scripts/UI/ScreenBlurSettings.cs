using UnityEngine;
public static class ScreenBlurSettings
{
    public static bool Enabled { get; private set; }

    public static float Strength { get; private set; }

    public static int Iterations { get; private set; }

    public static void Set(
        bool enabled,
        float strength,
        int iterations
    )
    {
        Enabled = enabled;

        Strength = Mathf.Max(
            0f,
            strength
        );

        Iterations = Mathf.Clamp(
            iterations,
            1,
            8
        );
    }
}