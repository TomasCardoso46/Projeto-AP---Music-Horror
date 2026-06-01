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
    }
}