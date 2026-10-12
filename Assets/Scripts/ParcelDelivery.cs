using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ParcelDelivery : MonoBehaviour
{
    [SerializeField] private PackingGrid packingGrid;
    [SerializeField] private DraggableItem ghostCharm;
    [SerializeField] private DraggableItem moonSeal;
    [SerializeField] private DraggableItem teddyBear;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button shipButton;
    [SerializeField] private float feedbackDuration = 4f;

    public bool IsPreparingShipment => preparingShipment;
    private bool preparingShipment;
    private Coroutine clearFeedbackRoutine;
    private Vector3 ghostStartPosition;
    private Vector3 moonStartPosition;
    private Vector3 bearStartPosition;

    private void Awake()
    {
        if (!ReferencesAssigned())
            return;

        ghostStartPosition = ghostCharm.transform.position;
        moonStartPosition = moonSeal.transform.position;
        bearStartPosition = teddyBear.transform.position;
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
        if (session == null || !session.HasActiveOrder)
        {
            ShowFeedback("Please start from the main menu to accept an order.");
            return;
        }

        if (!session.HasSelectedBox || !packingGrid.isActiveAndEnabled)
        {
            ShowFeedback("Choose a box to begin.");
            return;
        }

        if (!packingGrid.IsPacked(ghostCharm) ||
            !packingGrid.IsPacked(moonSeal) ||
            !packingGrid.IsPacked(teddyBear))
        {
            ShowFeedback("Please pack all three gifts first.");
            return;
        }

        if (!packingGrid.AreNeighbors(ghostCharm, teddyBear))
        {
            ShowFeedback("The ghost feels lonely! Its edge must touch Theodore.");
            return;
        }
        PackingPeanuts peanuts = packingGrid.GetComponent<PackingPeanuts>();

        if (peanuts == null)
        {
            ShowFeedback("The packing peanut tool is missing.");
            return;
        }

        int spacesLeft = peanuts.UnprotectedCellCount;

        if (spacesLeft > 0)
        {
            ShowFeedback(
                $"Fill {spacesLeft} empty space(s) with packing peanuts first.");
            return;
        }
        if (session.EstimatedTotalPrice > session.Budget)
        {
            ShowFeedback("Box, peanuts, and postage exceed the budget.Choose a smaller box.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded("ShippingStation"))
        {
            ShowFeedback("ShippingStation is missing from the build scene list.");
            Debug.LogError("Add ShippingStation to the active build profile's scene list.");
            return;
        }

        preparingShipment = true;
        shipButton.interactable = false;
        StopFeedbackTimer();
        session.ApprovePacking();
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
        ResetItem(ghostCharm, ghostStartPosition);
        ResetItem(moonSeal, moonStartPosition);
        ResetItem(teddyBear, bearStartPosition);

        OrderSession session = OrderSession.Instance;
        if (session != null)
            session.ClearPackingApproval();

        SetBoxAvailable(session != null && session.HasSelectedBox &&
            packingGrid.isActiveAndEnabled);
    }

    private void ResetItem(DraggableItem item, Vector3 startPosition)
    {
        item.enabled = false;
        packingGrid.RemoveItem(item);
        item.transform.position = startPosition;
        item.enabled = true;
    }

    private bool ReferencesAssigned()
    {
        if (packingGrid == null || ghostCharm == null || moonSeal == null ||
            teddyBear == null || feedbackText == null || shipButton == null)
        {
            Debug.LogError("ParcelDelivery: assign every Inspector field.", this);
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
