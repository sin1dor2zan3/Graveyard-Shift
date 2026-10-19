using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameSFX : MonoBehaviour
{
    public static GameSFX Instance { get; private set; }

    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip upgradePurchase;
    [SerializeField] private AudioClip orderSuccess;
    [SerializeField] private AudioClip orderFailure;

    private AudioSource source;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.volume = 0.2f;
    }

    private void Play(AudioClip clip)
    {
        if (clip != null)
            source.PlayOneShot(clip);
    }

    public void Click() { Play(buttonClick); }
    public void Purchase() { Play(upgradePurchase); }

    public void Result(bool success)
    {
        Play(success ? orderSuccess : orderFailure);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}