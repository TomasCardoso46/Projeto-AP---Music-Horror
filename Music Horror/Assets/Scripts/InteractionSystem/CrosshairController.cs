using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [SerializeField] private Image crosshairImage;

    [Header("Colors")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color interactColor = Color.green;

    public void SetInteractState(bool canInteract)
    {
        crosshairImage.color = canInteract ? interactColor : defaultColor;
    }
}