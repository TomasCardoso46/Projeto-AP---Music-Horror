using System.Collections;
using UnityEngine;

public class AutoSaveController : MonoBehaviour
{
    public float interval = 120f;

    private void Start()
    {
        StartCoroutine(AutoSaveLoop());
    }

    private IEnumerator AutoSaveLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            var sm = SaveManager.Instance;
            if (sm == null) continue;

            if (sm.IsEnemyInvestigating())
                continue;

            sm.CreateAutoSave("Auto", "default");
        }
    }
}