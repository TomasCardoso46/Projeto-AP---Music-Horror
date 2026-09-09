using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CustomMenuEventSystem : MonoBehaviour
{
    [System.Serializable]
    public class MenuItem
    {
        [Header("UI Element")]
        public Selectable selectable;

        [Header("Navigation")]
        public Selectable up;
        public Selectable down;
        public Selectable left;
        public Selectable right;
    }

    [Header("First Selected")]
    [SerializeField] private Selectable firstSelected;

    [Header("Menu Items")]
    [SerializeField] private List<MenuItem> menuItems = new();

    [Header("Navigation Input")]
    [SerializeField] private InputActionReference navigateAction;

    [Header("Submit / Cancel")]
    [SerializeField] private InputActionReference submitAction;
    [SerializeField] private InputActionReference cancelAction;

    [Header("Navigation Settings")]
    [SerializeField] private bool wrapAround = false;
    [SerializeField] private float navigationRepeatDelay = 0.35f;
    [SerializeField] private float navigationRepeatRate = 0.12f;

    [Header("Behaviour")]
    [SerializeField] private bool selectFirstOnEnable = true;
    [SerializeField] private bool ignoreTimeScale = true;
    [SerializeField] private bool requireInteractable = true;

    private Selectable currentSelected;

    private float navigationRepeatTimer;
    private bool navigationHeld;
    private Vector2 lastNavigationDirection;

    private void OnEnable()
    {
        EnableAction(navigateAction);
        EnableAction(submitAction);
        EnableAction(cancelAction);

        if (selectFirstOnEnable)
        {
            SelectFirst();
        }
    }

    private void OnDisable()
    {
        DisableAction(navigateAction);
        DisableAction(submitAction);
        DisableAction(cancelAction);
    }

    private void Update()
    {
        ValidateCurrentSelection();
        HandleNavigation();
        HandleSubmit();
        HandleCancel();
    }

    private void ValidateCurrentSelection()
    {
        if (currentSelected == null)
        {
            SelectFirst();
            return;
        }

        if (!currentSelected.gameObject.activeInHierarchy)
        {
            SelectFirstValidItem();
            return;
        }

        if (requireInteractable && !currentSelected.IsInteractable())
        {
            SelectFirstValidItem();
            return;
        }

        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject != currentSelected.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(currentSelected.gameObject);
        }
    }

    private void HandleNavigation()
    {
        if (navigateAction == null)
            return;

        Vector2 input = navigateAction.action.ReadValue<Vector2>();

        if (input.magnitude < 0.5f)
        {
            navigationHeld = false;
            navigationRepeatTimer = 0f;
            lastNavigationDirection = Vector2.zero;
            return;
        }

        Vector2 direction = GetCardinalDirection(input);

        if (direction == Vector2.zero)
            return;

        if (!navigationHeld)
        {
            navigationHeld = true;
            navigationRepeatTimer = navigationRepeatDelay;
            lastNavigationDirection = direction;

            Navigate(direction);
            return;
        }

        if (Vector2.Dot(direction, lastNavigationDirection) < 0.8f)
        {
            navigationRepeatTimer = navigationRepeatDelay;
            lastNavigationDirection = direction;

            Navigate(direction);
            return;
        }

        navigationRepeatTimer -= GetDeltaTime();

        if (navigationRepeatTimer <= 0f)
        {
            navigationRepeatTimer = navigationRepeatRate;
            Navigate(direction);
        }
    }

    private void HandleSubmit()
    {
        if (submitAction == null)
            return;

        if (!submitAction.action.WasPressedThisFrame())
            return;

        if (currentSelected == null)
            return;

        if (!IsValidSelectable(currentSelected))
            return;

        ExecuteSubmit(currentSelected);
    }

    private void HandleCancel()
    {
        if (cancelAction == null)
            return;

        if (!cancelAction.action.WasPressedThisFrame())
            return;

        ExecuteCancel();
    }

    private void Navigate(Vector2 direction)
    {
        if (currentSelected == null)
        {
            SelectFirst();
            return;
        }

        MenuItem currentItem = FindMenuItem(currentSelected);

        if (currentItem == null)
            return;

        Selectable target = GetTarget(currentItem, direction);

        if (IsValidSelectable(target))
        {
            Select(target);
            return;
        }

        if (wrapAround)
        {
            target = FindWrapAroundTarget(direction);

            if (IsValidSelectable(target))
            {
                Select(target);
            }
        }
    }

    private Selectable GetTarget(MenuItem item, Vector2 direction)
    {
        if (direction.y > 0.5f)
            return item.up;

        if (direction.y < -0.5f)
            return item.down;

        if (direction.x < -0.5f)
            return item.left;

        if (direction.x > 0.5f)
            return item.right;

        return null;
    }

    private Vector2 GetCardinalDirection(Vector2 input)
    {
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            return input.x > 0f
                ? Vector2.right
                : Vector2.left;
        }

        return input.y > 0f
            ? Vector2.up
            : Vector2.down;
    }

    private Selectable FindWrapAroundTarget(Vector2 direction)
    {
        if (menuItems.Count == 0)
            return null;

        List<Selectable> validItems = new();

        foreach (MenuItem item in menuItems)
        {
            if (item == null)
                continue;

            if (!IsValidSelectable(item.selectable))
                continue;

            validItems.Add(item.selectable);
        }

        if (validItems.Count == 0)
            return null;

        int currentIndex = validItems.IndexOf(currentSelected);

        if (currentIndex < 0)
            return validItems[0];

        if (direction.y > 0.5f || direction.x < -0.5f)
        {
            return validItems[
                (currentIndex - 1 + validItems.Count) % validItems.Count
            ];
        }

        if (direction.y < -0.5f || direction.x > 0.5f)
        {
            return validItems[
                (currentIndex + 1) % validItems.Count
            ];
        }

        return null;
    }

    private void SelectFirst()
    {
        if (IsValidSelectable(firstSelected))
        {
            Select(firstSelected);
            return;
        }

        SelectFirstValidItem();
    }

    private void SelectFirstValidItem()
    {
        foreach (MenuItem item in menuItems)
        {
            if (item == null)
                continue;

            if (!IsValidSelectable(item.selectable))
                continue;

            Select(item.selectable);
            return;
        }

        currentSelected = null;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private bool IsValidSelectable(Selectable selectable)
    {
        if (selectable == null)
            return false;

        if (!selectable.gameObject.activeInHierarchy)
            return false;

        if (requireInteractable && !selectable.IsInteractable())
            return false;

        return true;
    }

    private void Select(Selectable selectable)
    {
        if (!IsValidSelectable(selectable))
            return;

        currentSelected = selectable;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }

        selectable.Select();

        selectable.OnSelect(
            new BaseEventData(EventSystem.current)
        );
    }

    private MenuItem FindMenuItem(Selectable selectable)
    {
        foreach (MenuItem item in menuItems)
        {
            if (item != null && item.selectable == selectable)
                return item;
        }

        return null;
    }

    private void ExecuteSubmit(Selectable selectable)
    {
        Button button = selectable as Button;

        if (button != null)
        {
            button.onClick.Invoke();
            return;
        }

        Toggle toggle = selectable as Toggle;

        if (toggle != null)
        {
            toggle.isOn = !toggle.isOn;
            return;
        }

        ExecuteEvents.Execute(
            selectable.gameObject,
            new BaseEventData(EventSystem.current),
            ExecuteEvents.submitHandler
        );
    }

    private void ExecuteCancel()
    {
        ExecuteEvents.Execute(
            gameObject,
            new BaseEventData(EventSystem.current),
            ExecuteEvents.cancelHandler
        );
    }

    public void SelectItem(Selectable selectable)
    {
        Select(selectable);
    }

    public void SelectItemByIndex(int index)
    {
        if (index < 0 || index >= menuItems.Count)
            return;

        MenuItem item = menuItems[index];

        if (item == null)
            return;

        Select(item.selectable);
    }

    public Selectable GetCurrentSelected()
    {
        return currentSelected;
    }

    public void SetFirstSelected(Selectable selectable)
    {
        firstSelected = selectable;
    }

    private void EnableAction(InputActionReference actionReference)
    {
        if (actionReference != null)
            actionReference.action.Enable();
    }

    private void DisableAction(InputActionReference actionReference)
    {
        if (actionReference != null)
            actionReference.action.Disable();
    }

    private float GetDeltaTime()
    {
        return ignoreTimeScale
            ? Time.unscaledDeltaTime
            : Time.deltaTime;
    }
}