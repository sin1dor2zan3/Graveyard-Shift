using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShippingServicesUI : MonoBehaviour
{
    [SerializeField] private Button standardButton;
    [SerializeField] private Button expressButton;
    [SerializeField] private Button wardButton;
    [SerializeField] private TMP_Text serviceText;
    [SerializeField] private ParcelScale parcelScale;

    private void OnEnable()
    {
        Refresh();
    }

    public void ChooseStandard()
    {
        if (!CanChoose())
            return;

        OrderSession.Instance.SelectStandard();
        GameSFX.Instance?.Click();
        Refresh();
    }

    public void ChooseExpress()
    {
        if (!CanChoose())
            return;

        OrderSession.Instance.SelectExpress();
        GameSFX.Instance?.Click();
        Refresh();
    }

    public void ToggleProtection()
    {
        if (!CanChoose())
            return;

        OrderSession.Instance.ToggleWard();
        if (OrderSession.Instance.UseWard)
            GameSFX.Instance?.MagicalProtection();
        else
            GameSFX.Instance?.Click();
        Refresh();
    }

    private bool CanChoose()
    {
        OrderSession session = OrderSession.Instance;

        return !PauseMenu.IsPaused &&
            !OrderPopupController.IsOpen &&
            session != null &&
            session.HasActiveOrder &&
            !session.LabelAttached &&
            !session.ShipmentSent;
    }

    private void Update()
    {
        RefreshButtons();
    }

    private void RefreshButtons()
    {
        OrderSession session = OrderSession.Instance;
        bool ready = CanChoose();
        bool hasOrder = session != null && session.HasActiveOrder;

        if (standardButton != null)
            standardButton.interactable = ready;

        if (expressButton != null)
            expressButton.interactable = ready;

        if (wardButton != null)
            wardButton.interactable = ready;

        SetCardText(
            standardButton,
            hasOrder && !session.UseExpress
                ? "Standard\nSelected"
                : "Standard\nBase postage"
        );

        SetCardText(
            expressButton,
            hasOrder && session.UseExpress
                ? "Express +3\nSelected"
                : "Express\n+3 glimmer"
        );

        SetCardText(
            wardButton,
            hasOrder && session.UseWard
                ? "Magical Protection\nON · +2 glimmer"
                : "Magical Protection\nOFF · +2 glimmer"
        );
    }
    private void SetCardText(Button button, string message)
    {
        if (button == null)
            return;

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);

        if (label != null && label.text != message)
            label.text = message;
    }

    private void Refresh()
    {
        OrderSession session = OrderSession.Instance;

        if (serviceText != null)
        {
            serviceText.text = session != null && session.HasActiveOrder
                ? $"Selected: {session.ServiceDescription}\n" +
                  $"Service extras: {session.ServicePrice} glimmer"
                : "No active order";
        }

        RefreshButtons();

        if (parcelScale != null)
            parcelScale.RefreshCosts();
    }
}