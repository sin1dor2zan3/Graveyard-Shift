using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    public void BeginShift()
    {
        OrderSession.GetOrCreate().StartFirstOrder();

        SceneManager.LoadScene("CustomerCounter");
    }

    public void AcceptParcel()
    {
        OrderSession session = OrderSession.GetOrCreate();

        if (!session.HasActiveOrder)
            session.StartFirstOrder();

        SceneManager.LoadScene("PackingRoom");
    }

    public void ReturnToMenu()
    {
        OrderSession.GetOrCreate().ClearOrder();

        SceneManager.LoadScene("MainMenu");
    }
}