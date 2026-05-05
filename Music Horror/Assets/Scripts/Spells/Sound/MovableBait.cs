using UnityEngine;

public class MovableBait : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;

    private bool hasTriggered = false;


    private void Update()
    {
        if (!hasTriggered)
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (hasTriggered) return;

        GameObject collidedObject = collision.gameObject;

        // Try to find SoundBait on the collided object
        SoundBait soundBait = collidedObject.GetComponent<SoundBait>();

        if (soundBait != null)
        {
            hasTriggered = true;

            // Enable the SoundBait script
            soundBait.enabled = true;
        }

        // Destroy projectile regardless of result
        Destroy(gameObject);
    }
}