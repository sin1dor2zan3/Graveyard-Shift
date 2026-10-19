using UnityEngine;
using System.Collections.Generic;
public class ShiftController : MonoBehaviour
{
    public static ShiftController Instance { get; private set; }

    private OrderDefinition[] shiftOrders;

    [SerializeField] private OrderDefinition[] orders;

    public int CurrentOrderNumber => currentIndex + 1;
    public int OrderCount => orders != null ? orders.Length : 0;
    public bool HasNextOrder => currentIndex + 1 < OrderCount;

    private int currentIndex = -1;
    public int SuccessfulOrders { get; private set; }
    public int FailedOrders { get; private set; }
    public int TotalSpent { get; private set; }
    public int ShiftEarnings { get; private set; }
    public int NightNumber { get; private set; }
    private bool shiftCompleted;

    private readonly HashSet<int> recordedOrders = new HashSet<int>();

    public void RecordCurrentResult()
    {
        OrderSession session = OrderSession.Instance;

        if (currentIndex < 0 || session == null ||
            !session.ReceiptGiven || !recordedOrders.Add(currentIndex))
        {
            return;
        }

        if (session.OrderSucceeded)
        {
            SuccessfulOrders++;
            ShiftEarnings += PlayerProgress.SuccessfulOrderPay;
            PlayerProgress.AddEarnings(PlayerProgress.SuccessfulOrderPay);
        }
        else
        {
            FailedOrders++;
        }

        TotalSpent += session.TotalPrice;

        if (!HasNextOrder && !shiftCompleted)
        {
            shiftCompleted = true;
            PlayerProgress.CompleteShift();
        }
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
        if (orders == null || orders.Length != 3)
        {
            Debug.LogError("Assign three orders to ShiftController.");
            return false;
        }

        foreach (OrderDefinition order in orders)
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

        shiftOrders = new OrderDefinition[orders.Length];

        for (int i = 0; i < orders.Length; i++)
        {
            shiftOrders[i] = Instantiate(orders[i]);

            // Keep the tested base budget, with up to 2 extra glimmer.
            shiftOrders[i].budget = orders[i].budget + Random.Range(0, 3);
        }

        // Shuffle without repeating or omitting customers.
        for (int i = shiftOrders.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            OrderDefinition temporary = shiftOrders[i];
            shiftOrders[i] = shiftOrders[randomIndex];
            shiftOrders[randomIndex] = temporary;
        }

        if (!OrderSession.GetOrCreate().StartOrder(shiftOrders[0]))
            return false;
        NightNumber = PlayerProgress.CurrentNight;
        SuccessfulOrders = 0;
        FailedOrders = 0;
        TotalSpent = 0;
        ShiftEarnings = 0;
        shiftCompleted = false;
        recordedOrders.Clear();
        currentIndex = 0;
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