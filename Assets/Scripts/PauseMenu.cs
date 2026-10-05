using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [SerializeField] private GameObject pauseOverlay;
    [SerializeField] private GameObject pauseCard;
    [SerializeField] private GameObject controlsCard;
    [SerializeField] private GameObject quitConfirmCard;

    private float previousTimeScale = 1f;
    private bool leavingScene;

    private void Awake()
    {
        IsPaused = false;

        ShowPauseCard();

        if (pauseOverlay != null)
            pauseOverlay.SetActive(false);
    }

    private void Update()
    {
        if (leavingScene)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!IsPaused)
            {
                Pause();
            }
            else if (
                (controlsCard != null && controlsCard.activeSelf) ||
                (quitConfirmCard != null && quitConfirmCard.activeSelf))
            {
                ShowPauseCard();
            }
            else
            {
                Resume();
            }
        }
    }

    public void Pause()
    {
        if (IsPaused || pauseOverlay == null || leavingScene)
            return;

        previousTimeScale = Time.timeScale;
        IsPaused = true;
        Time.timeScale = 0f;

        ShowPauseCard();
        pauseOverlay.SetActive(true);
    }

    public void Resume()
    {
        if (!IsPaused)
            return;

        IsPaused = false;
        Time.timeScale = previousTimeScale;

        if (pauseOverlay != null)
            pauseOverlay.SetActive(false);
    }

    public void ShowControls()
    {
        if (!IsPaused || controlsCard == null)
            return;

        HideCards();
        controlsCard.SetActive(true);
    }

    public void ShowPauseCard()
    {
        HideCards();

        if (pauseCard != null)
            pauseCard.SetActive(true);
    }

    public void ShowQuitConfirmation()
    {
        if (!IsPaused || quitConfirmCard == null)
            return;

        HideCards();
        quitConfirmCard.SetActive(true);
    }

    public void KeepPlaying()
    {
        ShowPauseCard();
        Resume();
    }

    public void ConfirmMainMenu()
    {
        if (!IsPaused || leavingScene)
            return;

        if (!Application.CanStreamedLevelBeLoaded("MainMenu"))
        {
            Debug.LogError(
                "Add MainMenu to the active build profile's scene list."
            );
            return;
        }

        leavingScene = true;

        if (OrderSession.Instance != null)
            OrderSession.Instance.ClearOrder();

        Resume();

        SceneManager.LoadScene("MainMenu");
    }

    private void HideCards()
    {
        if (pauseCard != null)
            pauseCard.SetActive(false);

        if (controlsCard != null)
            controlsCard.SetActive(false);

        if (quitConfirmCard != null)
            quitConfirmCard.SetActive(false);
    }

    private void OnDisable()
    {
        if (IsPaused)
            Resume();
    }
}