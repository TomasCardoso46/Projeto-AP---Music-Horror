using UnityEngine;

public class KillPlayer : MonoBehaviour
{
    [SerializeField] private Jumpscare jumpscare;
    [SerializeField] private GameObject objectToDestroy;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<FirstPersonRigidbodyController>()) return;

        jumpscare?.TriggerJumpscare();
        if (objectToDestroy) Destroy(objectToDestroy);
    }
}