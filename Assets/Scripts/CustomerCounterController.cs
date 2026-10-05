using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomerCounterController : MonoBehaviour
{
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text budgetText;
    [SerializeField] private TMP_Text receiptText;

    [SerializeField] private GameObject receiptPanel;
    [SerializeField] private GameObject acceptButton;
    [SerializeField] private GameObject giveReceiptButton;
    [SerializeField] private GameObject finishShiftButton;

    private OrderSession session;

    private void Start()
    {
        if (!ReferencesAssigned())
        {
            enabled = false;
            return;
        }

        session = OrderSession.GetOrCreate();

        // Allow direct testing of the customer scene.
        if (!session.HasActiveOrder)
            session.StartFirstOrder();

        RefreshCounter();
    }

    private void RefreshCounter()
    {
        bool sent = session.ShipmentSent;
        bool receiptGiven = session.ReceiptGiven;

        acceptButton.SetActive(!sent);
        giveReceiptButton.SetActive(sent && !receiptGiven);
        finishShiftButton.SetActive(sent && receiptGiven);
        receiptPanel.SetActive(sent && !receiptGiven);

        budgetText.text = $"Budget: {session.Budget} glimmer";

        if (!sent)
        {
            dialogueText.text = session.Request;
            return;
        }

        receiptText.text =
            "REST IN POST\n\n" +
            $"To: {session.Recipient}\n" +
            $"{session.Address}\n" +
            $"Box: {session.BoxName}\n" +
            $"Weight: {session.TotalWeightGrams} g\n\n" +
            $"Box: {session.BoxPrice} glimmer\n" +
            $"Postage: {session.PostagePrice} glimmer\n" +
            $"Total: {session.TotalPrice} glimmer";

        if (!receiptGiven)
        {
            dialogueText.text =
                "\"All sent? Wonderful. May I have my receipt?\"";
        }
        else
        {
            dialogueText.text =
                "\"You kept them together. Thank you. " +
                "My friend will be so happy.\"\n\n" +
                "Shift complete — one parcel sent with care.";

            budgetText.text =
                $"Order total: {session.TotalPrice} glimmer";
        }
    }

    public void HandOverReceipt()
    {
        if (session == null || !session.GiveReceipt())
            return;

        RefreshCounter();
    }

    public void FinishShift()
    {
        if (session == null || !session.ReceiptGiven)
            return;

        if (!Application.CanStreamedLevelBeLoaded("MainMenu"))
        {
            Debug.LogError("Add MainMenu to the active build scene list.");
            return;
        }

        session.ClearOrder();
        SceneManager.LoadScene("MainMenu");
    }

    private bool ReferencesAssigned()
    {
        if (dialogueText == null ||
            budgetText == null ||
            receiptText == null ||
            receiptPanel == null ||
            acceptButton == null ||
            giveReceiptButton == null ||
            finishShiftButton == null)
        {
            Debug.LogError(
                "CustomerCounterController: assign every Inspector field."
            );
            return false;
        }

        return true;
    }
}