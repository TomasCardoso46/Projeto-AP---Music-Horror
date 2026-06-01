using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMenuUI : MonoBehaviour
{
    public enum SaveFilter { Manual, Auto, Forced }

    [SerializeField] private Transform contentRoot;
    [SerializeField] private SaveSlotUI slotPrefab;

    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject loadMenuRoot;

    private SaveFilter currentFilter;

    private void OnEnable()
    {
        ShowManual();
    }

    public void ShowManual()
    {
        currentFilter = SaveFilter.Manual;
        Refresh();
    }

    public void ShowAuto()
    {
        currentFilter = SaveFilter.Auto;
        Refresh();
    }

    public void ShowForced()
    {
        currentFilter = SaveFilter.Forced;
        Refresh();
    }

    public void Refresh()
    {
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        var saves = SaveSystem.GetAllSaves()
            .Where(s => MatchesFilter(s.data.saveType))
            .OrderByDescending(s => s.data.dateTime);

        foreach (var save in saves)
        {
            var slot = Instantiate(slotPrefab, contentRoot);
            slot.Setup(save.fileName, save.data);
        }
    }

    private bool MatchesFilter(string type)
    {
        return currentFilter switch
        {
            SaveFilter.Manual => type == "Manual",
            SaveFilter.Auto => type == "Auto",
            SaveFilter.Forced => type == "Forced",
            _ => false
        };
    }

    public void ReturnToMainMenu()
    {
        loadMenuRoot.SetActive(false);
        mainMenuRoot.SetActive(true);
    }

    public void DeleteAllSaves()
    {
        foreach (var save in SaveSystem.GetAllSaves())
            SaveSystem.DeleteSave(save.fileName);

        Refresh();
    }
}