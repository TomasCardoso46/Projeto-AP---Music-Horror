using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MultiTeleportTrigger : MonoBehaviour
{
    [Header("PLAYER GROUP")]
    [SerializeField] private List<Transform> playerObjects;
    [SerializeField] private Transform playerSourceAnchor;
    [SerializeField] private Transform playerTargetAnchor;

    [Header("OTHER GROUP")]
    [SerializeField] private List<Transform> otherObjects;
    [SerializeField] private Transform otherSourceAnchor;
    [SerializeField] private Transform otherTargetAnchor;
    [SerializeField] private EnemyPatrol enemyPatrol;

    [Header("NAVMESH AGENTS GROUP")]
    [SerializeField] private List<NavMeshAgent> navMeshAgents;
    [SerializeField] private Transform navSourceAnchor;
    [SerializeField] private Transform navTargetAnchor;

    [Header("Settings")]
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && hasTriggered) return;

        hasTriggered = true;
        StartCoroutine(TeleportEndOfFrame());
    }

    private IEnumerator TeleportEndOfFrame()
    {
        // Wait until all physics + trigger processing is fully done
        yield return new WaitForEndOfFrame();

        // Now everything is safe to move without rendering artifacts
        TeleportGroup(playerObjects, playerSourceAnchor, playerTargetAnchor);
        TeleportGroup(otherObjects, otherSourceAnchor, otherTargetAnchor);
        TeleportNavMeshAgents(navMeshAgents, navSourceAnchor, navTargetAnchor);
    }

    private void TeleportGroup(List<Transform> objects, Transform source, Transform target)
    {
        if (objects == null || source == null || target == null) return;

        foreach (var obj in objects)
        {
            if (obj == null) continue;

            Vector3 relativePos = source.InverseTransformPoint(obj.position);
            Quaternion relativeRot = Quaternion.Inverse(source.rotation) * obj.rotation;

            obj.position = target.TransformPoint(relativePos);
            obj.rotation = target.rotation * relativeRot;
        }
    }

    private void TeleportNavMeshAgents(List<NavMeshAgent> agents, Transform source, Transform target)
    {
        if (agents == null || source == null || target == null) return;

        foreach (var agent in agents)
        {
            if (agent == null) continue;

            Transform obj = agent.transform;

            Vector3 relativePos = source.InverseTransformPoint(obj.position);
            Quaternion relativeRot = Quaternion.Inverse(source.rotation) * obj.rotation;

            Vector3 newWorldPos = target.TransformPoint(relativePos);
            Quaternion newWorldRot = target.rotation * relativeRot;

            bool wasEnabled = agent.enabled;

            // Prevent NavMesh from fighting the teleport
            agent.enabled = false;

            obj.SetPositionAndRotation(newWorldPos, newWorldRot);

            agent.enabled = wasEnabled;

            // Hard snap NavMesh internal state to avoid one-frame ghosting
            agent.Warp(newWorldPos);
            agent.nextPosition = newWorldPos;
        }

        // Ensure all transforms are fully synchronized before next render
        Physics.SyncTransforms();

        enemyPatrol?.SwitchPatrol(3);
    }
}