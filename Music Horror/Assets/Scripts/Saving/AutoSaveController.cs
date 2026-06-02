using System.Collections;
using UnityEngine;

public class AutoSaveController : MonoBehaviour
{
    [SerializeField] private float interval = 120f;
    [SerializeField] private EnemyController enemy;

    private void Start()
    {
        StartCoroutine(AutoSaveLoop());
    }

    private IEnumerator AutoSaveLoop()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(interval);

            if (enemy != null)
            {
                if (enemy.currentState == EnemyController.State.Investigate ||
                    enemy.currentState == EnemyController.State.Chase ||
                    enemy.currentState == EnemyController.State.Attack)
                {
                    continue;
                }
            }

            SaveManager.Instance.CreateAutoSave();
        }
    }
}