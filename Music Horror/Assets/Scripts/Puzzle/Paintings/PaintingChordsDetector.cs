using UnityEngine;

public class PaintingChordsDetector : MonoBehaviour
{
    [SerializeField] private GameObject objectToActivate;
    [SerializeField] private GameObject objectToDeactivate;
    [SerializeField] private KeyCode chord;
    [SerializeField] private int queuePosition;
    [SerializeField] private bool isLast;
    [SerializeField] private Color color;
    private bool playerInside = false;
    private bool correctOrder = false;
    void Start()
    {
        
    }

    void Update()
    {
        if (playerInside && Input.GetKeyDown(chord))
        {
            if (correctOrder)
            {
                objectToActivate.SetActive(true);
                if (isLast)
                {
                    objectToDeactivate.SetActive(false);
                }
            }
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInside = false;
    }
}
