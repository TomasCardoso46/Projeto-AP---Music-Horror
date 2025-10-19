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

    // Core components (private but visible for debugging)
    [SerializeField] private EnemyPerception perception;
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private EnemyAttack attack;
    [SerializeField] private EnemyHealth health; // optional

    [SerializeField] private State currentState = State.Idle;

    private Transform target;
    private Vector3 lastKnownPosition;
    private float timeSinceSeen;

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
        health = GetComponent<EnemyHealth>(); // optional now

        // Initialize subsystems
        perception.Initialize(settings, this);
        movement.Initialize(settings, this);
        attack.Initialize(settings, this);

        if (health != null)
            health.Initialize(this);

        currentState = State.Patrol;
    }

    private void Update()
    {
        // Only check death if health exists and can die
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
        if (animator != null) animator.SetInteger("State", (int)currentState);
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
                break;
        }
    }

    private void SetState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        // Debug.Log($"{name} → {currentState}");
    }

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

        // Always become alert when attacked
        lastKnownPosition = hitPoint;
        timeSinceSeen = 0f;
        SetState(State.Chase);
    }

    public bool IsAlive => health == null || health.IsAlive;
    #endregion
}
