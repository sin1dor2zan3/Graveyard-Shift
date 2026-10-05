using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class ParcelLabelDrag : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private ParcelLabelPrinter labelPrinter;
    [SerializeField] private SpriteRenderer parcelRenderer;
    [SerializeField] private RectTransform attachTarget;

    private RectTransform labelRect;
    private RectTransform parentRect;
    private Camera mainCamera;

    private Vector2 homePosition;
    private Vector2 grabOffset;
    private bool dragging;
    private bool attached;
    private int activePointerId;

    private void Awake()
    {
        labelRect = GetComponent<RectTransform>();
        parentRect = transform.parent as RectTransform;
        mainCamera = Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (attached || dragging ||
            labelPrinter == null ||
            !labelPrinter.HasPrintedLabel ||
            parcelRenderer == null ||
            attachTarget == null ||
            parentRect == null ||
            mainCamera == null)
        {
            return;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pointerPosition))
        {
            return;
        }

        homePosition = labelRect.anchoredPosition;

        // Both the label and its parent use centered coordinates.
        grabOffset = (Vector2)labelRect.localPosition - pointerPosition;

        activePointerId = eventData.pointerId;
        dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || eventData.pointerId != activePointerId)
            return;

        MoveLabel(eventData);
    }

    private void MoveLabel(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pointerPosition))
        {
            Vector2 position = pointerPosition + grabOffset;

            labelRect.localPosition = new Vector3(
                position.x,
                position.y,
                labelRect.localPosition.z
            );
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging || eventData.pointerId != activePointerId)
            return;

        MoveLabel(eventData);
        dragging = false;

        Vector2 screenCenter = RectTransformUtility.WorldToScreenPoint(
            eventData.pressEventCamera,
            labelRect.position
        );

        float distanceToParcel =
            parcelRenderer.transform.position.z -
            mainCamera.transform.position.z;

        Vector3 worldCenter = mainCamera.ScreenToWorldPoint(
            new Vector3(
                screenCenter.x,
                screenCenter.y,
                distanceToParcel
            )
        );

        Bounds parcelBounds = parcelRenderer.bounds;

        bool overParcel =
            worldCenter.x >= parcelBounds.min.x &&
            worldCenter.x <= parcelBounds.max.x &&
            worldCenter.y >= parcelBounds.min.y &&
            worldCenter.y <= parcelBounds.max.y;

        OrderSession session = OrderSession.Instance;

        if (overParcel &&
            session != null &&
            labelPrinter.HasPrintedLabel &&
            session.AttachLabel(session.Recipient, session.Address))
        {
            attached = true;
            labelRect.position = attachTarget.position;
        }
        else
        {
            labelRect.anchoredPosition = homePosition;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            CancelDrag();
    }

    private void OnDisable()
    {
        CancelDrag();
    }

    private void CancelDrag()
    {
        if (!dragging)
            return;

        dragging = false;
        labelRect.anchoredPosition = homePosition;
    }
}