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
    }

    public void ClearOrder()
    {
        HasActiveOrder = false;
        CustomerName = "";
        Request = "";
        Recipient = "";
        Address = "";
        Budget = 0;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}