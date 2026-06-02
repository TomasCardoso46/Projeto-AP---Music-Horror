using UnityEngine;

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

        string pendingSave = PlayerPrefs.GetString("PendingSaveToLoad", "");

        if (!string.IsNullOrEmpty(pendingSave))
        {
            PlayerPrefs.DeleteKey("PendingSaveToLoad");
            SaveManager.Instance.LoadGame(pendingSave);
        }
    }
}