using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

[DisallowMultipleComponent]
public class NavMeshLinkTraversalSettings : MonoBehaviour
{
    [Header("Traversal Mode")]
    [Tooltip("If enabled, agents will play an animation while traversing all links on this object")]
    public bool useAnimationTraversal = false;

    [Header("Animation Traversal Settings")]
    public string traversalTrigger = "Traverse";
    public float animationDuration = 1.0f;

    [Header("Speed-Based Traversal Settings")]
    public float jumpHeight = 0f;

    private NavMeshLink[] links;

    void Awake()
    {
        links = GetComponents<NavMeshLink>();

        if (links.Length == 0)
        {
            Debug.LogWarning($"NavMeshLinkTraversalSettings on '{name}' has no NavMeshLinks.");
            return;
        }

        // Ensure consistency across all links on this object
        foreach (NavMeshLink link in links)
        {
            link.bidirectional = true;
        }
    }
}
