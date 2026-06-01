using UnityEngine;

public class CrosshairEnable : MonoBehaviour
{
    [SerializeField] private GameObject crosshair;

    public void ShowCrosshair()
    {
        crosshair?.SetActive(true);
    }

    public void HideCrosshair()
    {
        crosshair?.SetActive(false);
    }
}