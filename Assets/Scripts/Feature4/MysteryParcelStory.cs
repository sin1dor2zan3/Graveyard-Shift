using System;
using UnityEngine;

public class MysteryParcelStory : MonoBehaviour
{
    [SerializeField] private GameObject parcelOverlay;

    private const string SeenKey =
        "GraveyardShift.Story.ParcelSeen.v1";

    private Action afterReveal;
    private bool showing;

    private void Awake()
    {
        if (parcelOverlay != null)
            parcelOverlay.SetActive(false);
    }

    public bool TryShow(Action onComplete)
    {
        if (parcelOverlay == null ||
            PlayerPrefs.GetInt(SeenKey, 0) == 1)
        {
            return false;
        }

        if (showing)
            return true;

        afterReveal = onComplete;
        showing = true;
        parcelOverlay.SetActive(true);
        parcelOverlay.transform.SetAsLastSibling();
        GameSFX.Instance?.MagicalProtection();
        return true;
    }

    public void ContinueToShop()
    {
        if (!showing)
            return;

        PlayerPrefs.SetInt(SeenKey, 1);
        PlayerPrefs.Save();

        parcelOverlay.SetActive(false);
        showing = false;

        Action next = afterReveal;
        afterReveal = null;
        next?.Invoke();
    }

    [ContextMenu("Reset Parcel Reveal For Testing")]
    private void ResetReveal()
    {
        PlayerPrefs.DeleteKey(SeenKey);
        PlayerPrefs.Save();
        Debug.Log("Parcel reveal reset for the next shift summary.");
    }
}
