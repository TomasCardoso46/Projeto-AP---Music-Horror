using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class GamepadMaterialSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class MaterialEntry
    {
        public Renderer targetRenderer;
        public DecalProjector decalProjector;
        public SpriteRenderer sprite;
        public MeshRenderer meshRenderer;
        public Sprite spriteA;
        public Sprite spriteB;
        public Material materialA;
        public Material materialB;
        public Mesh meshA;
        public Mesh meshB;
        public bool isDecal;
        public bool isSprite;
        public bool isMesh;
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

            Material materialToUse = gamepadConnected
                ? entry.materialB
                : entry.materialA;
            if (materialToUse == null)
                continue;

            if (entry.isSprite)
            {
                Sprite spriteToUse = gamepadConnected
                    ? entry.spriteA
                    : entry.spriteB;

                if (spriteToUse == null)
                    continue;

                entry.sprite.sprite = spriteToUse;
            }



            //entry.targetRenderer.material = materialToUse;
            if (entry.isDecal)
            {
                entry.decalProjector.material = materialToUse;
            }
            else
            {
                entry.targetRenderer.material = materialToUse;
            }
        }
    }

}