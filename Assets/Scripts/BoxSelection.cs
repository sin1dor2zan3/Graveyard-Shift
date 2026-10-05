using TMPro;
using UnityEngine;

public class BoxSelection : MonoBehaviour
{
    [SerializeField] private PackingGrid packingGrid;
    [SerializeField] private ParcelDelivery parcelDelivery;
    [SerializeField] private TMP_Text boxCostText;

    public int BoxPrice { get; private set; }
    public string BoxName { get; private set; }

    private int selectedSize;

    private void Start()
    {
        if (!ReferencesAssigned())
            return;

        OrderSession session = OrderSession.GetOrCreate();

        if (!session.HasActiveOrder)
            session.StartFirstOrder();

        // A newly loaded packing scene has an empty grid.
        session.ClearPackingApproval();

        ApplySessionBox(session);
    }

    public void SelectSmall()
    {
        SelectBox(3);
    }

    public void SelectMedium()
    {
        SelectBox(4);
    }

    public void SelectLarge()
    {
        SelectBox(5);
    }

    private void SelectBox(int size)
    {
        if (!ReferencesAssigned() || selectedSize == size)
            return;

        parcelDelivery.ResetParcel();

        OrderSession session = OrderSession.GetOrCreate();
        session.SelectBox(size);

        ApplySessionBox(session);
    }

    private void ApplySessionBox(OrderSession session)
    {
        selectedSize = session.BoxSize;
        BoxName = session.BoxName;
        BoxPrice = session.BoxPrice;

        packingGrid.SetBoxSize(selectedSize);

        int remaining = session.Budget - BoxPrice;

        boxCostText.text =
            $"{BoxName} box: {BoxPrice} glimmer\n" +
            $"After box cost: {remaining} glimmer left";
    }

    private bool ReferencesAssigned()
    {
        if (packingGrid == null ||
            parcelDelivery == null ||
            boxCostText == null)
        {
            Debug.LogError(
                "BoxSelection: assign every field in the Inspector."
            );

            return false;
        }

        return true;
    }
}