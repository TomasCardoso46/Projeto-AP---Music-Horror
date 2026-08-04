using UnityEngine;

public class GuitarAimController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FirstPersonRigidbodyController playerController;
    [SerializeField] private Chord chordPlayer;
    [SerializeField] private ChordSequenceManager sequenceManager;

    [Header("UI")]
    [SerializeField] private Canvas borderCanvas;

    [SerializeField] private RectTransform leftBorder;
    [SerializeField] private RectTransform topBorder;
    [SerializeField] private RectTransform rightBorder;
    [SerializeField] private RectTransform bottomBorder;

    [Header("Cursor")]
    [SerializeField] private Texture2D guitarCursor;

    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;

    [Header("Behaviour")]
    [SerializeField]
    private bool disableMovement = false;

    private bool guitarModeActive;

    private bool cursorMovedSinceOpening;

    private Border currentBorder = Border.None;

    [SerializeField]
    private float minimumSwipeDistance = 20f;

    private Vector2 previousMousePosition;

    private enum Border
    {
        None,
        Left,
        Up,
        Right,
        Down
    }

    void Awake()
    {
        if (borderCanvas != null)
            borderCanvas.enabled = false;
    }

    void Update()
    {
        if (GameState.IsPaused)
            return;

        HandleModeToggle();

        if (!guitarModeActive)
            return;

        DetectBorders();
    }

    private void HandleModeToggle()
    {
        if (Input.GetMouseButtonDown(0))
        {
            EnterGuitarMode();
        }

        if (Input.GetMouseButtonUp(0))
        {
            ExitGuitarMode();
        }
    }

    private void EnterGuitarMode()
    {
        previousMousePosition = Input.mousePosition;
        if (guitarModeActive)
            return;

        guitarModeActive = true;

        currentBorder = Border.None;

        cursorMovedSinceOpening = false;

        if (playerController != null)
        {
            playerController.DisableCameraLook();
            playerController.SetCursorLocked(false);
        }

        if (borderCanvas != null)
        {
            borderCanvas.enabled = true;
        }

        if (guitarCursor != null)
        {
            Cursor.SetCursor(
                guitarCursor,
                cursorHotspot,
                CursorMode.Auto);
        }
    }

    private void ExitGuitarMode()
    {
        if (!guitarModeActive)
            return;

        guitarModeActive = false;

        currentBorder = Border.None;

        if (sequenceManager != null)
        {
            sequenceManager.ResetSequence();
        }

        if (playerController != null)
        {
            playerController.EnableCameraLook();
            playerController.SetCursorLocked(true);
        }

        if (borderCanvas != null)
        {
            borderCanvas.enabled = false;
        }

        Cursor.SetCursor(
            null,
            Vector2.zero,
            CursorMode.Auto);
    }

    public bool IsInGuitarMode()
    {
        return guitarModeActive;
    }
    private void DetectBorders()
    {
        Vector2 mousePosition = Input.mousePosition;

        if (!cursorMovedSinceOpening)
        {
            if (Vector2.Distance(mousePosition, previousMousePosition) > 2f)
            {
                cursorMovedSinceOpening = true;
            }
            else
            {
                previousMousePosition = mousePosition;
                return;
            }
        }

        Border detectedBorder = GetCurrentBorder();

        if (detectedBorder == Border.None)
        {
            currentBorder = Border.None;
            previousMousePosition = mousePosition;
            return;
        }

        if (detectedBorder == currentBorder)
        {
            previousMousePosition = mousePosition;
            return;
        }

        Vector2 delta = mousePosition - previousMousePosition;

        if (delta.magnitude < minimumSwipeDistance)
        {
            previousMousePosition = mousePosition;
            return;
        }

        if (IsCorrectSwipeDirection(detectedBorder, delta))
        {
            currentBorder = detectedBorder;
            PlayBorderChord(detectedBorder);
        }

        previousMousePosition = mousePosition;
    }

    private Border GetCurrentBorder()
    {
        Vector2 mousePosition = Input.mousePosition;

        if (leftBorder != null &&
            RectTransformUtility.RectangleContainsScreenPoint(
                leftBorder,
                mousePosition))
        {
            return Border.Left;
        }

        if (topBorder != null &&
            RectTransformUtility.RectangleContainsScreenPoint(
                topBorder,
                mousePosition))
        {
            return Border.Up;
        }

        if (rightBorder != null &&
            RectTransformUtility.RectangleContainsScreenPoint(
                rightBorder,
                mousePosition))
        {
            return Border.Right;
        }

        if (bottomBorder != null &&
            RectTransformUtility.RectangleContainsScreenPoint(
                bottomBorder,
                mousePosition))
        {
            return Border.Down;
        }

        return Border.None;
    }

    private void PlayBorderChord(Border border)
    {
        if (chordPlayer == null)
            return;

        switch (border)
        {
            case Border.Left:
                chordPlayer.PlayChord(1);
                break;

            case Border.Up:
                chordPlayer.PlayChord(2);
                break;

            case Border.Right:
                chordPlayer.PlayChord(3);
                break;

            case Border.Down:
                chordPlayer.PlayChord(4);
                break;
        }
    }
    private bool IsCorrectSwipeDirection(Border border, Vector2 delta)
    {
        switch (border)
        {
            case Border.Left:
                return delta.x < 0f &&
                    Mathf.Abs(delta.x) > Mathf.Abs(delta.y);

            case Border.Right:
                return delta.x > 0f &&
                    Mathf.Abs(delta.x) > Mathf.Abs(delta.y);

            case Border.Up:
                return delta.y > 0f &&
                    Mathf.Abs(delta.y) > Mathf.Abs(delta.x);

            case Border.Down:
                return delta.y < 0f &&
                    Mathf.Abs(delta.y) > Mathf.Abs(delta.x);
        }

        return false;
    }
}