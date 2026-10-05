using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParcelScale : MonoBehaviour
{
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button weighButton;

    private void Start()
    {
        if (!ReferencesAssigned())
            return;

        OrderSession session = OrderSession.Instance;

        bool ready = session != null &&
            session.HasActiveOrder &&
            session.PackingApproved;

        weighButton.interactable = ready;

        if (!ready)
        {
            weightText.text = "No parcel ready";
            costText.text = "Accept and pack an order first.";
            return;
        }

        if (session.IsWeighed)
        {
            DisplayResults(session);
        }
        else
        {
            weightText.text = "Not weighed";
            costText.text = "Weigh the parcel to calculate postage.";
        }
    }

    public void Weigh()
    {
        if (!ReferencesAssigned())
            return;

        OrderSession session = OrderSession.Instance;

        if (session == null || !session.WeighParcel())
        {
            weightText.text = "No parcel ready";
            costText.text = "Accept and pack an order first.";
            return;
        }

        DisplayResults(session);
    }

    private void DisplayResults(OrderSession session)
    {
        weightText.text = $"{session.TotalWeightGrams} g";

        string budgetMessage = session.RemainingBudget >= 0
            ? $"Budget left: {session.RemainingBudget} glimmer"
            : $"Over budget: {-session.RemainingBudget} glimmer";

        costText.text =
            $"Box: {session.BoxPrice} glimmer\n" +
            $"Postage: {session.PostagePrice} glimmer\n" +
            $"Total: {session.TotalPrice} glimmer\n" +
            budgetMessage;
    }

    private bool ReferencesAssigned()
    {
        if (weightText == null ||
            costText == null ||
            weighButton == null)
        {
            Debug.LogError(
                "ParcelScale: assign every field in the Inspector."
            );

            return false;
        }

        return true;
    }
}