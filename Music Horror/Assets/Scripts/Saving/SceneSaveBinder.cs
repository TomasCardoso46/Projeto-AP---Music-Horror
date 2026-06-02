using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SceneSaveBinder : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private EnemyController enemy;
    [SerializeField] private Transform drawingsRoot;
    [SerializeField] private MonoBehaviour playerController;

    private void Start()
    {
        SaveManager.Instance.BindScene(
            player,
            enemy,
            drawingsRoot,
            playerController
        );

        StartCoroutine(LoadPending());
    }

    private IEnumerator LoadPending()
    {
        yield return null; // ensures scene is fully initialized

        string pendingSave = PlayerPrefs.GetString("PendingSaveToLoad", "");

        if (!string.IsNullOrEmpty(pendingSave))
        {
            PlayerPrefs.DeleteKey("PendingSaveToLoad");
            SaveManager.Instance.LoadGame(pendingSave);
        }
    }
}