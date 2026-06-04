using UnityEditor;
using UnityEngine;
using Unity.AI.Navigation;

public static class NavMeshRebakeAll
{
    [MenuItem("Tools/NavMesh/Rebake All Surfaces")]
    public static void RebakeAll()
    {
        NavMeshSurface[] surfaces = Object.FindObjectsByType<NavMeshSurface>(FindObjectsSortMode.None);

        if (surfaces == null || surfaces.Length == 0)
        {
            Debug.LogWarning("No NavMeshSurface components found in the scene.");
            return;
        }

        int count = 0;

        foreach (var surface in surfaces)
        {
            if (surface == null) continue;

            surface.BuildNavMesh();
            count++;
        }

        Debug.Log($"Rebaked {count} NavMeshSurface(s).");
    }
}