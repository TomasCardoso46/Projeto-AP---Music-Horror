using UnityEngine;
using UnityEngine.UI;

public class ManualSaveButton : MonoBehaviour
{
    [SerializeField] private GameObject successText;
    [SerializeField] private float showTime = 1.5f;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(DoSave);

        if (successText != null)
            successText.SetActive(false);
    }

    private void DoSave()
    {
        if (SaveManager.Instance == null)
            return;

        SaveManager.Instance.CreateManualSave();

        if (successText != null)
        {
            StopAllCoroutines();
            StartCoroutine(ShowSuccess());
        }
    }

    private System.Collections.IEnumerator ShowSuccess()
    {
        successText.SetActive(true);
        yield return new WaitForSecondsRealtime(showTime);
        successText.SetActive(false);
    }
}