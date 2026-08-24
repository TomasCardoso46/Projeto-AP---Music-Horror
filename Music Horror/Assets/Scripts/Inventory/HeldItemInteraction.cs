using UnityEngine;

public class HeldItemInteraction : MonoBehaviour
{
    private CanPickUp originalObject;
    private GameObject heldObject;

    private bool canTake;
    private Transform returnPosition;

    private FirstPersonRigidbodyController playerController;

    private bool initialized = false;

    public void Setup(
        CanPickUp original,
        GameObject held,
        bool takeable,
        Transform returnTransform)
    {
        originalObject = original;
        heldObject = held;

        canTake = takeable;
        returnPosition = returnTransform;

        playerController =
            FindFirstObjectByType<
                FirstPersonRigidbodyController
            >();

        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
            return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            ReleaseItem();
        }
    }

    private void ReleaseItem()
    {
        if (canTake)
        {
            ReleaseTakeableItem();
        }
        else
        {
            ReleaseTemporaryItem();
        }
    }

    private void ReleaseTakeableItem()
    {
        if (originalObject == null)
        {
            Destroy(heldObject);
            return;
        }

        if (returnPosition == null)
        {
            Debug.LogWarning(
                "No return position assigned for " +
                originalObject.gameObject.name
            );

            return;
        }

        originalObject.transform.position =
            returnPosition.position;

        originalObject.transform.rotation =
            returnPosition.rotation;

        originalObject.gameObject.SetActive(true);

        Destroy(heldObject);

        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.RemoveItem(
                originalObject.ItemID
            );
        }
    }

    private void ReleaseTemporaryItem()
    {
        if (heldObject != null)
        {
            Destroy(heldObject);
        }

        if (originalObject != null)
        {
            originalObject.gameObject.SetActive(true);
        }

        EnablePlayer();
    }

    public void DisablePlayer()
    {
        if (playerController == null)
        {
            playerController =
                FindFirstObjectByType<
                    FirstPersonRigidbodyController
                >();
        }

        if (playerController != null)
        {
            playerController.LockPlayer();
        }
    }

    private void EnablePlayer()
    {
        if (playerController == null)
        {
            playerController =
                FindFirstObjectByType<
                    FirstPersonRigidbodyController
                >();
        }

        if (playerController != null)
        {
            playerController.UnlockPlayer();
            playerController.HardResetCameraMotion();
        }
    }
}