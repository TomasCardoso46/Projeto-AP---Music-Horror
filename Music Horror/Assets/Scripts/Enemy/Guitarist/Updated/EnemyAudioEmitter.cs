using System;
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

    public Action<SoundLevel> OnSoundEmitted;

    public void EmitSound(SoundLevel level, float duration = 0.2f)
    {
        OnSoundEmitted?.Invoke(level);
        StartCoroutine(EmitRoutine(level, duration));
    }

  
    public void StartHighSound()
    {
        highSound = true;
        OnSoundEmitted?.Invoke(SoundLevel.High);
    }

    
    public void StopSound()
    {
        lowSound = false;
        normalSound = false;
        highSound = false;
    }

    private System.Collections.IEnumerator EmitRoutine(SoundLevel level, float duration)
    {
        switch (level)
        {
            case SoundLevel.Low:
                lowSound = true;
                break;

            case SoundLevel.Normal:
                normalSound = true;
                break;

            case SoundLevel.High:
                highSound = true;
                break;
        }

        yield return new WaitForSeconds(duration);

        switch (level)
        {
            case SoundLevel.Low:
                lowSound = false;
                break;

            case SoundLevel.Normal:
                normalSound = false;
                break;

            case SoundLevel.High:
                highSound = false;
                break;
        }
    }

    public enum SoundLevel
    {
        Low,
        Normal,
        High
    }
}