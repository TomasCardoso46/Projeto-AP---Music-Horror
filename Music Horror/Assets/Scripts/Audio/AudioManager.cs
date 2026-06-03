using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Listener")]
    [SerializeField] private Transform listener;

    [Header("Occlusion Layers")]
    [SerializeField] private LayerMask occlusionLayers;

    [Header("Block List (NO MUFFLE AUTO-ADD)")]
    [SerializeField] private List<GameObject> blockedObjects = new();

    [Header("Performance")]
    [SerializeField] private float updateInterval = 0.15f;
    [SerializeField] private int sourcesPerUpdate = 4;
    [SerializeField] private float rescanInterval = 5f;
    [SerializeField] private float sampleRadius = 0.35f;

    [Header("GLOBAL MUFFLE SETTINGS")]
    [Range(0f, 1f)]
    [SerializeField] private float minVolumeMultiplier = 0.35f;

    [Range(0f, 1f)]
    [SerializeField] private float maxOcclusionStrength = 1f;

    [SerializeField] private float clearCutoffFrequency = 22000f;
    [SerializeField] private float muffledCutoffFrequency = 800f;

    [SerializeField] private AnimationCurve occlusionCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    private readonly List<Muffle> sources = new();

    private float updateTimer;
    private float rescanTimer;
    private int currentIndex;

    private readonly Vector3[] rayOffsets =
    {
        Vector3.zero,
        Vector3.up,
        Vector3.down,
        Vector3.right,
        Vector3.left
    };

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (listener == null)
        {
            AudioListener audioListener =
                FindFirstObjectByType<AudioListener>();

            if (audioListener != null)
                listener = audioListener.transform;
        }

        RefreshSources();
    }

    private void Update()
    {
        if (listener == null)
            return;

        updateTimer += Time.deltaTime;
        rescanTimer += Time.deltaTime;

        if (rescanTimer >= rescanInterval)
        {
            rescanTimer = 0f;
            RefreshSources();
        }

        if (updateTimer < updateInterval)
            return;

        updateTimer = 0f;

        int processed = 0;

        while (processed < sourcesPerUpdate && sources.Count > 0)
        {
            if (currentIndex >= sources.Count)
                currentIndex = 0;

            Muffle source = sources[currentIndex];
            currentIndex++;

            if (source == null)
                continue;

            UpdateOcclusion(source);
            processed++;
        }
    }

    private bool IsBlocked(GameObject go)
    {
        for (int i = 0; i < blockedObjects.Count; i++)
        {
            if (blockedObjects[i] == go)
                return true;
        }
        return false;
    }

    private void RefreshSources()
    {
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        HashSet<GameObject> processedObjects = new();

        sources.Clear();

        foreach (AudioSource audioSource in allAudioSources)
        {
            if (audioSource == null)
                continue;

            GameObject go = audioSource.gameObject;

            if (!processedObjects.Add(go))
                continue;

            // -------------------------
            // BLOCK LIST CHECK
            // -------------------------
            if (IsBlocked(go))
                continue;

            Muffle muffle = go.GetComponent<Muffle>();

            if (muffle == null)
                muffle = go.AddComponent<Muffle>();

            sources.Add(muffle);
        }
    }

    private void UpdateOcclusion(Muffle source)
    {
        if (listener == null)
            return;

        Vector3 sourcePos = source.transform.position;
        Vector3 listenerPos = listener.position;

        int blockedRays = 0;

        for (int i = 0; i < rayOffsets.Length; i++)
        {
            Vector3 startPos = sourcePos + (rayOffsets[i] * sampleRadius);

            Vector3 dir = listenerPos - startPos;
            float dist = dir.magnitude;

            if (dist <= 0.01f)
                continue;

            if (Physics.Raycast(
                    startPos,
                    dir.normalized,
                    dist,
                    occlusionLayers,
                    QueryTriggerInteraction.Ignore))
            {
                blockedRays++;
            }
        }

        float rayOcclusion = blockedRays / (float)rayOffsets.Length;

        int wallCount = Physics.RaycastAll(
            sourcePos,
            (listenerPos - sourcePos).normalized,
            Vector3.Distance(sourcePos, listenerPos),
            occlusionLayers,
            QueryTriggerInteraction.Ignore).Length;

        float wallOcclusion = Mathf.Clamp01(wallCount / 3f);

        float finalOcclusion = Mathf.Clamp01(
            (rayOcclusion * 0.7f) +
            (wallOcclusion * 0.3f)
        );

        finalOcclusion = occlusionCurve.Evaluate(finalOcclusion);
        finalOcclusion *= maxOcclusionStrength;

        source.ApplyMuffleSettings(
            finalOcclusion,
            minVolumeMultiplier,
            clearCutoffFrequency,
            muffledCutoffFrequency
        );
    }
}