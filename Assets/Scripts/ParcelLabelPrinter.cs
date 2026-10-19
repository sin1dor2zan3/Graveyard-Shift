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
    public string PrintedRecipient { get; private set; }
    public string PrintedAddress { get; private set; }

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
        PrintedRecipient = "";
        PrintedAddress = "";

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
            session.IsSealed &&
            !session.ShipmentSent;

        openLabelsButton.interactable = ready && !HasPrintedLabel;

        openLabelsButtonText.text = HasPrintedLabel
            ? "Label Printed"
            : ready ? "Print Label" : "Seal Box First";
    }

    public void OpenLabels()
    {
        OrderSession session = OrderSession.Instance;

        if (PauseMenu.IsPaused ||
            session == null ||
            !session.HasActiveOrder ||
            !session.PackingApproved ||
            !session.IsWeighed ||
            !session.IsSealed ||
            session.ShipmentSent ||
            HasPrintedLabel)
        {
            return;
        }

        // Generate once per scene/order, so reopening preserves positions.
        if (recipients == null)
            CreateChoices(session);

        optionAText.text = FormatLabel(0);
        optionBText.text = FormatLabel(1);
        optionCText.text = FormatLabel(2);

        destinationText.text =
            $"Customer requested:\n{session.Recipient}\n{session.Address}";

        labelChoicePanel.SetActive(true);
    }

    private void CreateChoices(OrderSession session)
    {
        recipients = new[]
        {
            session.Recipient,
            MakeNameTypo(session.Recipient),
            session.Recipient
        };

        addresses = new[]
        {
            session.Address,
            session.Address,
            MakeWrongRoad(session.Address)
        };

        // Shuffle recipient/address pairs together.
        for (int i = recipients.Length - 1; i > 0; i--)
        {
            int other = Random.Range(0, i + 1);

            string recipient = recipients[i];
            recipients[i] = recipients[other];
            recipients[other] = recipient;

            string address = addresses[i];
            addresses[i] = addresses[other];
            addresses[other] = address;
        }
    }

    private string MakeNameTypo(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "Unknown Recipient";

        // Duplicate the final letter: Eleanor becomes Eleanorr.
        for (int i = name.Length - 1; i >= 0; i--)
        {
            if (char.IsLetter(name[i]))
                return name.Insert(i + 1, name[i].ToString());
        }

        return name + "e";
    }

    private string MakeWrongRoad(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return "Unknown Road";

        string[] suffixes =
        {
            "Lane", "Boulevard", "Drive", "Road",
            "Street", "Avenue", "Court", "Way",
            "Blvd", "Ln", "Dr", "Rd", "St", "Ave"
        };

        string[] words = address.Split(' ');

        for (int i = words.Length - 1; i >= 0; i--)
        {
            string word = words[i].TrimEnd('.', ',');

            foreach (string suffix in suffixes)
            {
                if (!string.Equals(
                    word, suffix,
                    System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string punctuation = words[i].Substring(word.Length);

                bool isDrive = string.Equals(
                    word, "Drive",
                    System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                    word, "Dr",
                    System.StringComparison.OrdinalIgnoreCase);

                words[i] = (isDrive ? "Lane" : "Drive") + punctuation;
                return string.Join(" ", words);
            }
        }

        return address + " Drive";
    }

    public void ChooseA() { ChooseLabel(0); }
    public void ChooseB() { ChooseLabel(1); }
    public void ChooseC() { ChooseLabel(2); }

    private string FormatLabel(int index)
    {
        return $"{recipients[index]}\n{addresses[index]}";
    }

    private void ChooseLabel(int index)
    {
        OrderSession session = OrderSession.Instance;

        if (PauseMenu.IsPaused ||
            session == null ||
            !session.HasActiveOrder ||
            !session.IsSealed ||
            session.ShipmentSent ||
            HasPrintedLabel ||
            !labelChoicePanel.activeSelf ||
            addresses == null ||
            recipients == null ||
            index < 0 || index >= recipients.Length)
        {
            return;
        }

        PrintedRecipient = recipients[index];
        PrintedAddress = addresses[index];
        HasPrintedLabel = true;

        printedLabelText.text =
            $"TO: {PrintedRecipient}\n{PrintedAddress}";

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
                "ParcelLabelPrinter: assign every Inspector field.");
            return false;
        }

        return true;
    }
}