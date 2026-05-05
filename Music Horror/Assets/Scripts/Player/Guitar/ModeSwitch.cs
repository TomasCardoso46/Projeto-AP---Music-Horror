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
                OnForwardComplete();
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

    public void PlayAndSwitch()
    {
        PlayForward();
        SphereSwitcher();
    }

    private void OnForwardComplete()
    {
        SphereSwitcher();
        PlayReverse();
    }

    public void SphereSwitcher()
    {
        if (objectA == null || objectB == null) return;

        isUsingA = !isUsingA;

        objectA.SetActive(isUsingA);
        objectB.SetActive(!isUsingA);
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