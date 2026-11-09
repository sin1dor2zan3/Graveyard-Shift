using UnityEngine;
using System.Collections.Generic;
public class ShiftController : MonoBehaviour
{
    public static ShiftController Instance { get; private set; }

    private OrderDefinition[] shiftOrders;

    [Header("Original Customers")]
    [SerializeField] private OrderDefinition[] orders;

    [Header("New Customers")]
    [SerializeField] private OrderDefinition[] secondShiftOrders;

    [Header("Third Shift Customers")]
    [SerializeField] private OrderDefinition[] thirdShiftOrders;

    [Header("Testing")]
    [SerializeField] private bool testNewCustomers;
    [SerializeField] private bool testThirdShift;
    private int currentCustomerSet;
    public bool IsClosingResume { get; private set; }
    public int CurrentOrderNumber => currentIndex + 1;
    public int OrderCount => shiftOrders != null ? shiftOrders.Length : 0; public bool HasNextOrder => currentIndex + 1 < OrderCount;

    private int currentIndex = -1;
    public int SuccessfulOrders { get; private set; }
    public int FailedOrders { get; private set; }
    public int TotalSpent { get; private set; }
    public int ShiftEarnings { get; private set; }
    public int NightNumber { get; private set; }
    public bool IsThirdShiftSet { get; private set; }
    private bool shiftCompleted;

    private readonly HashSet<int> recordedOrders = new HashSet<int>();

    private PlayerProgress.ShiftCheckpoint CreateCheckpoint()
    {
        string[] names = new string[shiftOrders.Length];
        int[] budgets = new int[shiftOrders.Length];

        for (int i = 0; i < shiftOrders.Length; i++)
        {
            names[i] = shiftOrders[i].name;
            budgets[i] = shiftOrders[i].budget;
        }

        bool finished = SuccessfulOrders + FailedOrders >= OrderCount;

        return new PlayerProgress.ShiftCheckpoint
        {
            nightNumber = NightNumber,
            customerSet = currentCustomerSet,
            orderNames = names,
            orderBudgets = budgets,
            nextOrderIndex = SuccessfulOrders + FailedOrders,
            successfulOrders = SuccessfulOrders,
            failedOrders = FailedOrders,
            totalSpent = TotalSpent,
            shiftEarnings = ShiftEarnings,
            ordersFinished = finished
        };
    }

    private OrderDefinition FindOrder(string assetName)
    {
        OrderDefinition[][] sets =
        {
        orders,
        secondShiftOrders,
        thirdShiftOrders
    };

        foreach (OrderDefinition[] set in sets)
        {
            if (set == null)
                continue;

            foreach (OrderDefinition order in set)
            {
                if (order != null && order.name == assetName)
                    return order;
            }
        }

        return null;
    }

