using System.Collections;
using TMPro;
using UnityEngine;

public class CustomerStoryUI : MonoBehaviour
{
    [SerializeField] private GameObject openingOverlay;
    [SerializeField] private GameObject customerStoryOverlay;
    [SerializeField] private TMP_Text customerStoryText;

    [Header("Typewriter")]
    [SerializeField, Min(1f)]
    private float charactersPerSecond = 35f;

    private Coroutine typingRoutine;
    private bool isTyping;

    private bool showing;
    private string currentSaveKey;
    private string[] pages;
    private int pageIndex;

    private void Awake()
    {
        if (customerStoryOverlay != null)
            customerStoryOverlay.SetActive(false);
    }

    public IEnumerator ShowForOrder(OrderDefinition order)
    {
        while (openingOverlay != null &&
               openingOverlay.activeInHierarchy)
        {
            yield return null;
        }

        ShiftController shift = ShiftController.Instance;

        if (shift != null &&
            shift.IsThirdShiftSet &&
            shift.CurrentOrderNumber == 1)
        {
            MysteryParcelStory parcelStory =
                FindFirstObjectByType<MysteryParcelStory>();

            if (parcelStory != null)
            {
                bool revealFinished = false;

                bool revealStarted = parcelStory.TryShow(
                    () => revealFinished = true);

                if (revealStarted)
                {
                    while (!revealFinished)
                        yield return null;
                }
            }
        }

        if (order == null ||
            customerStoryOverlay == null ||
            customerStoryText == null ||
            string.IsNullOrWhiteSpace(order.storyId) ||
            string.IsNullOrWhiteSpace(order.firstVisitStory))
        {
            yield break;
        }

        currentSaveKey = SaveSlots.StoryKey("GraveyardShift.Story.Customer." + order.storyId);

        if (PlayerPrefs.GetInt(currentSaveKey, 0) == 1)
            yield break;

        string story = order.firstVisitStory
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

        pages = story.Split(
            new[] { "\n\n" },
            System.StringSplitOptions.RemoveEmptyEntries);

        if (pages.Length == 0)
            yield break;

        pageIndex = 0;
        showing = true;

        customerStoryText.maxVisibleCharacters = 0;
        customerStoryOverlay.SetActive(true);
        customerStoryOverlay.transform.SetAsLastSibling();

        Canvas.ForceUpdateCanvases();
        yield return null;

        ShowPage();

        while (showing)
            yield return null;
    }
    public IEnumerator ShowPages(string[] storyPages)
    {
        if (customerStoryOverlay == null ||
            customerStoryText == null ||
            storyPages == null ||
            storyPages.Length == 0)
        {
            yield break;
        }

        currentSaveKey = null;
        pages = storyPages;
        pageIndex = 0;
        showing = true;

        customerStoryText.maxVisibleCharacters = 0;
        customerStoryOverlay.SetActive(true);
        customerStoryOverlay.transform.SetAsLastSibling();

        Canvas.ForceUpdateCanvases();
        yield return null;

        ShowPage();

        while (showing)
            yield return null;
    }

    public bool HasPageReferences =>
        customerStoryOverlay != null && customerStoryText != null;


    private void ShowPage()
    {
        StopTyping();

        customerStoryText.maxVisibleCharacters = int.MaxValue;
        customerStoryText.text = pages[pageIndex].Trim();
        customerStoryText.ForceMeshUpdate(true);

        int characterCount =
            customerStoryText.textInfo.characterCount;

        customerStoryText.maxVisibleCharacters = 0;

        if (characterCount > 0)
        {
            isTyping = true;
            typingRoutine = StartCoroutine(TypePage(characterCount));
        }
    }

    private IEnumerator TypePage(int characterCount)
    {
        float visibleCharacters = 0f;

        while (customerStoryText.maxVisibleCharacters < characterCount)
        {
            yield return null;

            if (PauseMenu.IsPaused)
                continue;

            visibleCharacters +=
                charactersPerSecond * Time.unscaledDeltaTime;

            customerStoryText.maxVisibleCharacters =
                Mathf.Min(
                    characterCount,
                    Mathf.FloorToInt(visibleCharacters));
        }

        isTyping = false;
        typingRoutine = null;
    }

    private void StopTyping()
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = null;
        isTyping = false;
    }

    private void RevealPage()
    {
        StopTyping();
        customerStoryText.maxVisibleCharacters = int.MaxValue;
    }

    private void OnDisable()
    {
        RevealPage();
    }

    public void ContinueToRequest()
    {
        if (!showing || PauseMenu.IsPaused)
            return;

        GameSFX.Instance?.Click();

        if (isTyping)
        {
            RevealPage();
            return;
        }

        pageIndex++;

        if (pageIndex < pages.Length)
        {
            ShowPage();
            return;
        }

        if (!string.IsNullOrEmpty(currentSaveKey))
        {
            PlayerPrefs.SetInt(currentSaveKey, 1);
            PlayerPrefs.Save();
        }

        customerStoryOverlay.SetActive(false);
        showing = false;
    }

    [ContextMenu("Reset Customer Stories For Testing")]
    private void ResetCustomerStoriesForTesting()
    {
        foreach (OrderDefinition order in
                 Resources.FindObjectsOfTypeAll<OrderDefinition>())
        {
            if (order == null ||
                string.IsNullOrWhiteSpace(order.storyId))
            {
                continue;
            }

            PlayerPrefs.DeleteKey(SaveSlots.StoryKey("GraveyardShift.Story.Customer." + order.storyId));
        }

        PlayerPrefs.Save();

        Debug.Log(
            "Seen flags reset for currently loaded customer orders.");
    }
}