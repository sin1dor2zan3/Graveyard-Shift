using TMPro;
using UnityEngine;

public class OrderDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text requestText;
    [SerializeField] private TMP_Text budgetText;

    private void Start()
    {
        OrderSession session = OrderSession.GetOrCreate();

        if (!session.HasActiveOrder)
            session.StartFirstOrder();

        if (requestText != null)
            requestText.text = session.Request;

        if (budgetText != null)
            budgetText.text = $"Budget: {session.Budget}";
    }
}
