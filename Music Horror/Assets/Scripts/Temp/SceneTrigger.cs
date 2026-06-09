using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTrigger : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string sceneToLoad;

    [Header("Fade")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private bool isLoading = false;

    private void Start()
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading)
            return;

        if (other.CompareTag("Player"))
        {
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                StartCoroutine(FadeAndLoadScene());
            }
            else
            {
                Debug.LogWarning("SceneTrigger: No scene name assigned!");
            }
        }
    }

    private IEnumerator FadeAndLoadScene()
    {
        isLoading = true;

        if (fadeImage != null)
        {
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;

                Color c = fadeImage.color;
                c.a = Mathf.Clamp01(timer / fadeDuration);
                fadeImage.color = c;

                yield return null;
            }
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}