using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    public void BeginShift()
    {
        SceneManager.LoadScene("CustomerCounter");
    }

    public void AcceptParcel()
    {
        SceneManager.LoadScene("PackingRoom");
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
