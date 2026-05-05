using UnityEngine;

public class TriggerAfterDoors : MonoBehaviour
{
    [Header("Layer")]
    [SerializeField] private string playerLayerName = "Player";

    [Header("Objects")]
    [SerializeField] private GameObject[] objectsToDeactivate;
    [SerializeField] private GameObject[] objectsToActivate;

    private int playerLayer;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer(playerLayerName);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != playerLayer)
            return;

        // Deactivate objects
        foreach (GameObject obj in objectsToDeactivate)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // Activate objects
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }
}
