using UnityEngine;
using UnityEngine.UI;

public class TextSettingsUI : MonoBehaviour
{
    [SerializeField] private Slider textSpeedSlider;
    [SerializeField] private Toggle instantTextToggle;

    private void OnEnable()
    {
        textSpeedSlider.SetValueWithoutNotify(
            Mathf.Clamp(
                PlayerPrefs.GetFloat("Settings.TextSpeed", 35f),
                10f, 100f)
        );

        instantTextToggle.SetIsOnWithoutNotify(
            PlayerPrefs.GetInt("Settings.InstantText", 0) == 1
        );

        textSpeedSlider.interactable = !instantTextToggle.isOn;

        textSpeedSlider.onValueChanged.AddListener(ChangeSpeed);
        instantTextToggle.onValueChanged.AddListener(ChangeInstant);
    }

    private void OnDisable()
    {
        textSpeedSlider.onValueChanged.RemoveListener(ChangeSpeed);
        instantTextToggle.onValueChanged.RemoveListener(ChangeInstant);
    }

    private void ChangeSpeed(float value)
    {
        PlayerPrefs.SetFloat("Settings.TextSpeed", value);
        PlayerPrefs.Save();
    }

    private void ChangeInstant(bool enabled)
    {
        PlayerPrefs.SetInt("Settings.InstantText", enabled ? 1 : 0);
        PlayerPrefs.Save();

        textSpeedSlider.interactable = !enabled;
    }
}