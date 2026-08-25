using UnityEngine;

public class PlayAnimationToEnd : StateMachineBehaviour
{
    [Tooltip("If enabled, the state cannot be exited until the animation reaches its end.")]
    [SerializeField] private bool playToEnd = true;

    private bool animationFinished;

    public override void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        animationFinished = false;
    }

    public override void OnStateUpdate(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        if (!playToEnd || animationFinished)
            return;

        // Animation has reached the end.
        if (stateInfo.normalizedTime >= 1f)
        {
            animationFinished = true;
        }
        else
        {
            // Prevent transitions from interrupting this state.
            animator.CrossFade(
                stateInfo.fullPathHash,
                0f,
                layerIndex,
                stateInfo.normalizedTime
            );
        }
    }
}