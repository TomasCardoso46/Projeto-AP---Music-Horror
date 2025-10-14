using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Enemy AI with Patrol / Investigate / Chase / Return states.
/// - Uses NavMeshAgent for obstacle avoidance and pathfinding.
/// - Subscribes to SoundEmitter.OnSoundPlayed events and reacts if the sound's tag is in hearingTags and within range.
/// - Fully blind (does not use vision). Hearing is dynamic (sound has a radius).
/// - Customize everything via serialized fields in the inspector.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    enum State { Patrol, Investigate, Chase, Returning }

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints = null;      // checkpoints editable in editor
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float patrolWaitTime = 1.0f;          // wait on each patrol point

    [Header("Chase / Investigate")]
    [SerializeField] private float chaseSpeed = 6.5f;
    [SerializeField] private float chaseStopDistance = 1.2f;       // stop distance while chasing
    [SerializeField] private float losePlayerDistance = 20f;      // if player farther than this, stop chasing and go to lastSoundPosition
    [SerializeField] private float investigateWaitTime = 4.0f;    // time to wait at last sound position before returning to patrol

    [Header("Hearing")]
    [SerializeField] private List<string> hearingTags = new List<string>() { "Player", "Noise" };
    [SerializeField] private bool requirePlayerTagToChase = true; // if true, enemy chases the 'Player' tag when it hears any allowed sound
    [SerializeField] private string playerTag = "Player";

    [Header("Misc")]
    [SerializeField] private bool startPatrollingOnStart = true;
    [SerializeField] private float stuckRecoveryDistance = 0.5f;   // if agent doesn't move much, treat as stuck
    [SerializeField] private float stuckRecoveryTimeout = 2f;

    // runtime
    private NavMeshAgent agent;
    private State currentState = State.Patrol;
    private int patrolIndex = 0;
    private float stateTimer = 0f;
    private Vector3 lastSoundPosition;
    private Transform playerTransform;    // cached when found
    private bool hasLastSound = false;

    // stuck detection
    private Vector3 lastAgentPos;
    private float stuckTimer = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        SoundEmitter.OnSoundPlayed += HandleSound;
    }

    private void OnDisable()
    {
        SoundEmitter.OnSoundPlayed -= HandleSound;
    }

    private void Start()
    {
        if (startPatrollingOnStart && patrolPoints != null && patrolPoints.Length > 0)
        {
            currentState = State.Patrol;
            agent.speed = patrolSpeed;
            GoToPatrolPoint(patrolIndex);
        }
        else
        {
            // If no patrol points, just idle.
            currentState = State.Patrol;
            agent.isStopped = true;
        }

        lastAgentPos = transform.position;
    }

    private void Update()
    {
        stateTimer += Time.deltaTime;

        // stuck detection
        if (Vector3.Distance(transform.position, lastAgentPos) < stuckRecoveryDistance * Time.deltaTime)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= stuckRecoveryTimeout)
            {
                // try simple recovery: re-path to destination
                if (agent.hasPath)
                {
                    var d = agent.destination;
                    agent.ResetPath();
                    agent.SetDestination(d);
                }
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
        lastAgentPos = transform.position;

        switch (currentState)
        {
            case State.Patrol:
                UpdatePatrol();
                break;
            case State.Investigate:
                UpdateInvestigate();
                break;
            case State.Chase:
                UpdateChase();
                break;
            case State.Returning:
                UpdateReturning();
                break;
        }
    }

    private void UpdatePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // arrived
            if (stateTimer >= patrolWaitTime)
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                GoToPatrolPoint(patrolIndex);
                stateTimer = 0f;
            }
        }
    }

    private void GoToPatrolPoint(int index)
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        agent.isStopped = false;
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.1f;
        agent.SetDestination(patrolPoints[index].position);
    }

    private void UpdateInvestigate()
    {
        agent.isStopped = false;
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.1f;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // arrived to last heard position, wait
            if (stateTimer >= investigateWaitTime)
            {
                // give up and return to patrol
                StartReturningToPatrol();
            }
        }
    }

    private void UpdateChase()
    {
        if (playerTransform == null)
        {
            // If we no longer have a reference, try to find it
            var pgo = GameObject.FindWithTag(playerTag);
            if (pgo != null) playerTransform = pgo.transform;
            else
            {
                // can't find player, switch to investigate last sound
                StartInvestigate();
                return;
            }
        }

        // continuously chase player's current position
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.stoppingDistance = chaseStopDistance;
        agent.SetDestination(playerTransform.position);

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distToPlayer > losePlayerDistance)
        {
            // lost the player, go to last known sound position (if we have it)
            if (hasLastSound)
            {
                StartInvestigate(); // Investigate goes to lastSoundPosition
            }
            else
            {
                StartReturningToPatrol();
            }
        }
    }

    private void UpdateReturning()
    {
        agent.isStopped = false;
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.1f;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // reached patrol area -> resume patrol
            currentState = State.Patrol;
            stateTimer = 0f;
            GoToPatrolPoint(patrolIndex);
        }
    }

    private void StartInvestigate()
    {
        if (!hasLastSound)
        {
            StartReturningToPatrol();
            return;
        }

        currentState = State.Investigate;
        stateTimer = 0f;
        agent.isStopped = false;
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.1f;
        agent.SetDestination(lastSoundPosition);
    }

    private void StartChase(Transform player)
    {
        playerTransform = player;
        currentState = State.Chase;
        stateTimer = 0f;
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.stoppingDistance = chaseStopDistance;
        // immediate set destination to player's last known position:
        if (playerTransform != null) agent.SetDestination(playerTransform.position);
    }

    private void StartReturningToPatrol()
    {
        currentState = State.Returning;
        stateTimer = 0f;
        agent.isStopped = false;
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.1f;

        // set destination to nearest patrol point to resume smoothly
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            int nearest = 0;
            float best = float.MaxValue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                float d = Vector3.Distance(transform.position, patrolPoints[i].position);
                if (d < best) { best = d; nearest = i; }
            }
            patrolIndex = nearest;
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
        else
        {
            agent.ResetPath();
            agent.isStopped = true;
        }

        // clear last sound
        hasLastSound = false;
        playerTransform = null;
    }

    /// <summary>
    /// Sound event handler
    /// </summary>
    private void HandleSound(SoundEmitter.SoundEvent sound)
    {
        // ignore if the sound's tag isn't one we listen to
        if (!hearingTags.Contains(sound.sourceTag)) return;

        // dynamic hearing based on distance and sound.radius
        float dist = Vector3.Distance(transform.position, sound.position);
        if (dist > sound.radius) return; // can't hear it

        // record last heard position
        lastSoundPosition = sound.position;
        hasLastSound = true;

        // if we must chase the player when a sound is heard, try to find player object
        if (requirePlayerTagToChase)
        {
            GameObject pgo = GameObject.FindWithTag(playerTag);
            if (pgo != null)
            {
                StartChase(pgo.transform);
                return;
            }
        }

        // if not chasing player directly, go investigate sound
        StartInvestigate();
    }

    #region Editor Gizmos
    private void OnDrawGizmosSelected()
    {
        // draw patrol points lines
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null) Gizmos.DrawWireSphere(patrolPoints[i].position, 0.2f);
            }
        }

        // draw last sound pos
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(lastSoundPosition, 0.25f);
    }
    #endregion
}
