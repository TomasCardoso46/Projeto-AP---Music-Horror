using UnityEngine;

public class DeleteEnemy : MonoBehaviour
{
    public GameObject objectToDelete;

    void Update()
    {
        
        if (Input.inputString.Contains("ç") || Input.inputString.Contains("Ç"))
        {
            if (objectToDelete != null)
            {
                Destroy(objectToDelete);
            }
        }
    }
}
