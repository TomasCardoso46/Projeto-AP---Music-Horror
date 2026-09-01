using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GamepadMenuCursor : MonoBehaviour
{
    [Header("Cursor Settings")]
    [SerializeField] private float cursorSpeed = 1000f;
    [SerializeField] private float cursorAcceleration = 1f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Input Settings")]
    [SerializeField] private float stickDeadzone = 0.15f;

    private Vector2 cursorPosition;

    private void Start()
    {
        cursorPosition = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : new Vector2(Screen.width / 2f, Screen.height / 2f);

        SetCursorPosition(cursorPosition);
    }

    private void Update()
    {
        if (Gamepad.current == null)
            return;

        MoveCursor();
        HandleClick();
    }

    private void MoveCursor()
    {
        Vector2 stickInput = Gamepad.current.leftStick.ReadValue();

        if (stickInput.magnitude < stickDeadzone)
            return;

        float magnitude = Mathf.InverseLerp(
            stickDeadzone,
            1f,
            stickInput.magnitude
        );

        stickInput = stickInput.normalized * magnitude;

        float deltaTime = useUnscaledTime
            ? Time.unscaledDeltaTime
            : Time.deltaTime;

        Vector2 movement =
            stickInput *
            cursorSpeed *
            cursorAcceleration *
            deltaTime;

        cursorPosition += movement;

        cursorPosition.x = Mathf.Clamp(
            cursorPosition.x,
            0f,
            Screen.width
        );

        cursorPosition.y = Mathf.Clamp(
            cursorPosition.y,
            0f,
            Screen.height
        );

        SetCursorPosition(cursorPosition);
    }

    private void SetCursorPosition(Vector2 position)
    {
        if (Mouse.current != null)
        {
            Mouse.current.WarpCursorPosition(position);
        }
    }

    private void HandleClick()
    {
        if (!Gamepad.current.buttonSouth.wasPressedThisFrame)
            return;

        if (EventSystem.current == null)
            return;

        PointerEventData pointerData =
            new PointerEventData(EventSystem.current);

        pointerData.position = cursorPosition;
        pointerData.button = PointerEventData.InputButton.Left;

        var results = new System.Collections.Generic.List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count == 0)
            return;

        GameObject clickedObject = results[0].gameObject;

        ExecuteEvents.Execute(
            clickedObject,
            pointerData,
            ExecuteEvents.pointerClickHandler
        );
    }
}