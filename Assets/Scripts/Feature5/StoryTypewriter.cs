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

    private void OnEnable()
    {
        text = GetComponent<TMP_Text>();

        text.maxVisibleCharacters = int.MaxValue;
        text.ForceMeshUpdate(true);

        int characterCount = text.textInfo.characterCount;
        text.maxVisibleCharacters = 0;

        if (characterCount > 0)
        {
            IsTyping = true;
            typingRoutine = StartCoroutine(TypeText(characterCount));
        }
    }

    private IEnumerator TypeText(int characterCount)
    {
        float visibleCharacters = 0f;

        while (text.maxVisibleCharacters < characterCount)
        {
            yield return null;

            if (PauseMenu.IsPaused)
                continue;

            visibleCharacters +=
                charactersPerSecond * Time.unscaledDeltaTime;

            text.maxVisibleCharacters = Mathf.Min(
                characterCount,
                Mathf.FloorToInt(visibleCharacters));
        }

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