using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomerCounterController : MonoBehaviour
{
    [Header("Order Results")]
    [SerializeField] private Image resultIcon;
    [SerializeField] private Sprite successIcon;
    [SerializeField] private Sprite incompleteIcon;
    [SerializeField] private Sprite damagedIcon;
    [SerializeField] private Sprite sadFace;
    [Header("Text")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text budgetText;
    [SerializeField] private TMP_Text receiptText;

    [Header("Panels")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject receiptPanel;
    [SerializeField] private GameObject displayItems;
    [SerializeField] private SpriteRenderer[] giftPreviews;
    [SerializeField, Min(0.1f)] private float previewSize = 1.5f;

    [Header("Buttons")]
    [SerializeField] private GameObject acceptButton;
    [SerializeField] private GameObject giveReceiptButton;
    [SerializeField] private GameObject finishShiftButton;

    [Header("Customer Introduction")]

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
    private bool showingShiftSummary;

    private void Awake()
    {
        if (resultIcon != null)
            resultIcon.gameObject.SetActive(false);

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
    private void ShowOrderResult()
    {
        bool success = session.OrderSucceeded;
        bool damaged = session.DamagedItems.Count > 0;

        dialogueText.text = success
            ? "Thank you! Everything arrived safely.\n\nORDER COMPLETE"
            : "Something went wrong with my delivery.\n\n" +
              session.ResultDescription;

        budgetText.text =
            $"Order total: {session.TotalPrice} / {session.Budget} glimmer";

        SetExpression(success ? happyFace : sadFace);

        if (resultIcon != null)
        {
            resultIcon.sprite = success
                ? successIcon
                : damaged ? damagedIcon : incompleteIcon;

            resultIcon.preserveAspect = true;
            resultIcon.gameObject.SetActive(true);
        }

        // After delivery, show only the damaged gifts as result artwork.
        displayItems.SetActive(damaged);

        for (int i = 0; i < giftPreviews.Length; i++)
        {
            SpriteRenderer preview = giftPreviews[i];

            if (preview == null)
                continue;

            bool hasDamagedItem = i < session.DamagedItems.Count;
            preview.gameObject.SetActive(hasDamagedItem);

            if (!hasDamagedItem)
                continue;

            ItemDefinition item = session.DamagedItems[i];

            preview.sprite = item.damagedSprite != null
                ? item.damagedSprite
                : item.intactSprite;

            if (preview.sprite == null)
                continue;

            Vector3 size = preview.sprite.bounds.size;
            float longestSide = Mathf.Max(size.x, size.y);

            if (longestSide > 0f)
            {
                preview.transform.localScale =
                    Vector3.one * (previewSize / longestSide);
            }
        }
    }
    private void RefreshGiftPreviews()
    {
        OrderDefinition order = session.CurrentOrder;

        for (int i = 0; i < giftPreviews.Length; i++)
        {
            SpriteRenderer preview = giftPreviews[i];

            if (preview == null)
                continue;

            bool hasItem = order != null &&
                order.items != null &&
                i < order.items.Length &&
                order.items[i] != null;

            preview.gameObject.SetActive(hasItem);

            if (!hasItem)
                continue;

            preview.sprite = order.items[i].intactSprite;

            if (preview.sprite == null)
                continue;

            Vector3 size = preview.sprite.bounds.size;
            float longestSide = Mathf.Max(size.x, size.y);

            if (longestSide > 0f)
            {
                preview.transform.localScale =
                    Vector3.one * (previewSize / longestSide);
            }
        }
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
        RefreshGiftPreviews();
        budgetText.text = $"Budget: {session.Budget} glimmer";

        if (session.ShipmentSent)
        {
            RefreshCounter();
            TMP_Text nextButtonText =
            finishShiftButton.GetComponentInChildren<TMP_Text>(true);

            if (nextButtonText != null)
            {
                bool hasNext = ShiftController.Instance != null &&
                    ShiftController.Instance.HasNextOrder;

                nextButtonText.text = hasNext ? "Next Customer" : "Finish Shift";
            }
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

        dialogueText.text =
                $"{session.CustomerName}\n\n{session.Request}";
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
            dialogueText.text =
                $"{session.CustomerName}\n\n{session.Request}";
            SetExpression(neutralFace);
            return;
        }

        receiptText.text =
            "REST IN POST\n\n" +
            $"To: {session.Recipient}\n" +
            $"{session.Address}\n" +
            $"Box: {session.BoxName}\n" +
            $"Peanuts: {session.CushioningPrice} glimmer\n" +
            $"Weight: {session.TotalWeightGrams} g\n\n" +
            $"Box: {session.BoxPrice} glimmer\n" +
            $"Postage: {session.PostagePrice} glimmer\n" +
            $"Total: {session.TotalPrice} glimmer";

        if (!receiptGiven)
        {
            dialogueText.text =
                "Has the parcel been sent? May I have my receipt?";

            SetExpression(neutralFace);
        }
        else
        {
            ShowOrderResult();
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
        if (ShiftController.Instance != null)
            ShiftController.Instance.RecordCurrentResult();
        RefreshCounter();
    }

    public void FinishShift()
    {
        if (PauseMenu.IsPaused || session == null ||
            !session.ReceiptGiven)
        {
            return;
        }

        ShiftController shift = ShiftController.Instance;

        if (shift == null)
            return;

        if (showingShiftSummary)
        {
            if (!Application.CanStreamedLevelBeLoaded("MainMenu"))
                return;

            session.ClearOrder();
            SceneManager.LoadScene("MainMenu");
            return;
        }

        if (shift.HasNextOrder)
        {
            if (!Application.CanStreamedLevelBeLoaded("CustomerCounter"))
                return;

            if (shift.StartNextOrder())
                SceneManager.LoadScene("CustomerCounter");

            return;
        }

        showingShiftSummary = true;

        displayItems.SetActive(false);
        receiptPanel.SetActive(false);
        acceptButton.SetActive(false);
        giveReceiptButton.SetActive(false);

        if (resultIcon != null)
            resultIcon.gameObject.SetActive(false);

        dialoguePanel.SetActive(true);
        finishShiftButton.SetActive(true);

        dialogueText.text =
            "SHIFT COMPLETE\n\n" +
            $"Orders completed: {shift.SuccessfulOrders + shift.FailedOrders}" +
            $" / {shift.OrderCount}\n" +
            $"Successful: {shift.SuccessfulOrders}\n" +
            $"Failed: {shift.FailedOrders}\n" +
            $"Total spent: {shift.TotalSpent} glimmer";

        budgetText.text = "";

        TMP_Text buttonText =
            finishShiftButton.GetComponentInChildren<TMP_Text>(true);

        if (buttonText != null)
            buttonText.text = "Main Menu";

        SetExpression(shift.FailedOrders == 0 ? happyFace : sadFace);
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