using System.Collections;
using UnityEngine;
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

    private bool delivered;
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
        if (delivered || !ReferencesAssigned())
            return;

        if (UnityEngine.InputSystem.Mouse.current != null &&
            UnityEngine.InputSystem.Mouse.current.leftButton.isPressed)
        {
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
            ShowFeedback(
                "The ghost feels lonely! Its edge must touch the bear."
            );
            return;
        }

        delivered = true;

        ShowFeedback(
            "Delivered! \"You kept them together. Thank you.\"",
            true
        );

        shipButton.interactable = false;

        ghostCharm.enabled = false;
        moonSeal.enabled = false;
        teddyBear.enabled = false;
    }

    public void ResetParcel()
    {
        if (!ReferencesAssigned())
            return;

        StopFeedbackTimer();
        feedbackText.text = "";

        ResetItem(ghostCharm, ghostStartPosition);
        ResetItem(moonSeal, moonStartPosition);
        ResetItem(teddyBear, bearStartPosition);

        delivered = false;
        shipButton.interactable = true;
    }

    private void ResetItem(DraggableItem item, Vector3 startPosition)
    {
        item.enabled = false;

        packingGrid.RemoveItem(item);

        item.transform.position = startPosition;
        item.enabled = true;
    }

    private void ShowFeedback(string message, bool keepVisible = false)
    {
        StopFeedbackTimer();

        feedbackText.text = message;

        if (!keepVisible)
        {
            clearFeedbackRoutine =
                StartCoroutine(ClearFeedbackAfterDelay());
        }
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
        yield return new WaitForSecondsRealtime(feedbackDuration);

        feedbackText.text = "";
        clearFeedbackRoutine = null;
    }
}