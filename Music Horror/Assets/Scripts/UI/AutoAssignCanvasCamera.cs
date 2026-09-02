using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class AutoAssignCanvasCamera : MonoBehaviour
{
    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();

        if (canvas.renderMode != RenderMode.ScreenSpaceCamera)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
        }

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError(
                "AutoAssignCanvasCamera: Could not find a camera tagged MainCamera.",
                this
            );

            return;
        }

        canvas.worldCamera = mainCamera;
    }
}