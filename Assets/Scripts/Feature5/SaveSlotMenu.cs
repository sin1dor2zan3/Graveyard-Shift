using TMPro;
using UnityEngine;

public class SaveSlotMenu : MonoBehaviour
{
    [SerializeField] private GameObject slotPanel;
    [SerializeField] private SceneNavigation navigation;

    [Header("Slot Card Text")]
    [SerializeField] private TMP_Text slotOneText;
    [SerializeField] private TMP_Text slotTwoText;
    [SerializeField] private TMP_Text slotThreeText;

    private bool loading;

    public void Open()
    {
        loading = false;
        Refresh();

        if (slotPanel != null)
        {
            slotPanel.SetActive(true);
            slotPanel.transform.SetAsLastSibling();
        }
    }

    public void Close()
    {
        if (!loading && slotPanel != null)
            slotPanel.SetActive(false);
    }

    public void Refresh()
    {
        RefreshSlot(1, slotOneText);
        RefreshSlot(2, slotTwoText);
        RefreshSlot(3, slotThreeText);
    }

    private void RefreshSlot(int slot, TMP_Text label)
    {
        if (label == null)
            return;

        PlayerProgress.SlotSummary summary =
            PlayerProgress.GetSlotSummary(slot);

        label.richText = true;
        label.alignment = TextAlignmentOptions.Top;
        label.raycastTarget = false;

        string details = summary.exists
            ? $"Night {summary.night}\n" +
              $"Delivered: {summary.successfulOrders}\n" +
              $"Failed: {summary.failedOrders}\n" +
              $"Glimmer: {summary.wallet}"
            : "Empty\n—\n—\n—";

        string action = summary.exists ? "Continue" : "New Game";

        label.text =
            $"<b>Slot {slot}</b>\n\n" +
            $"{details}\n\n" +
            $"<b>{action}</b>";
    }

    public void ChooseSlot(int slot)
    {
        if (loading || navigation == null)
            return;

        if (!SaveSlots.SelectSlot(slot))
            return;

        loading = true;
        navigation.BeginShift();

        // If navigation failed, leave the menu usable.
        loading = false;
    }
    [ContextMenu("Clear All Test Saves")]
    private void ClearAllTestSaves()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Stop Play Mode before clearing test saves.");
            return;
        }

        PlayerProgress.ResetAllTestProgress();
        Refresh();
    }
}
