using UnityEngine;
using System.Collections.Generic;

public class OrderSession : MonoBehaviour
{
    public static OrderSession Instance { get; private set; }
    public OrderDefinition CurrentOrder { get; private set; }

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
    public bool HasSelectedBox => BoxSize >= 3 && BoxSize <= 5;
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

    public int CushioningPrice =>
    PackingApproved && HasSelectedBox
        ? PlayerProgress.GetCushioningPrice(BoxSize - 2)
        : 0;

    public int EstimatedCushioningPrice =>
        HasSelectedBox
            ? PlayerProgress.GetCushioningPrice(BoxSize - 2)
            : 0;

    public int EstimatedPostagePrice =>
        HasSelectedBox
            ? Mathf.CeilToInt(
                (ContentsWeightGrams + BoxWeightGrams) / 1000f) * 4
            : 0;

    public int EstimatedTotalPrice =>
        BoxPrice + EstimatedCushioningPrice + EstimatedPostagePrice;

    public int TotalPrice =>
        BoxPrice + CushioningPrice + PostagePrice; public int RemainingBudget => Budget - TotalPrice;

    public bool CanSend =>
        HasActiveOrder &&
        HasSelectedBox &&
        PackingApproved &&
        IsWeighed &&
        IsSealed &&
        LabelAttached &&
        !ShipmentSent;

    public int PackedContentsWeight { get; private set; }
    public string PackingIssues { get; private set; } = "";

    public List<ItemDefinition> DamagedItems { get; } =
        new List<ItemDefinition>();

    public bool OrderSucceeded =>
    ShipmentSent &&
    LabelAttached &&
    LabelRecipient == Recipient &&
    LabelAddress == Address &&
    string.IsNullOrEmpty(PackingIssues) &&
    RemainingBudget >= 0;

    public string ResultDescription
    {
        get
        {
            string result = PackingIssues;

            if (LabelAttached && LabelRecipient != Recipient)
                result += "Incorrect recipient name on shipping label.\n";

            if (LabelAttached && LabelAddress != Address)
                result += "Incorrect delivery address on shipping label.\n";

            if (RemainingBudget < 0)
                result += $"Over budget by {-RemainingBudget} glimmer.\n";

            return string.IsNullOrEmpty(result)
                ? "All gifts delivered safely and within budget."
                : result.Trim();
        }
    }

    public void RecordPacking(
        int packedWeight,
        string issues,
        List<ItemDefinition> damagedItems)
    {
        PackedContentsWeight = packedWeight;
        PackingIssues = issues;
        DamagedItems.Clear();
        DamagedItems.AddRange(damagedItems);
        ApprovePacking();
    }
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
    public int ContentsWeightGrams
    {
        get
        {
            if (CurrentOrder == null || CurrentOrder.items == null)
                return 0;

            int total = 0;

            foreach (ItemDefinition item in CurrentOrder.items)
            {
                if (item != null)
                    total += item.weightGrams;
            }

            return total;
        }
    }
    public bool StartOrder(OrderDefinition order)
    {
        if (order == null ||
            order.items == null ||
            order.items.Length == 0)
        {
            Debug.LogError("Order needs an assigned definition and items.");
            return false;
        }

        foreach (ItemDefinition item in order.items)
        {
            if (item == null)
            {
                Debug.LogError("An order contains an empty item slot.");
                return false;
            }
        }

        ClearBoxSelection();

        CurrentOrder = order;
        HasActiveOrder = true;

        CustomerName = order.customerName;
        Request = order.request;
        Recipient = order.recipient;
        Address = order.address;
        Budget = order.budget;
        PackedContentsWeight = 0;
        PackingIssues = "";
        DamagedItems.Clear();

        return true;
    }
    public void StartFirstOrder()
    {
        HasActiveOrder = true;
        CustomerName = "A Quiet Ghost";
        Recipient = "An Old Friend";
        Address = "13 Moonbeam Lane";
        Budget = 12;
        Request =
            "Please pack Theodore Graham, his ghost charm, and the keepsake letter. " +
            "Keep the charm beside Theodore. " +
            "Neither likes traveling alone.";

        ClearBoxSelection();
    }

    public void ClearBoxSelection()
    {
        BoxName = "";
        BoxSize = 0;
        BoxPrice = 0;
        BoxWeightGrams = 0;
        ClearPackingApproval();
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
        BoxPrice = PlayerProgress.GetBoxPrice(BoxPrice);
        ClearPackingApproval();
    }

    public void ApprovePacking()
    {
        if (HasActiveOrder && HasSelectedBox && !ShipmentSent)
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

        TotalWeightGrams = PackedContentsWeight + BoxWeightGrams;
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

        if (string.IsNullOrWhiteSpace(recipient) || string.IsNullOrWhiteSpace(address))
        {
            return false;
        }

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
        CurrentOrder = null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}