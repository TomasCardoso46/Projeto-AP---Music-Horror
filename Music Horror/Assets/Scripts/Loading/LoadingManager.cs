using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMPro.TextMeshProUGUI progressText;

    [Header("Scene To Load")]
    [SerializeField] private string sceneToLoad = "GameScene";

    private void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);

        // IMPORTANT: prevents automatic scene switch
        operation.allowSceneActivation = false;

        float displayedProgress = 0f;

        while (!operation.isDone)
        {
            // Unity progress goes 0 → 0.9 before activation
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // Smooth UI
            displayedProgress = Mathf.Lerp(displayedProgress, targetProgress, Time.deltaTime * 5f);

            if (progressBar)
                progressBar.value = displayedProgress;

            if (progressText)
                progressText.text = Mathf.RoundToInt(displayedProgress * 100f) + "%";

            // When fully loaded, allow scene switch
            if (operation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.3f); // optional polish delay
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}