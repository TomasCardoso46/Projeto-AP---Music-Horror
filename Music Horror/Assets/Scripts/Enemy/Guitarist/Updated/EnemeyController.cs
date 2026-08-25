using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyPerception))]
[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyAttack))]
public class EnemyController : MonoBehaviour, IEnemy
{
    public enum State
    {
        Idle,
        Patrol,
        Investigate,
        Chase,
        Attack,
        Dead
    }

    [SerializeField] private EnemySettings settings;
    [SerializeField] private Animator animator;

    [SerializeField] private EnemyPerception perception;
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private EnemyAttack attack;
    [SerializeField] private EnemyHealth health;

    public State currentState = State.Idle;

    private Transform target;
    private Vector3 lastKnownPosition;
    private float timeSinceSeen;

    private bool movementLocked = false;

    [Header("Chase Audio")]
    [SerializeField] private AudioSource chaseAudioSource;
    [SerializeField] private AudioClip chaseClip;
    [SerializeField] private float chaseFadeOutTime = 1.2f;
    [SerializeField][Range(0f, 1f)] private float investigateStartVolume = 1f;
    [SerializeField] private float chaseLoseTrackDelay = 2f;

    private Coroutine fadeOutCoroutine;
    private Coroutine loseTrackCoroutine;
    private bool chaseAudioPlaying = false;

    [Header("Enemy Chase Sounds")]
    [SerializeField] private AudioSource enemyChaseSounds;
    [SerializeField] private AudioClip[] enemyChaseSoundClips;
    [SerializeField] private float enemyChaseSoundVolume = 1f;
    [SerializeField] private float perceptionSoundCooldown = 2f;

    private Coroutine perceptionSoundCoroutine;
    private bool perceptionSoundPlaying = false;
    private bool perceptionSoundQueued = false;

    private AudioClip lastPlayedPerceptionSound;
    private float lastPerceptionSoundTime = -Mathf.Infinity;

    [Header("Investigate")]
    [SerializeField] private float minInvestigateTime = 3f;
    private float investigateTimer = 0f;

    [Header("Sonar")]
    [SerializeField] private EnemySonar sonar;
    [SerializeField] private float sonarCooldown = 5f;
    private float sonarTimer = 0f;

    [Header("Hide Spot Destruction")]
    [SerializeField] private LayerMask hideSpotLayer;
    [SerializeField] private float destroyRadius = 10f;
    [SerializeField] private float destroyAttackDelay = 0.5f;

    private bool destroyingHideSpot = false;
    private Transform player;

    private void Start()
    {
        gameObject.SetActive(false);
    }

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

