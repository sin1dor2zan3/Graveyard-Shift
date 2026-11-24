using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MenuMusicVolume : MonoBehaviour
{
    public static MenuMusicVolume Instance { get; private set; }

    private AudioSource source;

    private void Awake()
    {
        Instance = this;
        source = GetComponent<AudioSource>();

        SetVolume(PlayerPrefs.GetFloat(
            "Settings.MusicVolume", 0.3f));
    }

    public void SetVolume(float value)
    {
        source.volume = Mathf.Clamp01(value);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}