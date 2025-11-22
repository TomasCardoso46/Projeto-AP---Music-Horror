using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyPerception))]
[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyAttack))]
public class EnemyController : MonoBehaviour, IEnemy
{
    public enum State { Idle, Patrol, Investigate, Chase, Attack, Dead }

    [SerializeField] [Tooltip("Settings asset that contains adjustable values.")]
    private EnemySettings settings;

    [SerializeField] [Tooltip("Optional animator to sync states.")]
    private Animator animator;

    // Core components
    [SerializeField] private EnemyPerception perception;
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private EnemyAttack attack;
    [SerializeField] private EnemyHealth health;

    [SerializeField] private State currentState = State.Idle;

    private Transform target;
    private Vector3 lastKnownPosition;
    private float timeSinceSeen;

    // -----------------------------
    // AUDIO SETTINGS
    // -----------------------------
    [Header("Investigate Audio")]
    [SerializeField] private AudioSource chaseAudioSource;   // <-- your own audio source (e.g. player audio)
    [SerializeField] private AudioClip chaseClip;

    [SerializeField] private float chaseFadeOutTime = 1.2f;

    // NEW: Customize starting volume
    [SerializeField] [Range(0f, 1f)]
    private float investigateStartVolume = 1f;

    private Coroutine fadeOutCoroutine;
    private bool chaseAudioPlaying = false;
    // -----------------------------

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
        health = GetComponent<EnemyHealth>();

        perception.Initialize(settings, this);
        movement.Initialize(settings, this);
        attack.Initialize(settings, this);

        if (health != null)
            health.Initialize(this);

        currentState = State.Patrol;
    }

    private void Update()
    {
        if (health != null && !health.IsAlive)
        {
            SetState(State.Dead);
            return;
        }

        perception.Tick();

        if (perception.HasTarget)
        {
            target = perception.Target;
            lastKnownPosition = target.position;
            timeSinceSeen = 0f;

            if (Vector3.Distance(transform.position, target.position) <= settings.AttackRange)
                SetState(State.Attack);
            else
                SetState(State.Chase);
        }
        else
        {
            timeSinceSeen += Time.deltaTime;

            if (timeSinceSeen > settings.MemoryTime)
            {
                target = null;
                lastKnownPosition = Vector3.zero;
            }

            if (lastKnownPosition != Vector3.zero && timeSinceSeen <= settings.MemoryTime)
                SetState(State.Investigate);
            else
                SetState(State.Patrol);
        }

        ExecuteState();

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

    private void SetState(State newState)
    {
        if (currentState == newState) return;

        // AUDIO HANDLING FOR INVESTIGATE
        if (newState == State.Investigate)
        {
            PlayInvestigateAudio();
        }
        else if (currentState == State.Investigate && newState != State.Investigate)
        {
            FadeOutInvestigateAudio();
        }

        currentState = newState;
    }

    // ============================================================
    // AUDIO FUNCTIONS
    // ============================================================
    private void PlayInvestigateAudio()
    {
        if (chaseAudioSource == null || chaseClip == null)
            return;

        // Cancel fade-out if running
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }

        chaseAudioSource.clip = chaseClip;
        chaseAudioSource.volume = investigateStartVolume; // <-- uses your editor value
        chaseAudioSource.loop = true;
        chaseAudioSource.Play();

        chaseAudioPlaying = true;
    }

    private void FadeOutInvestigateAudio()
    {
        if (!chaseAudioPlaying || chaseAudioSource == null)
            return;

        fadeOutCoroutine = StartCoroutine(FadeOutInvestigateAudioCoroutine());
    }

    private IEnumerator FadeOutInvestigateAudioCoroutine()
    {
        float startVolume = chaseAudioSource.volume;
        float t = 0f;

        while (t < chaseFadeOutTime)
        {
            t += Time.deltaTime;
            chaseAudioSource.volume =
                Mathf.Lerp(startVolume, 0f, t / chaseFadeOutTime);

            yield return null;
        }

        chaseAudioSource.Stop();
        chaseAudioSource.volume = investigateStartVolume; // Reset for next time
        chaseAudioPlaying = false;
    }

    // ============================================================

    #region IEnemy Implementation
    public void AlertToPosition(Vector3 worldPos)
    {
        if (Random.value <= settings.InvestigateChance)
        {
            lastKnownPosition = worldPos;
            timeSinceSeen = 0f;
            SetState(State.Investigate);
        }
    }

    public void AlertToTarget(Transform targetTransform)
    {
        target = targetTransform;
        lastKnownPosition = targetTransform.position;
        timeSinceSeen = 0f;
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
        SetState(State.Investigate);
    }

    public bool IsAlive => health == null || health.IsAlive;
    #endregion
}
