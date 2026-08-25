using UnityEngine;

public class SmokeScreenLifeTime : MonoBehaviour
{
    [Header("Smoke")]
    [SerializeField] private ParticleSystem smokeParticleSystem;

    [Header("Dissipation")]
    [SerializeField] private float smokeDuration = 5f;
    [SerializeField] private AnimationCurve dissipationCurve =
        AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private ParticleSystem.EmissionModule emission;
    private float startRate;
    private float timer;

    private void Awake()
    {
        if (smokeParticleSystem == null)
            smokeParticleSystem = GetComponent<ParticleSystem>();

        if (smokeParticleSystem == null)
        {
            Debug.LogError($"{name}: No Particle System assigned.");
            enabled = false;
            return;
        }

        emission = smokeParticleSystem.emission;
        startRate = emission.rateOverTime.constant;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float progress = Mathf.Clamp01(timer / smokeDuration);

        float multiplier = dissipationCurve.Evaluate(progress);

        emission.rateOverTime = startRate * multiplier;

        if (progress >= 1f)
        {
            emission.rateOverTime = 0f;

            if (!smokeParticleSystem.IsAlive(true))
            {
                Destroy(gameObject);
            }
        }
    }
}