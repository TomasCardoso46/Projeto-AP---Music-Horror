using UnityEngine;

public class StickyLight : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private Vector3 speed = new Vector3(0, 0, 20f);
    [SerializeField] private bool followTarget = false;   // If true, sticks to and follows moving objects
    [SerializeField] private float shrinkingSpeed = 0.5f; // Units per second the projectile shrinks after sticking

    private Rigidbody rb;
    private Vector3 impactScale;
    private Vector3 originalScale;
    private bool isStuck = false;

    private Light pointLight;
    private float initialLightIntensity;
    private float initialLightRange;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Store the initial scale before impact
        originalScale = transform.localScale;

        // Find a child Point Light automatically
        pointLight = GetComponentInChildren<Light>();
        if (pointLight != null)
        {
            initialLightIntensity = pointLight.intensity;
            initialLightRange = pointLight.range;
        }

        // Apply an instantaneous push (frame-rate independent)
        rb.AddRelativeForce(speed, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isStuck) return; // Prevent multiple triggers
        isStuck = true;

        // Record the current scale at the moment of impact
        impactScale = transform.localScale;

        // Stop all physics
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Move to the contact point and orient toward the surface
        ContactPoint contact = collision.contacts[0];
        transform.position = contact.point;
        transform.rotation = Quaternion.LookRotation(contact.normal);

        // Optional: stick to moving objects using a scale-neutral anchor
        if (followTarget)
        {
            GameObject anchor = new GameObject("StickyAnchor");
            anchor.transform.position = contact.point;
            anchor.transform.rotation = transform.rotation;
            anchor.transform.SetParent(collision.transform, true);
            transform.SetParent(anchor.transform, true);
        }
        else
        {
            transform.SetParent(null);
        }

        // Restore the pre-impact scale
        transform.localScale = impactScale;
    }

    private void LateUpdate()
    {
        if (!isStuck) return;

        // Maintain same world scale even if parent scales
        if (transform.parent != null)
        {
            Vector3 parentScale = transform.parent.lossyScale;
            transform.localScale = new Vector3(
                impactScale.x / parentScale.x,
                impactScale.y / parentScale.y,
                impactScale.z / parentScale.z
            );
        }

        // Gradually shrink
        if (shrinkingSpeed > 0f)
        {
            float shrinkAmount = shrinkingSpeed * Time.deltaTime;
            impactScale -= Vector3.one * shrinkAmount;

            // Destroy when very small
            if (impactScale.x <= 0f || impactScale.y <= 0f || impactScale.z <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            transform.localScale = impactScale;

            // --- LIGHT FADE SECTION ---
            if (pointLight != null)
            {
                // Normalize light fade relative to the projectile’s original scale
                float normalizedScale = Mathf.Clamp01(impactScale.x / originalScale.x);

                // Smoothly fade out the light’s intensity and range
                pointLight.intensity = Mathf.Lerp(0f, initialLightIntensity, normalizedScale);
                pointLight.range = Mathf.Lerp(0f, initialLightRange, normalizedScale);
            }
        }
        else if (pointLight != null)
        {
            // If shrinking stops early, continue fading light smoothly to zero
            pointLight.intensity = Mathf.MoveTowards(pointLight.intensity, 0f, Time.deltaTime * initialLightIntensity);
            pointLight.range = Mathf.MoveTowards(pointLight.range, 0f, Time.deltaTime * initialLightRange);
        }
    }
}
