using UnityEngine;

public class InfoEnabler : MonoBehaviour
{
    [SerializeField] private GameObject objectToToggle;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            objectToToggle.SetActive(!objectToToggle.activeSelf);
        }
    }
}