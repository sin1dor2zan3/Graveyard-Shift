using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class StoryTypewriter : MonoBehaviour
{
    [SerializeField, Min(1f)]
    private float charactersPerSecond = 35f;

    private TMP_Text text;
    private Coroutine typingRoutine;

    public bool IsTyping { get; private set; }

    private bool InstantText =>
        PlayerPrefs.GetInt("Settings.InstantText", 0) == 1;

    private void OnEnable()
    {
        text = GetComponent<TMP_Text>();

        text.maxVisibleCharacters = int.MaxValue;
        text.ForceMeshUpdate(true);

        int characterCount = text.textInfo.characterCount;

        if (InstantText || characterCount == 0)
        {
            IsTyping = false;
            return;
        }

        text.maxVisibleCharacters = 0;
        IsTyping = true;
        typingRoutine = StartCoroutine(TypeText(characterCount));
    }

    private IEnumerator TypeText(int characterCount)
    {
        float visibleCharacters = 0f;

        while (text.maxVisibleCharacters < characterCount)
        {
            yield return null;

            if (PauseMenu.IsPaused)
                continue;

            if (InstantText)
                break;

            float speed = Mathf.Clamp(
                PlayerPrefs.GetFloat(
                    "Settings.TextSpeed", charactersPerSecond),
                10f, 100f
            );

            visibleCharacters += speed * Time.unscaledDeltaTime;

            text.maxVisibleCharacters = Mathf.Min(
                characterCount,
                Mathf.FloorToInt(visibleCharacters));
        }

        text.maxVisibleCharacters = int.MaxValue;
        IsTyping = false;
        typingRoutine = null;
    }

    public void Reveal()
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = null;
        IsTyping = false;

        if (text != null)
            text.maxVisibleCharacters = int.MaxValue;
    }

    private void OnDisable()
    {
        Reveal();
    }
}