    public bool ResumeOrBeginShift()
    {
        PlayerProgress.ShiftCheckpoint saved =
            PlayerProgress.GetCheckpoint();

        if (saved == null)
            return BeginShift();

        if (saved.orderNames == null ||
            saved.orderBudgets == null ||
            saved.orderNames.Length != 3 ||
            saved.orderBudgets.Length != 3 ||
            saved.nightNumber < 1 ||
            saved.customerSet < 0 ||
            saved.customerSet > 2 ||
            saved.nextOrderIndex < 0 ||
            saved.nextOrderIndex > 3 ||
            saved.successfulOrders < 0 ||
            saved.failedOrders < 0 ||
            saved.successfulOrders + saved.failedOrders !=
                saved.nextOrderIndex ||
            saved.ordersFinished != (saved.nextOrderIndex == 3))
        {
            Debug.LogError("Saved shift is invalid. Save was left unchanged.");
            return false;
        }

        OrderDefinition[] restored = new OrderDefinition[3];

        // Validate every asset before changing the current shift.
        for (int i = 0; i < restored.Length; i++)
        {
            restored[i] = FindOrder(saved.orderNames[i]);

            if (restored[i] == null)
            {
                Debug.LogError(
                    $"Cannot find saved order: {saved.orderNames[i]}. " +
                    "Save was left unchanged.");
                return false;
            }
        }

        if (shiftOrders != null)
        {
            foreach (OrderDefinition previous in shiftOrders)
            {
                if (previous != null)
                    Destroy(previous);
            }
        }

        shiftOrders = new OrderDefinition[restored.Length];

        for (int i = 0; i < restored.Length; i++)
        {
            shiftOrders[i] = Instantiate(restored[i]);
            shiftOrders[i].name = restored[i].name;
            shiftOrders[i].budget = saved.orderBudgets[i];
        }

        NightNumber = saved.nightNumber;
        currentCustomerSet = saved.customerSet;
        IsThirdShiftSet = currentCustomerSet == 2;

        SuccessfulOrders = saved.successfulOrders;
        FailedOrders = saved.failedOrders;
        TotalSpent = saved.totalSpent;
        ShiftEarnings = saved.shiftEarnings;

        shiftCompleted = saved.ordersFinished;
        IsClosingResume = saved.ordersFinished;

        recordedOrders.Clear();

        for (int i = 0; i < saved.nextOrderIndex; i++)
            recordedOrders.Add(i);

        OrderSession session = OrderSession.GetOrCreate();
        session.ClearOrder();

        if (IsClosingResume)
        {
            currentIndex = OrderCount - 1;
            return true;
        }

        currentIndex = saved.nextOrderIndex;
        return session.StartOrder(shiftOrders[currentIndex]);
    }
    public void RecordCurrentResult()
    {
        OrderSession session = OrderSession.Instance;

        if (currentIndex < 0 ||
            session == null ||
            !session.ReceiptGiven ||
            !recordedOrders.Add(currentIndex))
        {
            return;
        }

        bool succeeded = session.OrderSucceeded;

        if (succeeded)
        {
            SuccessfulOrders++;
            ShiftEarnings += PlayerProgress.SuccessfulOrderPay;
        }
        else
        {
            FailedOrders++;
        }

        TotalSpent += session.TotalPrice;

        PlayerProgress.ShiftCheckpoint checkpoint =
            CreateCheckpoint();

        shiftCompleted = checkpoint.ordersFinished;

        PlayerProgress.RecordOrderResult(succeeded, checkpoint);
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

    public bool BeginShift()
    {
        int upcomingNight = PlayerProgress.CurrentNight;

        int shiftSet = (upcomingNight - 1) % 3;

        if (testThirdShift)
            shiftSet = 2;
        else if (testNewCustomers)
            shiftSet = 1;

        OrderDefinition[] selectedOrders;

        switch (shiftSet)
        {
            case 1:
                selectedOrders = secondShiftOrders;
                break;

            case 2:
                selectedOrders = thirdShiftOrders;
                break;

            default:
                selectedOrders = orders;
                break;
        }

        if (selectedOrders == null || selectedOrders.Length != 3)
        {
            Debug.LogError("Assign three orders to ShiftController.");
            return false;
        }

        foreach (OrderDefinition order in selectedOrders)
        {
            if (order == null ||
                order.items == null ||
                order.items.Length == 0)
            {
                Debug.LogError("Each order needs a definition and items.");
                return false;
            }

            foreach (ItemDefinition item in order.items)
            {
                if (item == null)
                {
                    Debug.LogError("An order has an empty item slot.");
                    return false;
                }
            }
        }

        // Remove the previous shift's temporary order copies.
        if (shiftOrders != null)
        {
            foreach (OrderDefinition previous in shiftOrders)
            {
                if (previous != null)
                    Destroy(previous);
            }
        }

        shiftOrders = new OrderDefinition[selectedOrders.Length];

        for (int i = 0; i < selectedOrders.Length; i++)
        {
            shiftOrders[i] = Instantiate(selectedOrders[i]);
            shiftOrders[i].name = selectedOrders[i].name;
            shiftOrders[i].budget =
                selectedOrders[i].budget + Random.Range(0, 3);
        }

        // Keep the third customer set in narrative order.
        if (shiftSet != 2)
        {
            for (int i = shiftOrders.Length - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);

                OrderDefinition temporary = shiftOrders[i];
                shiftOrders[i] = shiftOrders[randomIndex];
                shiftOrders[randomIndex] = temporary;
            }
        }

        if (!OrderSession.GetOrCreate().StartOrder(shiftOrders[0]))
            return false;
        NightNumber = PlayerProgress.CurrentNight;
        IsThirdShiftSet = shiftSet == 2;
        SuccessfulOrders = 0;
        FailedOrders = 0;
        TotalSpent = 0;
        ShiftEarnings = 0;
        shiftCompleted = false;
        recordedOrders.Clear();
        currentIndex = 0;
        currentCustomerSet = shiftSet;
        IsClosingResume = false;

        PlayerProgress.SaveCheckpoint(CreateCheckpoint());
        return true;
    }

    public bool StartNextOrder()
    {
        if (currentIndex < 0 || !HasNextOrder)
            return false;

        OrderSession session = OrderSession.GetOrCreate();

        if (!session.ReceiptGiven)
            return false;

        int nextIndex = currentIndex + 1;

        if (!session.StartOrder(shiftOrders[nextIndex]))
            return false;

        currentIndex = nextIndex;
        return true;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}