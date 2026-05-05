using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyPerception : MonoBehaviour
{
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private LayerMask obstructionMask;

    private EnemySettings settings;
    private EnemyController controller;
    private Transform _target;
    public Transform Target => _target;
    public bool HasTarget { get; private set; }

    public void Initialize(EnemySettings settingsAsset, EnemyController owner)
    {
        settings = settingsAsset;
        controller = owner;
    }

    public void Tick()
    {
        HasTarget = false;

        GameObject found = GameObject.FindWithTag(targetTag);
        if (found != null)
        {
            Transform t = found.transform;
            if (CanSee(t))
            {
                HasTarget = true;
                _target = t;
                return;
            }
        }

        // Hearing: sphere overlap to detect noisy objects
        Collider[] hits = Physics.OverlapSphere(transform.position, settings.HighHearingRange); // use largest
        foreach (var col in hits)
        {
            var emitter = col.GetComponent<EnemyAudioEmitter>();
            if (emitter != null)
            {
                float distance = Vector3.Distance(transform.position, emitter.transform.position);

                if ((emitter.IsEmittingHigh && distance <= settings.HighHearingRange) ||
                    (emitter.IsEmittingNormal && distance <= settings.NormalHearingRange) ||
                    (emitter.IsEmittingLow && distance <= settings.LowHearingRange))
                {
                    controller.AlertToPosition(emitter.transform.position);
                    return;
                }
            }
        }

    }

    private bool CanSee(Transform t)
    {
        Vector3 dir = (t.position - transform.position);
        float dist = dir.magnitude;
        if (dist > settings.SightRange) return false;
        dir.Normalize();

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > settings.SightFOV * 0.5f) return false;

        Ray ray = new Ray(transform.position + Vector3.up * 0.8f, dir);
        if (Physics.Raycast(ray, out RaycastHit hit, settings.SightRange, ~0))
        {
            if (hit.transform == t || hit.transform.IsChildOf(t)) return true;
            return false;
        }
        return false;
    }
}
