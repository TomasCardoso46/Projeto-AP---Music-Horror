using UnityEngine;

public class FakeDoors : MonoBehaviour
{
    [Header("Target with Animation Component")]
    [SerializeField] private Animation targetAnimation;

    [Header("Animation Clip Name")]
    [SerializeField] private string animationName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("FakeDoors");
            if (targetAnimation != null && targetAnimation.GetClip(animationName) != null)
            {
                targetAnimation.Play(animationName);
                Destroy(this.gameObject.GetComponent<FakeDoors>());
            }
        }
    }
}
