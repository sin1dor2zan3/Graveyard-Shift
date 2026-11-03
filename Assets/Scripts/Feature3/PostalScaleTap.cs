using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider2D))]
public class PostalScaleTap : MonoBehaviour
{
    [SerializeField] private ParcelScale parcelScale;

    private Camera mainCamera;
    private BoxCollider2D hitArea;

    private readonly List<RaycastResult> uiHits =
        new List<RaycastResult>();

    private void Awake()
    {
        mainCamera = Camera.main;
        hitArea = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (PauseMenu.IsPaused ||
            OrderPopupController.IsOpen ||
            parcelScale == null)
        {
            return;
        }

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            TryWeigh(
                Touchscreen.current.primaryTouch.position.ReadValue());
        }
        else if (Mouse.current != null &&
                 Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryWeigh(Mouse.current.position.ReadValue());
        }
    }

    private void TryWeigh(Vector2 screenPosition)
    {
        OrderSession session = OrderSession.Instance;

        if (session == null ||
            !session.HasActiveOrder ||
            !session.PackingApproved ||
            session.IsWeighed ||
            session.ShipmentSent ||
            mainCamera == null)
        {
            return;
        }

        // Don't weigh through menus or other UI.
        if (EventSystem.current != null)
        {
            var pointer = new PointerEventData(EventSystem.current)
            {
                position = screenPosition
            };

            uiHits.Clear();
            EventSystem.current.RaycastAll(pointer, uiHits);

            if (uiHits.Count > 0)
                return;
        }

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        Plane scalePlane = new Plane(
            Vector3.forward, transform.position);

        if (scalePlane.Raycast(ray, out float distance) &&
            hitArea.OverlapPoint(ray.GetPoint(distance)))
        {
            parcelScale.Weigh();
        }
    }
}