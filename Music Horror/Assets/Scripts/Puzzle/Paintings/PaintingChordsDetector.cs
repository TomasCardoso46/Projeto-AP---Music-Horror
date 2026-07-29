using UnityEngine;

public class PaintingChordsDetector : MonoBehaviour
{
    [SerializeField] private GameObject objectToActivate;
    [SerializeField] private KeyCode Chord;
    public bool playerInside = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInside && Input.GetKeyDown(Chord))
        {
            objectToActivate.SetActive(true);
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
