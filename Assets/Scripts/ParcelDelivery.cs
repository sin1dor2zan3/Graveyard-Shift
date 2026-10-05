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

    private bool preparingShipment;
    private Coroutine clearFeedbackRoutine;

    private Vector3 ghostStartPosition;
    private Vector3 moonStartPosition;
    private Vector3 bearStartPosition;

    private void Start()
    {
        if (!ReferencesAssigned())
            return;

        ghostStartPosition = ghostCharm.transform.position;
        moonStartPosition = moonSeal.transform.position;
        bearStartPosition = teddyBear.transform.position;

        feedbackText.text = "";
    }

    private bool ReferencesAssigned()
    {
        if (packingGrid == null ||
            ghostCharm == null ||
            moonSeal == null ||
            teddyBear == null ||
            feedbackText == null ||
            shipButton == null)
        {
            Debug.LogError(
                "ParcelDelivery: assign every field in the Inspector."
            );

            return false;
        }

        return true;
    }

    public void ShipParcel()
    {
        if (preparingShipment || !ReferencesAssigned())
            return;

        if (!packingGrid.IsPacked(ghostCharm) ||
            !packingGrid.IsPacked(moonSeal) ||
            !packingGrid.IsPacked(teddyBear))
        {
            ShowFeedback("Please pack all three gifts first.");
            return;
        }

        if (!packingGrid.AreNeighbors(ghostCharm, teddyBear))
        {
            ShowFeedback(
                "The ghost feels lonely! Its edge must touch the bear."
            );
            return;
        }

        OrderSession session = OrderSession.Instance;

        if (session == null || !session.HasActiveOrder)
        {
            ShowFeedback("Please start from the main menu to accept an order.");
            return;
        }

        if (session.BoxPrice > session.Budget)
        {
            ShowFeedback("This box exceeds the customer's budget.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded("ShippingStation"))
        {
            ShowFeedback("ShippingStation is missing from the build scene list.");

            Debug.LogError(
                "Add ShippingStation to the active build profile's scene list."
            );

            return;
        }

        // Packing has passed. Shipping preparation comes next.
        preparingShipment = true;
        shipButton.interactable = false;

        StopFeedbackTimer();
        session.ApprovePacking();

        SceneManager.LoadScene("ShippingStation");
    }

    public void ResetParcel()
    {
        if (preparingShipment || !ReferencesAssigned())
            return;

        StopFeedbackTimer();
        feedbackText.text = "";

        ResetItem(ghostCharm, ghostStartPosition);
        ResetItem(moonSeal, moonStartPosition);
        ResetItem(teddyBear, bearStartPosition);

        if (OrderSession.Instance != null)
            OrderSession.Instance.ClearPackingApproval();

        shipButton.interactable = true;
    }

    private void ResetItem(DraggableItem item, Vector3 startPosition)
    {
        item.enabled = false;
        packingGrid.RemoveItem(item);

        item.transform.position = startPosition;
        item.enabled = true;
    }

    private void ShowFeedback(string message)
    {
        StopFeedbackTimer();
        feedbackText.text = message;

        clearFeedbackRoutine =
            StartCoroutine(ClearFeedbackAfterDelay());
    }

    private void StopFeedbackTimer()
    {
        if (clearFeedbackRoutine != null)
        {
            StopCoroutine(clearFeedbackRoutine);
            clearFeedbackRoutine = null;
        }
    }

    private IEnumerator ClearFeedbackAfterDelay()
    {
        yield return new WaitForSeconds(feedbackDuration);

        feedbackText.text = "";
        clearFeedbackRoutine = null;
    }
}