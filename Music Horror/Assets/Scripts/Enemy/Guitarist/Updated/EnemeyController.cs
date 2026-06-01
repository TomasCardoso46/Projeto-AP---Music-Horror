using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyPerception))]
[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyAttack))]
public class EnemyController : MonoBehaviour, IEnemy
{
    public enum State { Idle, Patrol, Investigate, Chase, Attack, Dead }

    [SerializeField] private EnemySettings settings;
    [SerializeField] private Animator animator;

    // Core components
    [SerializeField] private EnemyPerception perception;
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private EnemyAttack attack;
    [SerializeField] private EnemyHealth health;

    public State currentState = State.Idle;

    private Transform target;
    private Vector3 lastKnownPosition;
    private float timeSinceSeen;

    // Investigate Audio
    [SerializeField] private AudioSource chaseAudioSource;
    [SerializeField] private AudioClip chaseClip;
    [SerializeField] private float chaseFadeOutTime = 1.2f;
    [SerializeField] [Range(0f, 1f)] private float investigateStartVolume = 1f;
    private Coroutine fadeOutCoroutine;
    private bool chaseAudioPlaying = false;

    // Investigate timing
    [SerializeField] private float minInvestigateTime = 3f;
    private float investigateTimer = 0f;

    // Sonar
    [SerializeField] private EnemySonar sonar;
    [SerializeField] private float sonarCooldown = 5f;
    private float sonarTimer = 0f;

    // Hiding spot destruction
    [Header("Hide Spot Destruction")]
    [SerializeField] private LayerMask hideSpotLayer;
    [SerializeField] private float destroyRadius = 10f;
    [SerializeField] private float destroyAttackDelay = 0.5f;

    private HideSpot playerHidingSpot = null;
    private bool destroyingHideSpot = false;
    private Transform player;

    private void Reset()
    {
        perception = GetComponent<EnemyPerception>();
        movement = GetComponent<EnemyMovement>();
        attack = GetComponent<EnemyAttack>();
        health = GetComponent<EnemyHealth>();
    }

    private void Awake()
    {
        perception ??= GetComponent<EnemyPerception>();
        movement ??= GetComponent<EnemyMovement>();
        attack ??= GetComponent<EnemyAttack>();
        health ??= GetComponent<EnemyHealth>();

        perception.Initialize(settings, this);
        movement.Initialize(settings, this);
        attack.Initialize(settings, this);
        if (health != null)
            health.Initialize(this);

        currentState = State.Patrol;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (health != null && !health.IsAlive)
        {
            SetState(State.Dead);
            return;
        }

        perception.Tick();

        // Normal perception handling
        if (perception.HasTarget)
        {
            target = perception.Target;
            lastKnownPosition = target.position;
            timeSinceSeen = 0f;
            investigateTimer = 0f;

            if (Vector3.Distance(transform.position, target.position) <= settings.AttackRange)
                SetState(State.Attack);
            else
                SetState(State.Chase);
        }
        else
        {
            timeSinceSeen += Time.deltaTime;

            if (currentState == State.Investigate)
                investigateTimer += Time.deltaTime;

            if (timeSinceSeen > settings.MemoryTime)
            {
                target = null;
                lastKnownPosition = Vector3.zero;
            }

            if (lastKnownPosition != Vector3.zero && timeSinceSeen <= settings.MemoryTime)
                SetState(State.Investigate);
            else if (currentState == State.Investigate && investigateTimer < minInvestigateTime)
                SetState(State.Investigate);
            else
            {
                SetState(State.Patrol);
                investigateTimer = 0f;
            }
        }

        // Execute behavior
        ExecuteState();

        // Sonar handling
        if (currentState == State.Patrol && sonar != null)
        {
            sonarTimer += Time.deltaTime;
            if (sonarTimer >= sonarCooldown)
            {
                sonar.ActivateAbility();
                sonarTimer = 0f;
            }
        }
        else
        {
            sonarTimer = 0f;
        }
        if (animator != null)
            animator.SetInteger("State", (int)currentState);
    }


    private void ExecuteState()
    {
        switch (currentState)
        {
            case State.Idle:
                movement.Idle();
                break;

            case State.Patrol:
                movement.Patrol();
                break;

            case State.Investigate:
                // Only move normally if not destroying hiding spots
                if (!destroyingHideSpot && lastKnownPosition != Vector3.zero)
                    movement.MoveTo(lastKnownPosition);
                break;

            case State.Chase:
                if (target != null)
                    movement.Chase(target);
                break;

            case State.Attack:
                if (target != null)
                {
                    Vector3 lookAt = new Vector3(target.position.x, transform.position.y, target.position.z);
                    transform.LookAt(lookAt);
                    attack.TryAttack(target);
                }
                else
                    SetState(State.Patrol);
                break;

            case State.Dead:
                movement.DisableMovement();
                attack.DisableAttack();
                FadeOutInvestigateAudio();
                break;
        }
    }

    public void SetState(State newState)
    {
        if (currentState == newState) return;

        // Investigate audio handling
        if (newState == State.Investigate)
            PlayInvestigateAudio();
        else if (currentState == State.Investigate && newState != State.Investigate)
            FadeOutInvestigateAudio();

        if (newState == State.Patrol)
            sonarTimer = 0f;

        currentState = newState;
    }

    private void PlayInvestigateAudio()
    {
        if (chaseAudioSource == null || chaseClip == null) return;

        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }

        chaseAudioSource.clip = chaseClip;
        chaseAudioSource.volume = investigateStartVolume;
        chaseAudioSource.loop = true;
        chaseAudioSource.Play();

        chaseAudioPlaying = true;
    }

    private void FadeOutInvestigateAudio()
    {
        if (!chaseAudioPlaying || chaseAudioSource == null) return;

        fadeOutCoroutine = StartCoroutine(FadeOutInvestigateAudioCoroutine());
    }

    private IEnumerator FadeOutInvestigateAudioCoroutine()
    {
        float startVolume = chaseAudioSource.volume;
        float t = 0f;

        while (t < chaseFadeOutTime)
        {
            t += Time.deltaTime;
            chaseAudioSource.volume = Mathf.Lerp(startVolume, 0f, t / chaseFadeOutTime);
            yield return null;
        }

        chaseAudioSource.Stop();
        chaseAudioSource.volume = investigateStartVolume;
        chaseAudioPlaying = false;
    }

    public void SetLastKnownPosition(Vector3 pos)
    {
        lastKnownPosition = pos;
    }
     public void ResetStateAfterLoad()
    {
        currentState = State.Patrol;
    }

   

    #region IEnemy Implementation
    public void AlertToPosition(Vector3 worldPos)
    {
        if (Random.value <= settings.InvestigateChance)
        {
            lastKnownPosition = worldPos;
            timeSinceSeen = 0f;
            investigateTimer = 0f;
            SetState(State.Investigate);
        }
    }

    public void AlertToTarget(Transform targetTransform)
    {
        target = targetTransform;
        lastKnownPosition = targetTransform.position;
        timeSinceSeen = 0f;
        investigateTimer = 0f;
        SetState(State.Chase);
    }

    public void TakeDamage(int amount, Vector3 hitPoint)
    {
        if (health != null)
        {
            health.TakeDamage(amount, hitPoint);
            if (!health.IsAlive)
            {
                SetState(State.Dead);
                return;
            }
        }

        lastKnownPosition = hitPoint;
        timeSinceSeen = 0f;
        investigateTimer = 0f;
        SetState(State.Investigate);
    }

    public bool IsAlive => health == null || health.IsAlive;
    #endregion
}
