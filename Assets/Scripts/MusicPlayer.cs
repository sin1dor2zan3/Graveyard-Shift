using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer instance;
    public static MusicPlayer Instance => instance;
    private AudioSource source;

    [SerializeField] private AudioClip music;
    [SerializeField, Range(0f, 1f)]
    private float volume = 0.3f;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;

        if (instance != null && instance != this)
        {
            source.Stop();
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        source.clip = music;
        source.loop = true;
        source.spatialBlend = 0f;
        source.volume = Mathf.Clamp01(
        PlayerPrefs.GetFloat("Settings.MusicVolume", volume));

        if (music != null)
            source.Play();
        else
            Debug.LogWarning("Assign the music clip to MusicPlayer.", this);
    }

    public void SetVolume(float value)
    {
        value = Mathf.Clamp01(value);
        source.volume = value;

        PlayerPrefs.SetFloat("Settings.MusicVolume", value);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}