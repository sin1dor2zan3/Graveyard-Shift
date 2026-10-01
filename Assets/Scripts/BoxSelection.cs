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

        selectedSize = 5;
        BoxName = "Large";
        BoxPrice = 7;

        packingGrid.SetBoxSize(selectedSize);
        UpdateCostText();
    }

    public void SelectSmall()
    {
        SelectBox(3, "Small", 3);
    }

    public void SelectMedium()
    {
        SelectBox(4, "Medium", 5);
    }

    public void SelectLarge()
    {
        SelectBox(5, "Large", 7);
    }

    private void SelectBox(int size, string boxName, int price)
    {
        if (!ReferencesAssigned() || selectedSize == size)
            return;

        parcelDelivery.ResetParcel();

        selectedSize = size;
        BoxName = boxName;
        BoxPrice = price;

        packingGrid.SetBoxSize(size);
        UpdateCostText();
    }

    private void UpdateCostText()
    {
        OrderSession session = OrderSession.GetOrCreate();

        if (!session.HasActiveOrder)
            session.StartFirstOrder();

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
