using System.Collections;
using TMPro;
using UnityEngine;

public class CustomerStoryUI : MonoBehaviour
{
    [SerializeField] private GameObject openingOverlay;
    [SerializeField] private GameObject customerStoryOverlay;
    [SerializeField] private TMP_Text customerStoryText;

    private bool showing;
    private string currentSaveKey;

    private void Awake()
    {
        if (customerStoryOverlay != null)
            customerStoryOverlay.SetActive(false);
    }

    public IEnumerator ShowForOrder(OrderDefinition order)
    {
        // Let the welcome note finish before introducing a customer.
        while (openingOverlay != null &&
               openingOverlay.activeInHierarchy)
        {
            yield return null;
        }

        if (order == null ||
            customerStoryOverlay == null ||
            customerStoryText == null ||
            string.IsNullOrWhiteSpace(order.storyId) ||
            string.IsNullOrWhiteSpace(order.firstVisitStory))
        {
            yield break;
        }

        currentSaveKey =
            "GraveyardShift.Story.Customer." + order.storyId;

        if (PlayerPrefs.GetInt(currentSaveKey, 0) == 1)
            yield break;

        customerStoryText.text = order.firstVisitStory;
        showing = true;

        customerStoryOverlay.SetActive(true);
        customerStoryOverlay.transform.SetAsLastSibling();

        while (showing)
            yield return null;
    }

    public void ContinueToRequest()
    {
        if (!showing)
            return;

        GameSFX.Instance?.Click();

        PlayerPrefs.SetInt(currentSaveKey, 1);
        PlayerPrefs.Save();

        customerStoryOverlay.SetActive(false);
        showing = false;
    }
}