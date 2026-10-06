using TMPro;
using UnityEngine;

public class OrderPopupController : MonoBehaviour
{
    [SerializeField] private GameObject orderPopup;
    [SerializeField] private TMP_Text orderBudgetText;

    public static bool IsOpen { get; private set; }

    private void Awake()
    {
        IsOpen = false;

        if (orderPopup != null)
            orderPopup.SetActive(false);
    }

    public void OpenOrder()
    {
        if (PauseMenu.IsPaused || orderPopup == null)
            return;

        OrderSession session = OrderSession.GetOrCreate();

        if (!session.HasActiveOrder)
            session.StartFirstOrder();

        if (orderBudgetText != null)
        {
            orderBudgetText.text =
            $"Budget: {session.Budget}";
        }

        IsOpen = true;
        orderPopup.SetActive(true);
    }

    public void CloseOrder()
    {
        IsOpen = false;

        if (orderPopup != null)
            orderPopup.SetActive(false);
    }

    private void OnDisable()
    {
        CloseOrder();
    }
}