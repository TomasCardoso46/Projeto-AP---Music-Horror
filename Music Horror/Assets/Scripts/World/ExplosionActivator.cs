using UnityEngine;
using System.Collections.Generic;

public class ExplosionActivator : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private Transform explosionPoint;
    [SerializeField] private float explosionForce = 20f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float upwardsModifier = 1f;

    [Header("Target Layer")]
    [SerializeField] private string explosionLayerName = "Explosion";

    [Header("Extra Settings")]
    [SerializeField] private bool disableAfterExplosion = true;
    [SerializeField] private float randomTorqueForce = 10f;

    private readonly List<Rigidbody> rigidbodies = new();
    private bool hasExploded = false;

    private void Start()
    {
        int explosionLayer = LayerMask.NameToLayer(explosionLayerName);

        if (explosionLayer == -1)
        {
            Debug.LogError($"Layer '{explosionLayerName}' does not exist.");
            return;
        }

        Rigidbody[] allRigidbodies = FindObjectsOfType<Rigidbody>();

        foreach (Rigidbody rb in allRigidbodies)
        {
            if (rb.gameObject.layer != explosionLayer)
                continue;

            rigidbodies.Add(rb);

            rb.isKinematic = true;
            rb.useGravity = false;

            rb.Sleep();
        }
    }

    public void TriggerExplosion()
    {
        if (hasExploded)
            return;

        hasExploded = true;

        foreach (Rigidbody rb in rigidbodies)
        {
            if (rb == null)
                continue;

            rb.isKinematic = false;
            rb.useGravity = true;

            rb.AddExplosionForce(
                explosionForce,
                explosionPoint.position,
                explosionRadius,
                upwardsModifier,
                ForceMode.Impulse
            );

            rb.AddTorque(
                Random.insideUnitSphere * randomTorqueForce,
                ForceMode.Impulse
            );
        }

        if (disableAfterExplosion)
        {
            enabled = false;
        }
    }
}