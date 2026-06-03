using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [SerializeField] private Image crosshairImage;

    [Header("Colors")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color interactColor = Color.green;

    [Header("Scale")]
    [SerializeField] private float defaultScale = 1f;
    [SerializeField] private float interactScale = 1.3f;
    [SerializeField] private float scaleLerpSpeed = 10f;

    private RectTransform rectTransform;
    private float targetScale;

    private void Awake()
    {
        rectTransform = crosshairImage.GetComponent<RectTransform>();
        targetScale = defaultScale;
    }

    private void Update()
    {
        float currentScale = rectTransform.localScale.x;
        float newScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * scaleLerpSpeed);

        rectTransform.localScale = new Vector3(newScale, newScale, 1f);
    }

    public void SetInteractState(bool canInteract)
    {
        crosshairImage.color = canInteract ? interactColor : defaultColor;
        targetScale = canInteract ? interactScale : defaultScale;
    }
}