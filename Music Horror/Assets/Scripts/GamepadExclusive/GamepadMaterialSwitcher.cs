using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadMaterialSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class MaterialEntry
    {
        public Renderer targetRenderer;
        public Material materialA;
        public Material materialB;
    }

    [Header("Objects")]
    [SerializeField] private List<MaterialEntry> objects = new List<MaterialEntry>();

    private bool gamepadConnected;

    private void Start()
    {
        UpdateGamepadState(true);
    }

    private void Update()
    {
        bool currentlyConnected = Gamepad.current != null;

        if (currentlyConnected != gamepadConnected)
        {
            UpdateGamepadState();
        }
    }

    private void UpdateGamepadState(bool forceUpdate = false)
    {
        bool currentlyConnected = Gamepad.current != null;

        if (!forceUpdate && currentlyConnected == gamepadConnected)
            return;

        gamepadConnected = currentlyConnected;

        ApplyMaterials();
    }

    private void ApplyMaterials()
    {
        foreach (MaterialEntry entry in objects)
        {
            if (entry.targetRenderer == null)
                continue;

            Material materialToUse = gamepadConnected
                ? entry.materialB
                : entry.materialA;

            if (materialToUse == null)
                continue;

            entry.targetRenderer.material = materialToUse;
        }
    }
}