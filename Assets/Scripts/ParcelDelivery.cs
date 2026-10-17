using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ParcelDelivery : MonoBehaviour
{
    [SerializeField] private PackingGrid packingGrid;
    [SerializeField] private OrderItemSpawner itemSpawner;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button shipButton;
    [SerializeField] private float feedbackDuration = 4f;

    public bool IsPreparingShipment => preparingShipment;
    private bool preparingShipment;
    private Coroutine clearFeedbackRoutine;

    private void Awake()
    {
        if (!ReferencesAssigned())
            return;

        feedbackText.text = "";
        shipButton.interactable = false;
    }

    public void SetBoxAvailable(bool available)
    {
        if (shipButton != null)
            shipButton.interactable = available && !preparingShipment;
    }

    public void ShipParcel()
    {
        if (PauseMenu.IsPaused || OrderPopupController.IsOpen ||
            preparingShipment || !ReferencesAssigned())
            return;

        OrderSession session = OrderSession.Instance;

        if (session == null || session.CurrentOrder == null)
        {
            ShowFeedback("Start a shift from the main menu.");
            return;
        }

        if (!session.HasSelectedBox || !packingGrid.isActiveAndEnabled)
        {
            ShowFeedback("Choose a box first.");
            return;
        }

        OrderDefinition order = session.CurrentOrder;

        if (itemSpawner.SpawnedItems.Count != order.items.Length)
        {
            ShowFeedback("The order's gifts have not loaded correctly.");
            return;
        }

        PackingPeanuts peanuts =
            packingGrid.GetComponent<PackingPeanuts>();

        if (peanuts == null)
        {
            ShowFeedback("The packing peanut tool is missing.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded("ShippingStation"))
        {
            ShowFeedback("ShippingStation is missing from the build.");
            return;
        }

        int packedWeight = 0;
        string issues = "";
        List<ItemDefinition> damaged = new List<ItemDefinition>();

        foreach (DraggableItem item in itemSpawner.SpawnedItems)
        {
            if (item == null || item.Definition == null)
            {
                ShowFeedback("A gift reference is missing.");
                return;
            }

            ItemDefinition definition = item.Definition;

            if (!packingGrid.IsPacked(item))
            {
                issues += $"Missing: {definition.itemName}.\n";
                continue;
            }

            packedWeight += definition.weightGrams;

            if (definition.isFragile && !peanuts.IsProtected(item))
            {
                damaged.Add(definition);
                issues += $"Damaged: {definition.itemName} — missing protection.\n";
            }
        }

        if (order.mustTouchFirst != null &&
            order.mustTouchSecond != null)
        {
            DraggableItem first = itemSpawner.SpawnedItems.Find(
                item => item.Definition == order.mustTouchFirst);

            DraggableItem second = itemSpawner.SpawnedItems.Find(
                item => item.Definition == order.mustTouchSecond);

            if (!packingGrid.AreNeighbors(first, second))
            {
                issues +=
                    $"{order.mustTouchFirst.itemName} was not beside " +
                    $"{order.mustTouchSecond.itemName}.\n";
            }
        }

        session.RecordPacking(packedWeight, issues, damaged);

        preparingShipment = true;
        shipButton.interactable = false;
        StopFeedbackTimer();

        SceneManager.LoadScene("ShippingStation");
    }

    public void ResetParcel()
    {
        if (PauseMenu.IsPaused || OrderPopupController.IsOpen ||
            preparingShipment || !ReferencesAssigned())
            return;

        PackingPeanuts peanuts = packingGrid.GetComponent<PackingPeanuts>();

        if (peanuts != null)
            peanuts.ClearPeanuts();

        PeanutScoop scoop = FindFirstObjectByType<PeanutScoop>();

        if (scoop != null)
            scoop.Empty();
        StopFeedbackTimer();
        feedbackText.text = "";
        itemSpawner.ResetItems();

        OrderSession session = OrderSession.Instance;
        if (session != null)
            session.ClearPackingApproval();

        SetBoxAvailable(session != null && session.HasSelectedBox &&
            packingGrid.isActiveAndEnabled);
    }

    private bool ReferencesAssigned()
    {
        if (packingGrid == null || itemSpawner == null ||
            feedbackText == null || shipButton == null)
        {
            Debug.LogError(
                "ParcelDelivery: assign grid, spawner, feedback, and button.",
                this);
            return false;
        }

        return true;
    }

    private void ShowFeedback(string message)
    {
        StopFeedbackTimer();
        feedbackText.text = message;
        clearFeedbackRoutine = StartCoroutine(ClearFeedbackAfterDelay());
    }

    private void StopFeedbackTimer()
    {
        if (clearFeedbackRoutine == null)
            return;
        StopCoroutine(clearFeedbackRoutine);
        clearFeedbackRoutine = null;
    }

    private IEnumerator ClearFeedbackAfterDelay()
    {
        yield return new WaitForSeconds(feedbackDuration);
        feedbackText.text = "";
        clearFeedbackRoutine = null;
    }
}
