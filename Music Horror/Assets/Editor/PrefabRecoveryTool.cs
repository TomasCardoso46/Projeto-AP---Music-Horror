using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class PrefabRecoveryTool : EditorWindow
{
    private string targetName = "MyObjectName";
    private GameObject masterObject;

    [MenuItem("Tools/Prefab Recovery Tool")]
    public static void ShowWindow()
    {
        GetWindow<PrefabRecoveryTool>("Prefab Recovery");
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Recovery Tool", EditorStyles.boldLabel);

        targetName = EditorGUILayout.TextField("Object Name", targetName);

        if (GUILayout.Button("Find Objects"))
        {
            FindObjects();
        }

        if (masterObject != null)
        {
            GUILayout.Space(10);
            GUILayout.Label("Master Object:");
            EditorGUILayout.ObjectField(masterObject, typeof(GameObject), true);

            if (GUILayout.Button("Keep Master + Delete Others"))
            {
                ReplaceOthersWithMaster();
            }
        }
    }

    private List<GameObject> foundObjects = new List<GameObject>();

    private void FindObjects()
    {
        foundObjects = GameObject.FindObjectsOfType<GameObject>()
            .Where(go => go.name == targetName)
            .ToList();

        Debug.Log($"Found {foundObjects.Count} objects named '{targetName}'");

        if (foundObjects.Count > 0)
        {
            masterObject = foundObjects[0];
        }
    }

    private void ReplaceOthersWithMaster()
    {
        if (masterObject == null) return;

        foreach (var obj in foundObjects)
        {
            if (obj == masterObject) continue;

            Undo.DestroyObjectImmediate(obj);
        }

        Debug.Log("Duplicates removed. Only master remains.");
    }
}