using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class ReceiptDrag : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private CustomerCounterController counter;
    [SerializeField] private RectTransform customerTarget;
    [SerializeField, Range(0.2f, 1f)]
    private float handoffScale = 0.65f;

    private Vector3 originalScale;
    private float startingDistance;

    private RectTransform rect;
    private RectTransform parent;
    private Vector2 home;
    private Vector2 offset;
    private bool dragging;
    private int pointerId;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        parent = transform.parent as RectTransform;
        originalScale = rect.localScale;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OrderSession session = OrderSession.Instance;

        if (PauseMenu.IsPaused || dragging ||
            eventData.button != PointerEventData.InputButton.Left ||
            session == null || !session.ShipmentSent ||
            session.ReceiptGiven || counter == null ||
            customerTarget == null || parent == null)
        {
            return;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, eventData.position,
            eventData.pressEventCamera, out Vector2 pointer))
        {
            return;
        }

        home = rect.anchoredPosition;
        rect.localScale = originalScale;

        Vector2 targetPosition =
            parent.InverseTransformPoint(
                customerTarget.TransformPoint(customerTarget.rect.center));

        startingDistance = Mathf.Max(
            1f,
            Vector2.Distance(rect.localPosition, targetPosition));
        offset = (Vector2)rect.localPosition - pointer;
        pointerId = eventData.pointerId;
        dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || eventData.pointerId != pointerId ||
            PauseMenu.IsPaused)
            return;

        Move(eventData);
    }

    private void Move(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, eventData.position,
            eventData.pressEventCamera, out Vector2 pointer))
        {
            Vector2 position = pointer + offset;

            rect.localPosition = new Vector3(
                position.x, position.y, rect.localPosition.z);

            Vector2 targetPosition = parent.InverseTransformPoint(
        customerTarget.TransformPoint(customerTarget.rect.center));

            float distance =
                Vector2.Distance(rect.localPosition, targetPosition);

            float progress =
                Mathf.Clamp01(1f - distance / startingDistance);

            rect.localScale = originalScale *
                Mathf.Lerp(1f, handoffScale, progress);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging || eventData.pointerId != pointerId)
            return;

        dragging = false;

        if (!PauseMenu.IsPaused)
        {
            Move(eventData);

            Vector2 center = RectTransformUtility.WorldToScreenPoint(
                eventData.pressEventCamera, rect.position);

            if (RectTransformUtility.RectangleContainsScreenPoint(
                customerTarget, center, eventData.pressEventCamera))
            {
                rect.anchoredPosition = home;
                rect.localScale = originalScale;
                counter.HandOverReceipt();
                return;
            }
        }

        rect.anchoredPosition = home;
        rect.localScale = originalScale;
    }

    private void OnDisable()
    {
        if (dragging)
        {
            dragging = false;
            rect.anchoredPosition = home;
            rect.localScale = originalScale;
        }
    }
}