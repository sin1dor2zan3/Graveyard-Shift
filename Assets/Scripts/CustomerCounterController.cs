using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomerCounterController : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text budgetText;
    [SerializeField] private TMP_Text receiptText;

    [Header("Panels")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject receiptPanel;
    [SerializeField] private GameObject displayItems;

    [Header("Buttons")]
    [SerializeField] private GameObject acceptButton;
    [SerializeField] private GameObject giveReceiptButton;
    [SerializeField] private GameObject finishShiftButton;

    [Header("Customer Introduction")]
    [TextArea(4, 8)]
    [SerializeField]
    private string openingDialogue =
        "Hello… I have a delivery for an old friend.\n" +
        "This is Theodore Graham, his little ghost charm, " +
        "and a letter.\n" +
        "Please keep the charm beside Theodore. " +
        "Neither likes traveling alone.";

    [SerializeField, Min(0f)]
    private float dialogueDelay = 0.8f;

    [SerializeField, Min(0f)]
    private float itemsDelay = 0.6f;

    [SerializeField, Min(0f)]
    private float nextButtonDelay = 0.3f;

    [Header("Expressions — Optional")]
    [SerializeField] private SpriteRenderer ghostFace;
    [SerializeField] private Sprite neutralFace;
    [SerializeField] private Sprite happyFace;

    private OrderSession session;
    private Coroutine introduction;

    private void Awake()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (displayItems != null)
            displayItems.SetActive(false);

        if (receiptPanel != null)
            receiptPanel.SetActive(false);

        if (acceptButton != null)
            acceptButton.SetActive(false);

        if (giveReceiptButton != null)
            giveReceiptButton.SetActive(false);

        if (finishShiftButton != null)
            finishShiftButton.SetActive(false);
    }

    private void Start()
    {
        if (!ReferencesAssigned())
        {
            enabled = false;
            return;
        }

        session = OrderSession.GetOrCreate();

        // Allow direct testing of this scene.
        if (!session.HasActiveOrder)
            session.StartFirstOrder();

        budgetText.text = $"Budget: {session.Budget} glimmer";

        if (session.ShipmentSent)
        {
            RefreshCounter();
        }
        else
        {
            SetExpression(neutralFace);
            introduction = StartCoroutine(ShowIntroduction());
        }
    }

    private IEnumerator ShowIntroduction()
    {
        yield return new WaitForSeconds(dialogueDelay);

        dialogueText.text = openingDialogue;
        dialoguePanel.SetActive(true);

        yield return new WaitForSeconds(itemsDelay);

        displayItems.SetActive(true);

        yield return new WaitForSeconds(nextButtonDelay);

        acceptButton.SetActive(true);
        introduction = null;
    }

    private void RefreshCounter()
    {
        bool sent = session.ShipmentSent;
        bool receiptGiven = session.ReceiptGiven;

        dialoguePanel.SetActive(true);
        displayItems.SetActive(!sent);

        acceptButton.SetActive(!sent);
        giveReceiptButton.SetActive(sent && !receiptGiven);
        finishShiftButton.SetActive(sent && receiptGiven);
        receiptPanel.SetActive(sent && !receiptGiven);

        budgetText.text = $"Budget: {session.Budget} glimmer";

        if (!sent)
        {
            dialogueText.text = openingDialogue;
            SetExpression(neutralFace);
            return;
        }

        receiptText.text =
            "REST IN POST\n\n" +
            $"To: {session.Recipient}\n" +
            $"{session.Address}\n" +
            $"Box: {session.BoxName}\n" +
            $"Weight: {session.TotalWeightGrams} g\n\n" +
            $"Box: {session.BoxPrice} glimmer\n" +
            $"Postage: {session.PostagePrice} glimmer\n" +
            $"Total: {session.TotalPrice} glimmer";

        if (!receiptGiven)
        {
            dialogueText.text =
                "All sent? Wonderful. May I have my receipt?";

            SetExpression(neutralFace);
        }
        else
        {
            dialogueText.text =
                "You kept them together. Thank you. " +
                "My friend will be so happy.\n\n" +
                "Shift complete — one parcel sent with care.";

            budgetText.text =
                $"Order total: {session.TotalPrice} glimmer";

            SetExpression(happyFace);
        }
    }

    private void SetExpression(Sprite expression)
    {
        if (ghostFace != null && expression != null)
            ghostFace.sprite = expression;
    }

    public void HandOverReceipt()
    {
        if (PauseMenu.IsPaused ||
            session == null ||
            !session.GiveReceipt())
        {
            return;
        }

        RefreshCounter();
    }

    public void FinishShift()
    {
        if (PauseMenu.IsPaused ||
            session == null ||
            !session.ReceiptGiven)
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded("MainMenu"))
        {
            Debug.LogError(
                "Add MainMenu to the active build scene list."
            );
            return;
        }

        session.ClearOrder();
        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        if (introduction != null)
            StopCoroutine(introduction);
    }

    private bool ReferencesAssigned()
    {
        if (dialogueText == null ||
            budgetText == null ||
            receiptText == null ||
            dialoguePanel == null ||
            receiptPanel == null ||
            displayItems == null ||
            acceptButton == null ||
            giveReceiptButton == null ||
            finishShiftButton == null)
        {
            Debug.LogError(
                "CustomerCounterController: assign all text, " +
                "panel, display-items, and button fields.",
                this
            );
            return false;
        }

        return true;
    }
}