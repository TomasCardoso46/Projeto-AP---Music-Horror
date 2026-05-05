using UnityEngine;

public class SimpleSpawner : MonoBehaviour
{
    public void SpawnAndAutoDestroy(GameObject prefab, Transform parent, float lifetime)
    {
        if (prefab == null || parent == null)
        {
            Debug.LogWarning("Spawn failed: prefab or parent is null.");
            return;
        }

        // Spawn as child of the parent
        GameObject instance = Instantiate(prefab, parent);

        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        // Destroy after time
        Destroy(instance, lifetime);
    }
}