using System.Collections;
using UnityEngine;

public class EnemyAudioEmitter : MonoBehaviour
{
    [Header("Sound States")]
    [SerializeField] private bool lowSound = false;
    [SerializeField] private bool normalSound = false;
    [SerializeField] private bool highSound = false;

    public bool IsEmittingLow => lowSound;
    public bool IsEmittingNormal => normalSound;
    public bool IsEmittingHigh => highSound;

    /// <summary>
    /// Emits a sound of the specified type.
    /// </summary>
    public void EmitSound(SoundLevel level, float duration = 0.2f)
    {
        StartCoroutine(EmitRoutine(level, duration));
    }

    private IEnumerator EmitRoutine(SoundLevel level, float duration)
    {
        switch (level)
        {
            case SoundLevel.Low: lowSound = true; break;
            case SoundLevel.Normal: normalSound = true; break;
            case SoundLevel.High: highSound = true; break;
        }

        yield return new WaitForSeconds(duration);

        switch (level)
        {
            case SoundLevel.Low: lowSound = false; break;
            case SoundLevel.Normal: normalSound = false; break;
            case SoundLevel.High: highSound = false; break;
        }
    }

    public enum SoundLevel
    {
        Low,
        Normal,
        High
    }
}
