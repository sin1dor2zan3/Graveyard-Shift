using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    [Header("Printing Animation")]
    [SerializeField] private Vector2 printStartOffset = new Vector2(0f, 90f);
    [SerializeField, Min(0.1f)] private float printDuration = 0.7f;
    [SerializeField] private RectMask2D labelPrintMask;

    public bool IsPrinting { get; private set; }

    [Header("Label Confirmation")]
    [SerializeField] private Button confirmPrintButton;
    [SerializeField] private TMP_Text selectedLabelText;

    public bool HasPrintedLabel { get; private set; }
    public string PrintedRecipient { get; private set; }
    public string PrintedAddress { get; private set; }

    private string[] recipients;
    private string[] addresses;
    private int selectedLabelIndex = -1;

    private void Start()
    {
        if (!ReferencesAssigned())
        {
            enabled = false;
            return;
        }

        if (labelPrintMask != null)
            labelPrintMask.enabled = false;

        labelChoicePanel.SetActive(false);
        printedLabel.SetActive(false);

        HasPrintedLabel = false;
        PrintedRecipient = "";
        PrintedAddress = "";
        selectedLabelIndex = -1;

        RefreshButton();
        RefreshSelection();
    }

    private void Update()
    {
        RefreshButton();
        RefreshSelection();
    }

    private bool ParcelReady()
    {
        OrderSession session = OrderSession.Instance;

        return session != null &&
            session.HasActiveOrder &&
            session.PackingApproved &&
            session.IsWeighed &&
            session.IsSealed &&
            !session.ShipmentSent &&
            !HasPrintedLabel;
    }

    private void RefreshButton()
    {
        bool ready = ParcelReady();

        openLabelsButton.interactable =
            ready && !PauseMenu.IsPaused;

        openLabelsButtonText.text = HasPrintedLabel
            ? "Label Printed"
            : ready ? "Open Computer" : "Weigh & Seal First";
    }

    public void OpenLabels()
    {
        if (PauseMenu.IsPaused || !ParcelReady())
            return;

        OrderSession session = OrderSession.Instance;

        // Preserve choices and selection when reopening the page.
        if (recipients == null)
            CreateChoices(session);

        optionAText.text = FormatLabel(0);
        optionBText.text = FormatLabel(1);
        optionCText.text = FormatLabel(2);

        destinationText.text =
            $"Customer requested:\n{session.Recipient}\n{session.Address}";

        labelChoicePanel.SetActive(true);
        RefreshSelection();
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
                    word,
                    suffix,
                    System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string punctuation =
                    words[i].Substring(word.Length);

                bool isDrive =
                    string.Equals(
                        word, "Drive",
                        System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        word, "Dr",
                        System.StringComparison.OrdinalIgnoreCase);

                words[i] =
                    (isDrive ? "Lane" : "Drive") + punctuation;

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

    private bool HasSelection()
    {
        return recipients != null &&
            addresses != null &&
            selectedLabelIndex >= 0 &&
            selectedLabelIndex < recipients.Length &&
            selectedLabelIndex < addresses.Length;
    }

    private void ChooseLabel(int index)
    {
        if (PauseMenu.IsPaused ||
            !ParcelReady() ||
            !labelChoicePanel.activeInHierarchy ||
            recipients == null ||
            addresses == null ||
            index < 0 ||
            index >= recipients.Length ||
            index >= addresses.Length)
        {
            return;
        }

        selectedLabelIndex = index;
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        bool hasSelection = HasSelection();

        confirmPrintButton.interactable =
            hasSelection &&
            ParcelReady() &&
            !PauseMenu.IsPaused;

        string message = hasSelection
            ? $"Selected: {recipients[selectedLabelIndex]}\n" +
              $"{addresses[selectedLabelIndex]}"
            : "Select a shipping label";

        OrderSession session = OrderSession.Instance;

        if (session != null && session.HasActiveOrder)
        {
            message +=
                $"\n{session.ServiceDescription}" +
                $" · Total: {session.TotalPrice} / " +
                $"{session.Budget} glimmer";
        }

        if (selectedLabelText.text != message)
            selectedLabelText.text = message;
    }

    public void PrintSelectedLabel()
    {
        if (PauseMenu.IsPaused ||
            !ParcelReady() ||
            !labelChoicePanel.activeInHierarchy ||
            !HasSelection())
        {
            return;
        }

        PrintedRecipient = recipients[selectedLabelIndex];
        PrintedAddress = addresses[selectedLabelIndex];
        HasPrintedLabel = true;
        GameSFX.Instance?.LabelPrint();

        printedLabelText.text =
            $"TO: {PrintedRecipient}\n{PrintedAddress}";

        labelChoicePanel.SetActive(false);
        StartCoroutine(AnimatePrinting());

        RefreshButton();
        RefreshSelection();
    }
    private IEnumerator AnimatePrinting()
    {
        IsPrinting = true;

        RectTransform rect = printedLabel.GetComponent<RectTransform>();
        Vector2 endPosition = rect.anchoredPosition;
        Vector2 startPosition = endPosition + printStartOffset;

        CanvasGroup group = printedLabel.GetComponent<CanvasGroup>();

        if (group == null)
            group = printedLabel.AddComponent<CanvasGroup>();

        float originalAlpha = group.alpha;
        bool originalInteractable = group.interactable;
        bool originalBlocksRaycasts = group.blocksRaycasts;

        group.interactable = false;
        group.blocksRaycasts = false;
        group.alpha = 0f;

        if (labelPrintMask != null)
            labelPrintMask.enabled = true;

        rect.anchoredPosition = startPosition;
        printedLabel.SetActive(true);

        yield return null;

        float elapsed = 0f;

        while (elapsed < printDuration)
        {
            if (!PauseMenu.IsPaused)
                elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / printDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, progress);

            rect.anchoredPosition =
                Vector2.Lerp(startPosition, endPosition, smooth);

            group.alpha = originalAlpha *
                Mathf.Clamp01(progress * 4f);

            yield return null;
        }

        rect.anchoredPosition = endPosition;

        if (labelPrintMask != null)
            labelPrintMask.enabled = false;

        group.alpha = originalAlpha;
        group.interactable = originalInteractable;
        group.blocksRaycasts = originalBlocksRaycasts;

        IsPrinting = false;
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
            printedLabelText == null ||
            confirmPrintButton == null ||
            selectedLabelText == null)
        {
            Debug.LogError(
                "ParcelLabelPrinter: assign every Inspector field.",
                this);

            return false;
        }

        return true;
    }
}