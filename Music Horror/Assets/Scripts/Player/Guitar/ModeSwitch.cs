using System.Linq.Expressions;
using UnityEngine;

public class ModeSwitch : MonoBehaviour
{
    [Header("Animation (Deterministic)")]
    [SerializeField] private AnimationClip animationClip;
    [SerializeField] private float playbackSpeed = 1f;

    private float currentTime;
    private float clipLength;

    private bool isPlaying;
    private int direction = 1;

    private bool hasCompletedForward;

    [Header("Sphere Switch (GameObject Toggle)")]
    [SerializeField] private GameObject objectA;
    [SerializeField] private GameObject objectB;
    [SerializeField] private GameObject objectC;

    private bool isUsingA = true;

    private void Awake()
    {
        if (animationClip != null)
            clipLength = animationClip.length;
    }

    private void Update()
    {
        if (!isPlaying || animationClip == null) return;

        currentTime += direction * playbackSpeed * Time.deltaTime;
        currentTime = Mathf.Clamp(currentTime, 0f, clipLength);

        animationClip.SampleAnimation(gameObject, currentTime);

        if (direction == 1 && currentTime >= clipLength)
        {
            if (!hasCompletedForward)
            {
                hasCompletedForward = true;
                OnForwardComplete(1);
            }

            isPlaying = false;
        }

        if (direction == -1 && currentTime <= 0f)
        {
            isPlaying = false;
        }
    }

    public void PlayForward()
    {
        direction = 1;
        isPlaying = true;
        hasCompletedForward = false;

        if (currentTime >= clipLength)
            currentTime = 0f;
    }

    public void PlayReverse()
    {
        direction = -1;
        isPlaying = true;

        if (currentTime <= 0f)
            currentTime = clipLength;
    }

    public void PlayAndSwitch(int i)
    {
        PlayForward();
        SphereSwitcher(i);
    }

    private void OnForwardComplete(int i)
    {
        SphereSwitcher(i);
        PlayReverse();
    }

    public void SphereSwitcher(int mode)
    {
        switch(mode)
        {
            case 1: 
                objectA.SetActive(true);
                objectB.SetActive(false);
                objectC.SetActive(false);
                break;

            case 2:
                objectA.SetActive(false);
                objectB.SetActive(true);
                objectC.SetActive(false);
                break;

            case 3:
                objectA.SetActive(false);
                objectB.SetActive(false);
                objectC.SetActive(true);
                break;

        }


        //objectA.SetActive(isUsingA);
        //objectB.SetActive(!isUsingA);
    }

    public void ResetAnimation()
    {
        currentTime = 0f;
        isPlaying = false;
        hasCompletedForward = false;

        if (animationClip != null)
            animationClip.SampleAnimation(gameObject, 0f);
    }
}