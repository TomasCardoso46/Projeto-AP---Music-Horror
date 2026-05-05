using UnityEngine;

public class StickyLight : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private Vector3 speed = new Vector3(0, 0, 20f);
    [SerializeField] private float shrinkingSpeed = 0.5f;

    [Header("Sticking Settings")]
    [SerializeField] private float surfaceOffset = 0f; 

    [Header("Follow Settings")]
    [Tooltip("Tags the projectile will follow after sticking (e.g. 'Enemy')")]
    [SerializeField] private string[] followTags = new string[] { "Enemy" };

    private Rigidbody rb;
    private Collider col;
    private Vector3 impactScale;
    private Vector3 originalScale;
    private bool isStuck = false;

    private Light pointLight;
    private float initialLightIntensity;
    private float initialLightRange;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        originalScale = transform.localScale;

        pointLight = GetComponentInChildren<Light>();
        if (pointLight != null)
        {
            initialLightIntensity = pointLight.intensity;
            initialLightRange = pointLight.range;
        }

        rb.AddRelativeForce(speed, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isStuck) return;
        isStuck = true;

        impactScale = transform.localScale;

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        ContactPoint contact = collision.contacts[0];

        // Determine sticking offset
        float offset = surfaceOffset;
        if (offset == 0f)
        {
            if (col != null)
            {
                Vector3 localForward = transform.InverseTransformDirection(-contact.normal);
                localForward = new Vector3(
                    Mathf.Abs(localForward.x),
                    Mathf.Abs(localForward.y),
                    Mathf.Abs(localForward.z)
                );

                Vector3 extents = col.bounds.extents;
                offset = Mathf.Max(localForward.x * extents.x,
                                   localForward.y * extents.y,
                                   localForward.z * extents.z);
            }
            else offset = 0.1f;
        }

        transform.position = contact.point + contact.normal * offset;
        transform.rotation = Quaternion.LookRotation(contact.normal);

        // ===== AUTO FOLLOW ENEMIES =====
        bool shouldFollow = false;
        foreach (string tag in followTags)
        {
            if (collision.collider.CompareTag(tag))
            {
                shouldFollow = true;
                break;
            }
        }

        if (shouldFollow)
        {
            GameObject anchor = new GameObject("StickyAnchor");
            anchor.transform.position = transform.position;
            anchor.transform.rotation = transform.rotation;
            anchor.transform.SetParent(collision.transform, true);

            transform.SetParent(anchor.transform, true);
        }
        else
        {
            transform.SetParent(null);
        }

        transform.localScale = impactScale;
    }

    private void LateUpdate()
    {
        if (!isStuck) return;

        // Maintain world scale even if parent scales
        if (transform.parent != null)
        {
            Vector3 parentScale = transform.parent.lossyScale;
            transform.localScale = new Vector3(
                impactScale.x / parentScale.x,
                impactScale.y / parentScale.y,
                impactScale.z / parentScale.z
            );
        }

        // Shrink
        if (shrinkingSpeed > 0f)
        {
            float shrinkAmount = shrinkingSpeed * Time.deltaTime;
            impactScale -= Vector3.one * shrinkAmount;

            if (impactScale.x <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            transform.localScale = impactScale;

            // Light fade
            if (pointLight != null)
            {
                float normalized = Mathf.Clamp01(impactScale.x / originalScale.x);

                pointLight.intensity = Mathf.Lerp(0f, initialLightIntensity, normalized);
                pointLight.range = Mathf.Lerp(0f, initialLightRange, normalized);
            }
        }
        else if (pointLight != null)
        {
            pointLight.intensity = Mathf.MoveTowards(pointLight.intensity, 0f, Time.deltaTime * initialLightIntensity);
            pointLight.range = Mathf.MoveTowards(pointLight.range, 0f, Time.deltaTime * initialLightRange);
        }
    }
}
