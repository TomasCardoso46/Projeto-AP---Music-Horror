using System;
using UnityEngine;

/// <summary>
/// Simple sound broadcaster. Attach to any object that can make a sound.
/// Call PlaySound() to broadcast a sound event to any listeners (EnemyAI).
/// </summary>
public class SoundEmitter : MonoBehaviour
{
    /// <summary>
    /// Sound event payload
    /// </summary>
    public struct SoundEvent
    {
        public Vector3 position;
        public float radius;       // max hearing radius (falloff handled by listeners)
        public string sourceTag;   // tag of the source GameObject
        public GameObject sourceObject;
    }

    // Event - enemies subscribe to this to react to sounds.
    public static event Action<SoundEvent> OnSoundPlayed;

    [SerializeField] private float baseRadius = 8f;
    [SerializeField]
    [Tooltip("Optional multiplier for certain actions")]
    private float radiusMultiplier = 1f;

    [Header("Optional: tag override (defaults to this.gameObject.tag if empty)")]
    [SerializeField] private string overrideTag = "";

    /// <summary>
    /// Play sound using current transform.position and baseRadius * multiplier.
    /// </summary>
    public void PlaySound(float multiplier = 1f)
    {
        if (OnSoundPlayed == null) return;

        SoundEvent ev = new SoundEvent()
        {
            position = transform.position,
            radius = baseRadius * radiusMultiplier * multiplier,
            sourceTag = string.IsNullOrEmpty(overrideTag) ? gameObject.tag : overrideTag,
            sourceObject = gameObject
        };

        OnSoundPlayed.Invoke(ev);
    }

    // convenience overloads
    public void PlaySoundAtPosition(Vector3 pos, float multiplier = 1f)
    {
        if (OnSoundPlayed == null) return;

        SoundEvent ev = new SoundEvent()
        {
            position = pos,
            radius = baseRadius * radiusMultiplier * multiplier,
            sourceTag = string.IsNullOrEmpty(overrideTag) ? gameObject.tag : overrideTag,
            sourceObject = gameObject
        };

        OnSoundPlayed.Invoke(ev);
    }

    // Optional: editor gizmo for sound radius
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow * 0.6f;
        Gizmos.DrawWireSphere(transform.position, baseRadius * radiusMultiplier);
    }
}
