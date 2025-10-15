using UnityEngine;

public class StickyLight : MonoBehaviour
{
    [SerializeField] private Vector3 speed;
    [SerializeField] private float time;
    private Rigidbody rb;
    private Transform newParent;
    private Vector3 lightScale;

    // Update is called once per frame

    private void Start()
    {
        this.rb = GetComponent<Rigidbody>();
        this.rb.AddRelativeForce(speed);
    }
    private void OnCollisionEnter(Collision collision)
    {
        this.rb.isKinematic = true;
    }
}
