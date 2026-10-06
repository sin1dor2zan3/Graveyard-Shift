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

    private void Awake()
    {
        // DeliveryManager stays active; only PackingArea is hidden.
        if (packingGrid != null)
            packingGrid.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (!ReferencesAssigned())
            return;

        OrderSession session = OrderSession.GetOrCreate();
        if (!session.HasActiveOrder)
            session.StartFirstOrder();

        // Every newly loaded packing scene begins with a fresh choice.
        session.ClearBoxSelection();
        selectedSize = 0;
        BoxPrice = 0;
        BoxName = "";
        boxCostText.text = "Choose a box to begin.";
        parcelDelivery.SetBoxAvailable(false);
    }

    public void SelectSmall() { SelectBox(3); }
    public void SelectMedium() { SelectBox(4); }
    public void SelectLarge() { SelectBox(5); }

    private void SelectBox(int size)
    {
        if (PauseMenu.IsPaused || OrderPopupController.IsOpen ||
            !ReferencesAssigned() || selectedSize == size ||
            parcelDelivery.IsPreparingShipment)
            return;

        parcelDelivery.ResetParcel();
        OrderSession session = OrderSession.GetOrCreate();
        session.SelectBox(size);

        selectedSize = session.BoxSize;
        BoxName = session.BoxName;
        BoxPrice = session.BoxPrice;

        packingGrid.SetBoxSize(selectedSize);
        packingGrid.gameObject.SetActive(true);
        parcelDelivery.SetBoxAvailable(true);

        int remaining = session.Budget - BoxPrice;
        boxCostText.text =
            $"{BoxName} box: {BoxPrice} glimmer\n" +
            $"After box cost: {remaining} glimmer left";
    }

    private bool ReferencesAssigned()
    {
        if (packingGrid == null || parcelDelivery == null || boxCostText == null)
        {
            Debug.LogError("BoxSelection: assign every Inspector field.", this);
            return false;
        }
        return true;
    }
}
