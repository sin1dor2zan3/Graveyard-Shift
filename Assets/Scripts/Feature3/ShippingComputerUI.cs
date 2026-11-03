using UnityEngine;

public class ShippingComputerUI : MonoBehaviour
{
    [SerializeField] private GameObject computerOverlay;
    [SerializeField] private GameObject servicesPage;
    [SerializeField] private GameObject labelsPage;
    [SerializeField] private ParcelLabelPrinter labelPrinter;

    private void Start()
    {
        CloseComputer();
    }

    public void OpenComputer()
    {
        OrderSession session = OrderSession.Instance;

        if (PauseMenu.IsPaused ||
            session == null ||
            !session.HasActiveOrder ||
            !session.IsWeighed ||
            !session.IsSealed ||
            session.ShipmentSent ||
            labelPrinter == null ||
            labelPrinter.HasPrintedLabel)
        {
            return;
        }

        bool wasClosed = !computerOverlay.activeSelf;

        computerOverlay.SetActive(true);
        ShowServices();

        if (wasClosed)
            GameSFX.Instance?.ComputerOpen();
    }

    public void ShowServices()
    {
        labelsPage.SetActive(false);
        servicesPage.SetActive(true);
    }

    public void ShowLabels()
    {
        if (PauseMenu.IsPaused || labelPrinter == null)
            return;

        labelPrinter.OpenLabels();

        if (labelsPage.activeSelf)
            servicesPage.SetActive(false);
    }

    public void CloseComputer()
    {
        labelsPage.SetActive(false);
        servicesPage.SetActive(false);
        computerOverlay.SetActive(false);
    }

    private void Update()
    {
        if (computerOverlay.activeSelf &&
            labelPrinter != null &&
            labelPrinter.HasPrintedLabel)
        {
            CloseComputer();
        }
    }
}