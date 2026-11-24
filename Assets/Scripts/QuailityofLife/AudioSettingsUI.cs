using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void OnEnable()
    {
        musicSlider.SetValueWithoutNotify(
            PlayerPrefs.GetFloat("Settings.MusicVolume", 0.3f)
        );

        sfxSlider.SetValueWithoutNotify(
            PlayerPrefs.GetFloat("Settings.SFXVolume", 0.2f)
        );

        musicSlider.onValueChanged.AddListener(ChangeMusic);
        sfxSlider.onValueChanged.AddListener(ChangeSFX);
    }

    private void OnDisable()
    {
        musicSlider.onValueChanged.RemoveListener(ChangeMusic);
        sfxSlider.onValueChanged.RemoveListener(ChangeSFX);
    }

    private void ChangeMusic(float value)
    {
        value = Mathf.Clamp01(value);

        PlayerPrefs.SetFloat("Settings.MusicVolume", value);
        PlayerPrefs.Save();

        MusicPlayer.Instance?.SetVolume(value);
        MenuMusicVolume.Instance?.SetVolume(value);
    }

    private void ChangeSFX(float value)
    {
        GameSFX.Instance?.SetVolume(value);
    }
}
