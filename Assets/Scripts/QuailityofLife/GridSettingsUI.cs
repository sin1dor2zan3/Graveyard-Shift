using UnityEngine;
using UnityEngine.UI;

public class GridSettingsUI : MonoBehaviour
{
    [SerializeField] private Toggle showGridToggle;

    private void OnEnable()
    {
        showGridToggle.SetIsOnWithoutNotify(
            PlayerPrefs.GetInt("ShowPackingGrid", 0) == 1
        );

        showGridToggle.onValueChanged.AddListener(ChangeGrid);
    }

    private void OnDisable()
    {
        showGridToggle.onValueChanged.RemoveListener(ChangeGrid);
    }

    private void ChangeGrid(bool visible)
    {
        PlayerPrefs.SetInt("ShowPackingGrid", visible ? 1 : 0);
        PlayerPrefs.Save();

        PackingGrid[] grids = FindObjectsByType<PackingGrid>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (PackingGrid grid in grids)
            grid.SetGridLines(visible);
    }
}
