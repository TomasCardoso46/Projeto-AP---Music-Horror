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

    [Header("NAVMESH AGENTS")]
    [SerializeField] private List<NavMeshAgent> navMeshAgents;
    [SerializeField] private Transform navSourceAnchor;
    [SerializeField] private Transform navTargetAnchor;

    [Header("CAMERAS")]
    [SerializeField] private FirstPersonRigidbodyController playerController;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera teleportCamera;
    [SerializeField] private Transform teleportCameraTarget; 

    [Header("Settings")]
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && hasTriggered) return;

        hasTriggered = true;
        StartCoroutine(TeleportSequence());
    }

    private IEnumerator TeleportSequence()
    {

        playerController.freezeCamera = true;
        playerController.HardResetCameraMotion();

        teleportCamera.transform.SetPositionAndRotation(
            teleportCameraTarget.position,
            teleportCameraTarget.rotation
        );


        mainCamera.enabled = false;
        teleportCamera.enabled = true;


        yield return new WaitForEndOfFrame();


        TeleportGroup(playerObjects, playerSourceAnchor, playerTargetAnchor);
        TeleportGroup(otherObjects, otherSourceAnchor, otherTargetAnchor);
        TeleportNavMeshAgents(navMeshAgents, navSourceAnchor, navTargetAnchor);

        Physics.SyncTransforms();
        enemyPatrol?.SwitchPatrol(3);

        yield return null;

        teleportCamera.enabled = false;
        mainCamera.enabled = true;

        playerController.freezeCamera = false;
    }

    private void TeleportGroup(List<Transform> objects, Transform source, Transform target)
    {
        if (objects == null || source == null || target == null) return;

        foreach (var obj in objects)
        {
            if (obj == null) continue;

            Vector3 relPos = source.InverseTransformPoint(obj.position);
            Quaternion relRot = Quaternion.Inverse(source.rotation) * obj.rotation;

            obj.position = target.TransformPoint(relPos);
            obj.rotation = target.rotation * relRot;
        }
    }

    private void TeleportNavMeshAgents(List<NavMeshAgent> agents, Transform source, Transform target)
    {
        if (agents == null || source == null || target == null) return;

        foreach (var agent in agents)
        {
            if (agent == null) continue;

            Transform t = agent.transform;

            Vector3 relPos = source.InverseTransformPoint(t.position);
            Vector3 newPos = target.TransformPoint(relPos);

            bool wasEnabled = agent.enabled;

            agent.enabled = false;
            t.position = newPos;
            agent.enabled = wasEnabled;

            agent.Warp(newPos);
            agent.nextPosition = newPos;
        }
    }
}