        if (enemyChaseSounds != null)
        {
            enemyChaseSounds.loop = false;
            enemyChaseSounds.playOnAwake = false;
        }
    }

    private void Update()
    {
        if (movementLocked)
            return;

        if (Cheats.EnemyDisabled)
        {
            StopAllAudio();

            target = null;
            lastKnownPosition = Vector3.zero;
            timeSinceSeen = 0f;
            investigateTimer = 0f;

            if (currentState != State.Patrol)
                SetState(State.Patrol);

            if (perception != null)
                perception.enabled = false;

            if (attack != null)
                attack.enabled = false;

            movement.Patrol();

            if (animator != null)
                animator.SetInteger("State", (int)State.Patrol);

            sonarTimer = 0f;

            return;
        }

        if (perception != null)
            perception.enabled = true;

        if (attack != null)
            attack.enabled = true;

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
            investigateTimer = 0f;

            CancelLoseTrackTimer();

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

            if (currentState == State.Chase)
                StartLoseTrackTimer();

            if (lastKnownPosition != Vector3.zero &&
                timeSinceSeen <= settings.MemoryTime)
            {
                SetState(State.Investigate);
            }
            else if (currentState == State.Investigate &&
                     investigateTimer < minInvestigateTime)
            {
                SetState(State.Investigate);
            }
            else
            {
                SetState(State.Patrol);
                investigateTimer = 0f;
            }
        }

        ExecuteState();

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
                if (!destroyingHideSpot &&
                    lastKnownPosition != Vector3.zero)
                {
                    movement.MoveTo(lastKnownPosition);
                }

                break;

            case State.Chase:
                if (target != null)
                    movement.Chase(target);

                break;

            case State.Attack:
                if (target != null)
                {
                    Vector3 lookAt = new Vector3(
                        target.position.x,
                        transform.position.y,
                        target.position.z
                    );

                    transform.LookAt(lookAt);

                    attack.TryAttack(target);
                }
                else
                {
                    SetState(State.Patrol);
                }

                break;

            case State.Dead:
                movement.DisableMovement();
                attack.DisableAttack();
                StopAllAudio();
                break;
        }
    }

    public void SetState(State newState)
    {
        if (currentState == newState)
            return;

        State previousState = currentState;

        if (newState == State.Investigate)
        {
            PlayInvestigateAudio();
        }
        else if (previousState == State.Investigate &&
                 newState != State.Investigate)
        {
            FadeOutInvestigateAudio();
        }

        if (newState == State.Chase)
        {
            PlayChaseAudio();
        }

        if (previousState == State.Chase &&
            newState != State.Chase &&
            newState != State.Attack)
        {
            StartLoseTrackTimer();
        }

        if (newState == State.Patrol)
            sonarTimer = 0f;

        currentState = newState;
    }

    private void PlayInvestigateAudio()
    {
        CancelLoseTrackTimer();

        if (chaseAudioSource != null &&
            chaseClip != null)
        {
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
                fadeOutCoroutine = null;
            }

            chaseAudioSource.clip = chaseClip;
            chaseAudioSource.volume = investigateStartVolume;
            chaseAudioSource.loop = true;

            if (!chaseAudioSource.isPlaying)
                chaseAudioSource.Play();

            chaseAudioPlaying = true;
        }
    }

    private void FadeOutInvestigateAudio()
    {
        if (!chaseAudioPlaying ||
            chaseAudioSource == null)
            return;

        if (fadeOutCoroutine != null)
            StopCoroutine(fadeOutCoroutine);

        fadeOutCoroutine = StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeOutCoroutine()
    {
        float startVolume = chaseAudioSource.volume;
        float t = 0f;

        while (t < chaseFadeOutTime)
        {
            t += Time.deltaTime;

            chaseAudioSource.volume = Mathf.Lerp(
                startVolume,
                0f,
                t / chaseFadeOutTime
            );

            yield return null;
        }

        chaseAudioSource.Stop();
        chaseAudioSource.volume = investigateStartVolume;

        chaseAudioPlaying = false;
        fadeOutCoroutine = null;
    }

    private void PlayChaseAudio()
    {
        CancelLoseTrackTimer();

        if (chaseAudioSource != null &&
            chaseClip != null)
        {
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
                fadeOutCoroutine = null;
            }

            chaseAudioSource.clip = chaseClip;
            chaseAudioSource.volume = investigateStartVolume;
            chaseAudioSource.loop = true;

            if (!chaseAudioSource.isPlaying)
                chaseAudioSource.Play();

            chaseAudioPlaying = true;
        }
    }

    public void PerceivedSomething()
    {
        if (Cheats.EnemyDisabled)
            return;

        if (enemyChaseSounds == null)
            return;

        if (enemyChaseSoundClips == null ||
            enemyChaseSoundClips.Length == 0)
            return;

        if (perceptionSoundPlaying)
        {
            perceptionSoundQueued = true;
            return;
        }

        float timeSinceLastSound =
            Time.time - lastPerceptionSoundTime;

        if (timeSinceLastSound < perceptionSoundCooldown)
        {
            perceptionSoundQueued = true;

            if (perceptionSoundCoroutine == null)
            {
                perceptionSoundCoroutine =
                    StartCoroutine(WaitForPerceptionSoundCooldown());
            }

            return;
        }

        PlayPerceptionSound();
    }

    private IEnumerator WaitForPerceptionSoundCooldown()
    {
        float remaining =
            perceptionSoundCooldown -
            (Time.time - lastPerceptionSoundTime);

        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        perceptionSoundCoroutine = null;

        if (perceptionSoundQueued)
        {
            perceptionSoundQueued = false;

            if (!perceptionSoundPlaying)
                PlayPerceptionSound();
        }
    }

    private void PlayPerceptionSound()
    {
        AudioClip clip = GetRandomPerceptionSound();

        if (clip == null)
            return;

        if (perceptionSoundCoroutine != null)
        {
            StopCoroutine(perceptionSoundCoroutine);
            perceptionSoundCoroutine = null;
        }

        perceptionSoundCoroutine =
            StartCoroutine(PerceptionSoundCoroutine(clip));
    }

    private IEnumerator PerceptionSoundCoroutine(AudioClip clip)
    {
        perceptionSoundPlaying = true;
        perceptionSoundQueued = false;

        lastPlayedPerceptionSound = clip;
        lastPerceptionSoundTime = Time.time;

        enemyChaseSounds.volume = enemyChaseSoundVolume;
        enemyChaseSounds.loop = false;
        enemyChaseSounds.clip = clip;

        enemyChaseSounds.Play();

        while (enemyChaseSounds.isPlaying)
            yield return null;

        enemyChaseSounds.clip = null;

        perceptionSoundPlaying = false;
        perceptionSoundCoroutine = null;

        if (perceptionSoundQueued)
        {
            float remaining =
                perceptionSoundCooldown -
                (Time.time - lastPerceptionSoundTime);

            if (remaining > 0f)
            {
                perceptionSoundCoroutine =
                    StartCoroutine(WaitForPerceptionSoundCooldown());
            }
            else
            {
                perceptionSoundQueued = false;
                PlayPerceptionSound();
            }
        }
    }

    private AudioClip GetRandomPerceptionSound()
    {
        if (enemyChaseSoundClips == null ||
            enemyChaseSoundClips.Length == 0)
            return null;

        List<AudioClip> validClips =
            new List<AudioClip>();

        foreach (AudioClip clip in enemyChaseSoundClips)
        {
            if (clip != null &&
                clip != lastPlayedPerceptionSound)
            {
                validClips.Add(clip);
            }
        }

        if (validClips.Count == 0)
        {
            foreach (AudioClip clip in enemyChaseSoundClips)
            {
                if (clip != null)
                    validClips.Add(clip);
            }
        }

        if (validClips.Count == 0)
            return null;

        return validClips[
            Random.Range(0, validClips.Count)
        ];
    }

    private void StopPerceptionSounds()
    {
        perceptionSoundQueued = false;
        perceptionSoundPlaying = false;
        lastPlayedPerceptionSound = null;
        lastPerceptionSoundTime = -Mathf.Infinity;

        if (perceptionSoundCoroutine != null)
        {
            StopCoroutine(perceptionSoundCoroutine);
            perceptionSoundCoroutine = null;
        }

        if (enemyChaseSounds != null)
        {
            enemyChaseSounds.Stop();
            enemyChaseSounds.clip = null;
        }
    }

    private void StartLoseTrackTimer()
    {
        if (!chaseAudioPlaying)
            return;

        if (loseTrackCoroutine != null)
            return;

        loseTrackCoroutine =
            StartCoroutine(LoseTrackTimerCoroutine());
    }

    private IEnumerator LoseTrackTimerCoroutine()
    {
        float timer = 0f;

        while (timer < chaseLoseTrackDelay)
        {
            if (perception != null &&
                perception.HasTarget)
            {
                loseTrackCoroutine = null;
                yield break;
            }

            if (currentState == State.Chase)
            {
                loseTrackCoroutine = null;
                yield break;
            }

            timer += Time.deltaTime;

            yield return null;
        }

        FadeOutAllChaseAudio();

        loseTrackCoroutine = null;
    }

    private void CancelLoseTrackTimer()
    {
        if (loseTrackCoroutine != null)
        {
            StopCoroutine(loseTrackCoroutine);
            loseTrackCoroutine = null;
        }
    }

    private void FadeOutAllChaseAudio()
    {
        if (chaseAudioPlaying &&
            chaseAudioSource != null)
        {
            if (fadeOutCoroutine != null)
                StopCoroutine(fadeOutCoroutine);

            fadeOutCoroutine =
                StartCoroutine(FadeOutCoroutine());
        }
    }

    private void StopAllAudio()
    {
        CancelLoseTrackTimer();

        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }

        if (chaseAudioSource != null)
        {
            chaseAudioSource.Stop();
            chaseAudioSource.volume = investigateStartVolume;
        }

        chaseAudioPlaying = false;

        StopPerceptionSounds();
    }

    public void SetLastKnownPosition(Vector3 pos)
    {
        lastKnownPosition = pos;
    }

    public void ResetStateAfterLoad()
    {
        StopAllAudio();
        currentState = State.Patrol;
    }

    public void AlertToPosition(Vector3 worldPos)
    {
        if (Cheats.EnemyDisabled)
            return;

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
        if (Cheats.EnemyDisabled)
            return;

        target = targetTransform;
        lastKnownPosition = targetTransform.position;
        timeSinceSeen = 0f;
        investigateTimer = 0f;

        SetState(State.Chase);
    }

    public void TakeDamage(int amount, Vector3 hitPoint)
    {
        if (Cheats.EnemyDisabled)
            return;

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

    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;
    }

    public bool IsAlive =>
        health == null || health.IsAlive;
}