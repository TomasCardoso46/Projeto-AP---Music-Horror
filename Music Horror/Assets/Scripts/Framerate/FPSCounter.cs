using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [Header("Display")]
    public bool showFPS = false;
    public KeyCode toggleKey = KeyCode.F10;

    private float deltaTime = 0.0f;

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            showFPS = !showFPS;
        }

        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        if (!showFPS)
            return;

        float fps = 1.0f / deltaTime;

        GUIStyle style = new GUIStyle
        {
            fontSize = 24,
            normal = { textColor = Color.white }
        };

        GUI.Label(
            new Rect(10, 10, 200, 40),
            $"FPS: {Mathf.RoundToInt(fps)}",
            style
        );
    }
}