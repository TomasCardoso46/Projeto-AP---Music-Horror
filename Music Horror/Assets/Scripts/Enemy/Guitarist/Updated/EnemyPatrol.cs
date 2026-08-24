using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Routes")]
    [Tooltip("Each element is a full patrol route (list of waypoints).")]
    [SerializeField] private List<List<Transform>> patrolRoutes = new List<List<Transform>>();

    [Tooltip("Inspector helper: Unity does NOT serialize nested lists well.")]
    [SerializeField] private List<PatrolRouteWrapper> inspectorRoutes = new List<PatrolRouteWrapper>();

    [Header("Roam Settings")]
    [SerializeField] private float minRoamTime = 2f;
    [SerializeField] private float maxRoamTime = 5f;

    [Header("Patrol Audio")]
    [Tooltip("Random sounds that can be played at the patrol point before the enemy leaves.")]
    [SerializeField] private List<AudioClip> patrolPointSounds = new List<AudioClip>();

    [Tooltip("The sound the enemy makes after the patrol point sound.")]
    [SerializeField] private AudioClip enemyDepartureSound;

    [Tooltip("Delay between the patrol point sound and the enemy sound.")]
    [SerializeField] private float soundDelay = 0.2f;

    [Tooltip("Delay after the enemy sound before the enemy starts moving.")]
    [SerializeField] private float enemySoundDelay = 0.5f;

    [Header("Patrol Point Sound Settings")]
    [Range(0f, 1f)]
    [Tooltip("0 = fully 2D, 1 = fully 3D.")]
    [SerializeField] private float patrolSoundSpatialBlend = 1f;

    [SerializeField] private float patrolSoundVolume = 1f;

    [Tooltip("Only affects 3D sounds.")]
    [SerializeField] private float patrolSoundMinDistance = 1f;

    [Tooltip("Only affects 3D sounds.")]
    [SerializeField] private float patrolSoundMaxDistance = 20f;

    private List<Transform> activeRoute;
    private int currentIndex = 0;
    private bool forward = true;
    private bool isRoaming = false;

    private NavMeshAgent agent;
    private EnemyMovement movement;
    [SerializeField] private AudioSource enemyAudio;

    public bool HasPatrol => activeRoute != null && activeRoute.Count > 0;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        movement = GetComponent<EnemyMovement>();
        //enemyAudio = GetComponent<AudioSource>();

        patrolRoutes = new List<List<Transform>>();

        foreach (var route in inspectorRoutes)
        {
            patrolRoutes.Add(route.points);
        }

        if (patrolRoutes.Count > 0)
            activeRoute = patrolRoutes[0];
    }

    private void Start()
    {
        if (HasPatrol)
        {
            agent.SetDestination(activeRoute[currentIndex].position);
        }
    }

    private void Update()
    {
        if (!HasPatrol || agent == null || !agent.isOnNavMesh || isRoaming)
            return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(HandlePatrolPoint());
        }
    }

    private IEnumerator HandlePatrolPoint()
    {
        isRoaming = true;

        agent.isStopped = true;
        agent.ResetPath();

        float roamTime = Random.Range(minRoamTime, maxRoamTime);

        if (movement != null && roamTime > 0f)
        {
            yield return movement.RoamAroundPoint(
                activeRoute[currentIndex].position,
                roamTime
            );
        }

        // Play random sound at patrol point
        yield return PlayPatrolPointSound();

        // Wait before enemy sound
        if (soundDelay > 0f)
            yield return new WaitForSeconds(soundDelay);

        // Play enemy sound
        PlayEnemyDepartureSound();

        // Wait before leaving
        if (enemySoundDelay > 0f)
            yield return new WaitForSeconds(enemySoundDelay);

        // Move to next patrol point
        AdvanceToNext();

        agent.isStopped = false;

        isRoaming = false;
    }

    private IEnumerator PlayPatrolPointSound()
    {
        if (patrolPointSounds == null || patrolPointSounds.Count == 0)
            yield break;

        List<AudioClip> validSounds = new List<AudioClip>();

        foreach (AudioClip clip in patrolPointSounds)
        {
            if (clip != null)
                validSounds.Add(clip);
        }

        if (validSounds.Count == 0)
            yield break;

        AudioClip randomClip = validSounds[
            Random.Range(0, validSounds.Count)
        ];

        // Create temporary sound object at patrol point
        GameObject soundObject = new GameObject("PatrolPointSound");

        soundObject.transform.position = activeRoute[currentIndex].position;

        AudioSource source = soundObject.AddComponent<AudioSource>();

        source.clip = randomClip;
        source.volume = patrolSoundVolume;

        // 0 = 2D
        // 1 = 3D
        source.spatialBlend = patrolSoundSpatialBlend;

        // These only matter when spatialBlend > 0
        source.minDistance = patrolSoundMinDistance;
        source.maxDistance = patrolSoundMaxDistance;

        source.Play();

        Destroy(soundObject, randomClip.length);

        yield return null;
    }

    private void PlayEnemyDepartureSound()
    {
        if (enemyDepartureSound == null)
            return;

        if (enemyAudio == null)
            return;

        enemyAudio.PlayOneShot(enemyDepartureSound);
    }

    public void AdvanceToNext()
    {
        if (!HasPatrol)
            return;

        currentIndex = GetNextIndex();

        agent.isStopped = false;
        agent.SetDestination(activeRoute[currentIndex].position);
    }

    private int GetNextIndex()
    {
        if (forward)
        {
            if (currentIndex + 1 >= activeRoute.Count)
            {
                forward = false;

                return Mathf.Max(0, currentIndex - 1);
            }

            return currentIndex + 1;
        }
        else
        {
            if (currentIndex - 1 < 0)
            {
                forward = true;

                return Mathf.Min(1, activeRoute.Count - 1);
            }

            return currentIndex - 1;
        }
    }

    public void SwitchPatrol(int index)
    {
        if (patrolRoutes == null || patrolRoutes.Count == 0)
        {
            Debug.LogWarning("No patrol routes defined.");
            return;
        }

        if (index < 0 || index >= patrolRoutes.Count)
        {
            Debug.LogWarning($"Invalid patrol index: {index}");
            return;
        }

        StopAllCoroutines();

        activeRoute = patrolRoutes[index];

        currentIndex = 0;
        forward = true;
        isRoaming = false;

        agent.isStopped = false;
        agent.ResetPath();

        if (HasPatrol)
        {
            agent.SetDestination(activeRoute[currentIndex].position);
        }
    }

    [System.Serializable]
    public class PatrolRouteWrapper
    {
        public List<Transform> points = new List<Transform>();
    }
}