using UnityEngine;

public class InfoEnabler : MonoBehaviour, IInteractable
{
    [Header("Info")]
    [SerializeField] private GameObject objectToToggle;

    [Header("Blur")]
    [SerializeField] private UIBlurException blur;

    public void Interact()
    {
        FirstPersonRigidbodyController playerController =
            FindFirstObjectByType<FirstPersonRigidbodyController>();

        bool shouldEnable = !objectToToggle.activeSelf;

        objectToToggle.SetActive(shouldEnable);

        if (shouldEnable)
        {
            if (blur != null)
            {
                blur.EnableBlur();
            }

            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }
        else
        {
            if (blur != null)
            {
                blur.DisableBlur();
            }

            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }
    }
}