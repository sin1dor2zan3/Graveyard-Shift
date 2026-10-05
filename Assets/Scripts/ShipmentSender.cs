using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShipmentSender : MonoBehaviour
{
    [SerializeField] private Button sendParcelButton;

    private bool changingScene;

    private void Start()
    {
        if (sendParcelButton == null)
        {
            Debug.LogError("ShipmentSender: assign Send Parcel Button.");
            enabled = false;
            return;
        }

        RefreshButton();
    }

    private void Update()
    {
        RefreshButton();
    }

    private void RefreshButton()
    {
        OrderSession session = OrderSession.Instance;

        sendParcelButton.interactable =
            !changingScene &&
            session != null &&
            session.CanSend;
    }

    public void SendParcel()
    {
        if (changingScene)
            return;

        if (!Application.CanStreamedLevelBeLoaded("CustomerCounter"))
        {
            Debug.LogError(
                "Add CustomerCounter to the active build scene list."
            );
            return;
        }

        OrderSession session = OrderSession.Instance;

        if (session == null || !session.TrySendParcel())
            return;

        changingScene = true;
        sendParcelButton.interactable = false;

        SceneManager.LoadScene("CustomerCounter");
    }
}