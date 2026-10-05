using UnityEngine;

public class OrderSession : MonoBehaviour
{
    public static OrderSession Instance { get; private set; }

    public bool HasActiveOrder { get; private set; }
    public string CustomerName { get; private set; }
    public string Request { get; private set; }
    public string Recipient { get; private set; }
    public string Address { get; private set; }
    public int Budget { get; private set; }

    public string BoxName { get; private set; }
    public int BoxSize { get; private set; }
    public int BoxPrice { get; private set; }
    public int BoxWeightGrams { get; private set; }
    public bool PackingApproved { get; private set; }

    public bool IsWeighed { get; private set; }
    public int TotalWeightGrams { get; private set; }
    public int PostagePrice { get; private set; }
    public bool IsSealed { get; private set; }

    public bool LabelAttached { get; private set; }
    public string LabelRecipient { get; private set; }
    public string LabelAddress { get; private set; }

    public bool ShipmentSent { get; private set; }
    public bool ReceiptGiven { get; private set; }

    public int TotalPrice => BoxPrice + PostagePrice;
    public int RemainingBudget => Budget - TotalPrice;

    public bool CanSend =>
        HasActiveOrder &&
        PackingApproved &&
        IsWeighed &&
        IsSealed &&
        LabelAttached &&
        LabelRecipient == Recipient &&
        LabelAddress == Address &&
        RemainingBudget >= 0 &&
        !ShipmentSent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static OrderSession GetOrCreate()
    {
        if (Instance == null)
        {
            GameObject sessionObject = new GameObject("OrderSession");
            sessionObject.AddComponent<OrderSession>();
        }

        return Instance;
    }

    public void StartFirstOrder()
    {
        HasActiveOrder = true;
        CustomerName = "A Quiet Ghost";
        Recipient = "An Old Friend";
        Address = "13 Moonbeam Lane";
        Budget = 12;

        Request =
            "Please pack the teddy bear, ghost charm, and keepsake letter. " +
            "Keep the charm beside the bear. " +
            "Neither likes traveling alone.";

        SelectBox(5);
    }

    public void SelectBox(int size)
    {
        switch (size)
        {
            case 3:
                BoxName = "Small";
                BoxSize = 3;
                BoxPrice = 3;
                BoxWeightGrams = 100;
                break;
            case 4:
                BoxName = "Medium";
                BoxSize = 4;
                BoxPrice = 5;
                BoxWeightGrams = 200;
                break;
            case 5:
                BoxName = "Large";
                BoxSize = 5;
                BoxPrice = 7;
                BoxWeightGrams = 300;
                break;
            default:
                Debug.LogError("Unsupported box size: " + size);
                return;
        }

        ClearPackingApproval();
    }

    public void ApprovePacking()
    {
        if (HasActiveOrder && !ShipmentSent)
            PackingApproved = true;
    }

    public void ClearPackingApproval()
    {
        PackingApproved = false;
        IsWeighed = false;
        TotalWeightGrams = 0;
        PostagePrice = 0;
        IsSealed = false;

        LabelAttached = false;
        LabelRecipient = "";
        LabelAddress = "";

        ShipmentSent = false;
        ReceiptGiven = false;
    }

    public bool WeighParcel()
    {
        if (!HasActiveOrder || !PackingApproved || ShipmentSent)
            return false;

        // First-order contents: bear 400 g, charm 100 g, letter 50 g.
        TotalWeightGrams = 400 + 100 + 50 + BoxWeightGrams;
        PostagePrice = Mathf.CeilToInt(TotalWeightGrams / 1000f) * 4;

        IsWeighed = true;
        return true;
    }

    public bool SealParcel()
    {
        if (!HasActiveOrder || !PackingApproved ||
            !IsWeighed || ShipmentSent)
        {
            return false;
        }

        IsSealed = true;
        return true;
    }

    public bool AttachLabel(string recipient, string address)
    {
        if (!HasActiveOrder || !PackingApproved ||
            !IsWeighed || !IsSealed ||
            LabelAttached || ShipmentSent)
        {
            return false;
        }

        if (recipient != Recipient || address != Address)
            return false;

        LabelRecipient = recipient;
        LabelAddress = address;
        LabelAttached = true;
        return true;
    }

    public bool TrySendParcel()
    {
        if (!CanSend)
            return false;

        ShipmentSent = true;
        return true;
    }

    public bool GiveReceipt()
    {
        if (!HasActiveOrder || !ShipmentSent || ReceiptGiven)
            return false;

        ReceiptGiven = true;
        return true;
    }

    public void ClearOrder()
    {
        HasActiveOrder = false;
        CustomerName = "";
        Request = "";
        Recipient = "";
        Address = "";
        Budget = 0;

        BoxName = "";
        BoxSize = 0;
        BoxPrice = 0;
        BoxWeightGrams = 0;

        ClearPackingApproval();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}