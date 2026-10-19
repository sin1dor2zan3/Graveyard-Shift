using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UpgradeShopUI : MonoBehaviour
{
    [Header("Shop")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TMP_Text walletText;

    [Header("Purchase Buttons")]
    [SerializeField] private Button scoopButton;
    [SerializeField] private Button boxButton;
    [SerializeField] private Button cushioningButton;

    [Header("Button Labels")]
    [SerializeField] private TMP_Text scoopButtonText;
    [SerializeField] private TMP_Text boxButtonText;
    [SerializeField] private TMP_Text cushioningButtonText;

    public void OpenShop()
    {
        if (shopPanel == null)
        {
            Debug.LogError("UpgradeShopUI: assign Shop Panel.");
            return;
        }

        shopPanel.SetActive(true);
        Refresh();
    }

    public void BuyScoop()
    {
        PlayerProgress.BuyLargerScoop();
        Refresh();
    }

    public void BuyBoxes()
    {
        PlayerProgress.BuyCheaperBoxes();
        Refresh();
    }

    public void BuyCushioning()
    {
        PlayerProgress.BuyCheaperCushioning();
        Refresh();
    }

    public void Refresh()
    {
        if (walletText != null)
            walletText.text = PlayerProgress.Wallet.ToString();

        RefreshButton(
            scoopButton,
            scoopButtonText,
            PlayerProgress.HasLargerScoop,
            PlayerProgress.LargerScoopPrice);

        RefreshButton(
            boxButton,
            boxButtonText,
            PlayerProgress.HasCheaperBoxes,
            PlayerProgress.CheaperBoxesPrice);

        RefreshButton(
            cushioningButton,
            cushioningButtonText,
            PlayerProgress.HasCheaperCushioning,
            PlayerProgress.CheaperCushioningPrice);
    }

    private void RefreshButton(
        Button button,
        TMP_Text label,
        bool purchased,
        int price)
    {
        bool affordable = PlayerProgress.Wallet >= price;

        if (button != null)
            button.interactable = !purchased && affordable;

        if (label != null)
        {
            label.text = purchased
                ? "Purchased"
                : $"Buy — {price} glimmer";
        }
    }

    public void StartNextShift()
    {
        if (PauseMenu.IsPaused)
            return;

        ShiftController shift = ShiftController.Instance;

        if (shift == null ||
            !Application.CanStreamedLevelBeLoaded("CustomerCounter"))
        {
            Debug.LogError(
                "Cannot start shift: check ShiftController and build scenes.");
            return;
        }

        if (shift.BeginShift())
            SceneManager.LoadScene("CustomerCounter");
    }
}