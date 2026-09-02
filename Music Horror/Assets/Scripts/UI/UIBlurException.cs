using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UIBlurException : MonoBehaviour
{
    [Header("Blur")]
    [SerializeField]
    [Min(0f)]
    private float blurStrength = 3f;

    [SerializeField]
    [Range(1, 8)]
    private int blurIterations = 2;

    [Header("Startup")]
    [SerializeField]
    private bool enableOnStart = true;

    private Canvas originalCanvas;
    private Canvas sharpCanvas;

    private Transform originalParent;

    private void Awake()
    {
        originalCanvas = GetComponentInParent<Canvas>();

        if (originalCanvas == null)
        {
            Debug.LogError(
                "UIBlurException must be attached to a UI object " +
                "that is inside a Canvas.",
                this
            );

            enabled = false;
            return;
        }

        originalParent = transform.parent;
    }

    public void EnableBlur()
    {
        if (sharpCanvas != null)
            return;

        CreateSharpCanvas();

        MoveObjectToSharpCanvas();

        ScreenBlurSettings.Set(
            true,
            blurStrength,
            blurIterations
        );
    }

    public void DisableBlur()
    {
        ScreenBlurSettings.Set(
            false,
            0f,
            0
        );

        // Move the UI object back to its original Canvas.
        if (originalParent != null)
        {
            transform.SetParent(
                originalParent,
                false
            );
        }

        // Destroy the special sharp canvas.
        if (sharpCanvas != null)
        {
            Destroy(sharpCanvas.gameObject);
            sharpCanvas = null;
        }
    }

    private void CreateSharpCanvas()
    {
        GameObject canvasObject = new GameObject(
            "Sharp UI Canvas",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        sharpCanvas = canvasObject.GetComponent<Canvas>();

        sharpCanvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        // Render this Canvas after everything else.
        sharpCanvas.sortingOrder = 32767;

        CopyCanvasScaler();
    }

    private void CopyCanvasScaler()
    {
        CanvasScaler originalScaler =
            originalCanvas.GetComponent<CanvasScaler>();

        if (originalScaler == null)
            return;

        CanvasScaler newScaler =
            sharpCanvas.GetComponent<CanvasScaler>();

        newScaler.uiScaleMode =
            originalScaler.uiScaleMode;

        newScaler.referenceResolution =
            originalScaler.referenceResolution;

        newScaler.screenMatchMode =
            originalScaler.screenMatchMode;

        newScaler.matchWidthOrHeight =
            originalScaler.matchWidthOrHeight;

        newScaler.referencePixelsPerUnit =
            originalScaler.referencePixelsPerUnit;
    }

    private void MoveObjectToSharpCanvas()
    {
        transform.SetParent(
            sharpCanvas.transform,
            false
        );
    }

    public void SetBlurStrength(float strength)
    {
        blurStrength = Mathf.Max(0f, strength);

        if (ScreenBlurSettings.Enabled)
        {
            ScreenBlurSettings.Set(
                true,
                blurStrength,
                blurIterations
            );
        }
    }

    public void SetBlurIterations(int iterations)
    {
        blurIterations = Mathf.Clamp(
            iterations,
            1,
            8
        );

        if (ScreenBlurSettings.Enabled)
        {
            ScreenBlurSettings.Set(
                true,
                blurStrength,
                blurIterations
            );
        }
    }

    private void OnDestroy()
    {
        ScreenBlurSettings.Set(
            false,
            0f,
            0
        );

        if (sharpCanvas != null)
        {
            Destroy(sharpCanvas.gameObject);
        }
    }
}