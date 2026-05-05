using UnityEngine;

public class DetectionTrigger : MonoBehaviour
{
    private EnemySonar ability;
    private LayerMask playerLayer;

    public void Initialize(EnemySonar abilityRef, LayerMask layer)
    {
        ability = abilityRef;
        playerLayer = layer;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            ability.SetPlayerInside(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            ability.SetPlayerInside(false);
        }
    }
}
