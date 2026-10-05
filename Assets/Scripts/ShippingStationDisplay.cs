using TMPro;
using UnityEngine;

public class ShippingStationDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text summaryText;

    private void Start()
    {
        if (summaryText == null)
        {
            Debug.LogError(
                "ShippingStationDisplay: assign the summary text."
            );
            return;
        }

        OrderSession session = OrderSession.Instance;

        if (session == null || !session.HasActiveOrder)
        {
            summaryText.text =
                "No active parcel.\n" +
                "Start from MainMenu to accept a customer order.";

            return;
        }

        string packingStatus = session.PackingApproved
            ? "Packing checked"
            : "Packing has not been approved";

        summaryText.text =
            $"To: {session.Recipient} — {session.Address}\n" +
            $"{session.BoxName} box: {session.BoxPrice} glimmer" +
            $"   |   Budget: {session.Budget} glimmer\n" +
            packingStatus;
    }
}
