using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    public void BeginShift()
    {
        if (!Application.CanStreamedLevelBeLoaded("CustomerCounter"))
        {
            Debug.LogError("CustomerCounter is missing from the build.");
            return;
        }

        if (ShiftController.Instance == null)
        {
            Debug.LogError("Add ShiftManager to the MainMenu scene.");
            return;
        }

        if (!ShiftController.Instance.BeginShift())
            return;

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