using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Horror Enemy/Enemy Settings", fileName = "EnemySettings")]
public class EnemySettings : ScriptableObject
{
    [Header("Perception")]
    [Tooltip("Field of view angle (degrees) for sight.")]
    [Range(0f, 180f)]
    [SerializeField] private float sightFOV = 90f;
    public float SightFOV => sightFOV;

    [Tooltip("Maximum sight distance.")]
    [SerializeField] private float sightRange = 20f;
    public float SightRange => sightRange;

    [Header("Hearing Ranges")]
    [SerializeField] private float lowHearingRange = 3f;
    [SerializeField] private float normalHearingRange = 8f;
    [SerializeField] private float highHearingRange = 12f;

    public float LowHearingRange => lowHearingRange;
    public float NormalHearingRange => normalHearingRange;
    public float HighHearingRange => highHearingRange;

    [Tooltip("How long the enemy remembers the last known position (seconds).")]
    [SerializeField] private float memoryTime = 6f;
    public float MemoryTime => memoryTime;

    [Header("Movement")]
    [Tooltip("Patrol speed.")]
    [SerializeField] private float patrolSpeed = 1.8f;
    public float PatrolSpeed => patrolSpeed;

    [Tooltip("Chase/engaged speed.")]
    [SerializeField] private float chaseSpeed = 4.5f;
    public float ChaseSpeed => chaseSpeed;

    [Tooltip("How close to the target before stopping (meters).")]
    [SerializeField] private float stoppingDistance = 1.2f;
    public float StoppingDistance => stoppingDistance;

    [Header("Attack")]
    [Tooltip("Attack range (melee).")]
    [SerializeField] private float attackRange = 1.6f;
    public float AttackRange => attackRange;

    [Tooltip("Seconds between attacks.")]
    [SerializeField] private float attackCooldown = 1.5f;
    public float AttackCooldown => attackCooldown;

    [Tooltip("Damage per attack.")]
    [SerializeField] private int attackDamage = 20;
    public int AttackDamage => attackDamage;

    [Header("Behavioral")]
    [Tooltip("Chance (0-1) to investigate noisy stimuli rather than ignore.")]
    [Range(0f, 1f)]
    [SerializeField] private float investigateChance = 0.9f;
    public float InvestigateChance => investigateChance;

    [Tooltip("How close to patrol point before considering it reached (m).")]
    [SerializeField] private float patrolPointTolerance = 0.5f;
    public float PatrolPointTolerance => patrolPointTolerance;

    [Tooltip("Should the enemy roam randomly if no patrol points?")]
    [SerializeField] private bool randomRoam = true;
    public bool RandomRoam => randomRoam;

    [Tooltip("Random roam radius around spawn.")]
    [SerializeField] private float roamRadius = 10f;
    public float RoamRadius => roamRadius;
}
