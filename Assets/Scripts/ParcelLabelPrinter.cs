using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParcelLabelPrinter : MonoBehaviour
{
    [SerializeField] private GameObject labelChoicePanel;
    [SerializeField] private TMP_Text destinationText;

    [SerializeField] private TMP_Text optionAText;
    [SerializeField] private TMP_Text optionBText;
    [SerializeField] private TMP_Text optionCText;

    [SerializeField] private Button openLabelsButton;
    [SerializeField] private TMP_Text openLabelsButtonText;

    [SerializeField] private GameObject printedLabel;
    [SerializeField] private TMP_Text printedLabelText;

    public bool HasPrintedLabel { get; private set; }

    private string[] recipients;
    private string[] addresses;

    private void Start()
    {
        if (!ReferencesAssigned())
        {
            enabled = false;
            return;
        }

        labelChoicePanel.SetActive(false);
        printedLabel.SetActive(false);
        HasPrintedLabel = false;

        RefreshButton();
    }

    private void Update()
    {
        RefreshButton();
    }

    private void RefreshButton()
    {
        OrderSession session = OrderSession.Instance;

        bool ready = session != null &&
            session.HasActiveOrder &&
            session.PackingApproved &&
            session.IsWeighed &&
            session.IsSealed;

        openLabelsButton.interactable = ready && !HasPrintedLabel;

        if (HasPrintedLabel)
            openLabelsButtonText.text = "Label Printed";
        else if (!ready)
            openLabelsButtonText.text = "Seal Box First";
        else
            openLabelsButtonText.text = "Print Label";
    }

    public void OpenLabels()
    {
        OrderSession session = OrderSession.Instance;

        if (session == null ||
            !session.HasActiveOrder ||
            !session.PackingApproved ||
            !session.IsWeighed ||
            !session.IsSealed ||
            HasPrintedLabel)
        {
            return;
        }

        // The correct label uses the actual order information.
        // The other two addresses are distractors for this first order.
        recipients = new string[]
        {
            session.Recipient,
            session.Recipient,
            session.Recipient
        };

        addresses = new string[]
        {
            "31 Moonbeam Lane",
            session.Address,
            "13 Moonflower Lane"
        };

        optionAText.text = FormatLabel(0);
        optionBText.text = FormatLabel(1);
        optionCText.text = FormatLabel(2);

        ShowDestination(session);
        labelChoicePanel.SetActive(true);
    }

    public void ChooseA()
    {
        ChooseLabel(0);
    }

    public void ChooseB()
    {
        ChooseLabel(1);
    }

    public void ChooseC()
    {
        ChooseLabel(2);
    }

    private string FormatLabel(int index)
    {
        return $"{recipients[index]}\n{addresses[index]}";
    }

    private void ShowDestination(OrderSession session)
    {
        destinationText.text =
            $"Customer requested:\n{session.Recipient}, {session.Address}";
    }

    private void ChooseLabel(int index)
    {
        OrderSession session = OrderSession.Instance;

        if (session == null ||
            !session.IsSealed ||
            HasPrintedLabel ||
            !labelChoicePanel.activeSelf ||
            addresses == null ||
            recipients == null)
        {
            return;
        }

        bool correct =
            recipients[index] == session.Recipient &&
            addresses[index] == session.Address;

        if (!correct)
        {
            destinationText.text =
                "That address doesn't match. Try again.\n" +
                $"Requested: {session.Recipient}, {session.Address}";

            return;
        }

        HasPrintedLabel = true;

        printedLabelText.text =
            $"TO: {session.Recipient}\n" +
            $"{session.Address}\n" +
            "STANDARD SPECTRAL";

        labelChoicePanel.SetActive(false);
        printedLabel.SetActive(true);

        RefreshButton();
    }

    public void CloseLabels()
    {
        labelChoicePanel.SetActive(false);
    }

    private bool ReferencesAssigned()
    {
        if (labelChoicePanel == null ||
            destinationText == null ||
            optionAText == null ||
            optionBText == null ||
            optionCText == null ||
            openLabelsButton == null ||
            openLabelsButtonText == null ||
            printedLabel == null ||
            printedLabelText == null)
        {
            Debug.LogError(
                "ParcelLabelPrinter: assign every field in the Inspector."
            );

            return false;
        }

        return true;
    }
}
