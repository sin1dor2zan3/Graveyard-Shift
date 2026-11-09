using UnityEngine;

public class StoryOpening : MonoBehaviour
{
    [SerializeField] private GameObject storyOverlay;
    [SerializeField] private StoryTypewriter typewriter;

    private static string SeenKey => SaveSlots.StoryKey(
    "GraveyardShift.Story.OpeningSeen.v1");

    private void Awake()
    {
        if (storyOverlay != null)
        {
            storyOverlay.SetActive(
                PlayerPrefs.GetInt(SeenKey, 0) == 0);
        }
    }

    public void CompleteOpening()
    {
        if (PauseMenu.IsPaused)
            return;

        if (typewriter != null && typewriter.IsTyping)
        {
            GameSFX.Instance?.Click();
            typewriter.Reveal();
            return;
        }
        GameSFX.Instance?.Click();
        PlayerPrefs.SetInt(SeenKey, 1);
        PlayerPrefs.Save();

        if (storyOverlay != null)
            storyOverlay.SetActive(false);
    }

    [ContextMenu("Reset Opening For Testing")]
    private void ResetOpeningForTesting()
    {
        PlayerPrefs.DeleteKey(SeenKey);
        PlayerPrefs.Save();

        Debug.Log("Opening reset. It will appear next time you press Play.");
    }
}