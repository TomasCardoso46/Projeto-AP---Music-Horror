#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class DisableEditorShortcuts
{
    static DisableEditorShortcuts()
    {
        // Hook into the editor update loop
        EditorApplication.update += DisableShortcuts;
    }

    static void DisableShortcuts()
    {
        // Only act in Play Mode
        if (!EditorApplication.isPlaying) return;

        // Consume key events to prevent default shortcuts
        if (Event.current != null && Event.current.type == EventType.KeyDown)
        {
            // Example: block F1-F12 keys
            if (Event.current.keyCode >= KeyCode.F1 && Event.current.keyCode <= KeyCode.F12)
            {
                Event.current.Use(); // Prevents Unity from using this key
                Debug.Log("Blocked Unity shortcut: " + Event.current.keyCode);
            }

            // Example: block Ctrl+S
            if (Event.current.control && Event.current.keyCode == KeyCode.S)
            {
                Event.current.Use();
                Debug.Log("Blocked Ctrl+S");
            }
        }
    }
}
#endif
