using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.VisualScripting;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentLinkTraversalController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    private bool isTraversing;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        agent.autoTraverseOffMeshLink = false;

        if (animator == null)
        {
            Debug.LogWarning($"[{name}] No Animator found on agent or children. Animation-based traversal will not work.");
        }
    }

    void Update()
    {
        if (!isTraversing && agent.isOnOffMeshLink)
        {
            StartCoroutine(HandleLinkTraversal());
        }
    }

    private IEnumerator HandleLinkTraversal()
    {
        isTraversing = true;

        OffMeshLinkData data = agent.currentOffMeshLinkData;
        GameObject linkObject = data.owner.GameObject();

        // Try to find settings on parent or self
        NavMeshLinkTraversalSettings settings =
            linkObject.GetComponentInParent<NavMeshLinkTraversalSettings>();

        if (settings == null)
        {
            Debug.LogWarning($"[{name}] No NavMeshLinkTraversalSettings found for link '{linkObject.name}'. Using default speed-based traversal.");
        }

        // Decide which traversal method
        if (settings != null && settings.useAnimationTraversal && animator != null)
        {
            yield return AnimationBasedTraversal(data, settings);
        }
        else
        {
            float jumpHeight = settings != null ? settings.jumpHeight : 0f;
            yield return SpeedBasedTraversal(data, jumpHeight);
        }

        agent.CompleteOffMeshLink();
        isTraversing = false;
    }

    private IEnumerator SpeedBasedTraversal(OffMeshLinkData data, float jumpHeight)
    {
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;

        float distance = Vector3.Distance(startPos, endPos);
        float duration = distance / Mathf.Max(agent.speed, 0.001f); // avoid divide by zero
        float elapsed = 0f;

        agent.updatePosition = false;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            Vector3 position = Vector3.Lerp(startPos, endPos, t);

            if (jumpHeight > 0f)
            {
                position += Vector3.up * Mathf.Sin(t * Mathf.PI) * jumpHeight;
            }

            agent.transform.position = position;

            elapsed += Time.deltaTime;
            yield return null;
        }

        agent.transform.position = endPos;
        agent.updatePosition = true;
    }

    private IEnumerator AnimationBasedTraversal(OffMeshLinkData data, NavMeshLinkTraversalSettings settings)
    {
        if (animator == null)
        {
            Debug.LogWarning($"[{name}] Animator missing, cannot use animation traversal. Falling back to speed-based movement.");
            float jumpHeight = settings != null ? settings.jumpHeight : 0f;
            yield return SpeedBasedTraversal(data, jumpHeight);
            yield break;
        }

        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;

        agent.isStopped = true;
        agent.updatePosition = false;

        // Fire traversal animation trigger safely
        if (!string.IsNullOrEmpty(settings.traversalTrigger))
        {
            animator.ResetTrigger(settings.traversalTrigger);
            animator.SetTrigger(settings.traversalTrigger);
        }

        float elapsed = 0f;
        float duration = Mathf.Max(settings.animationDuration, 0.01f);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            agent.transform.position = Vector3.Lerp(startPos, endPos, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        agent.transform.position = endPos;

        agent.isStopped = false;
        agent.updatePosition = true;
    }
}
