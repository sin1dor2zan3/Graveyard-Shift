using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider2D))]
public class ComputerMonitorTap : MonoBehaviour
{
    [SerializeField] private ShippingComputerUI computerUI;

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
        if (PauseMenu.IsPaused || computerUI == null)
            return;

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            TryOpen(
                Touchscreen.current.primaryTouch.position.ReadValue());
        }
        else if (Mouse.current != null &&
                 Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryOpen(Mouse.current.position.ReadValue());
        }
    }

    private void TryOpen(Vector2 screenPosition)
    {
        if (mainCamera == null)
            return;

        // Ignore taps on menus, buttons, and the open overlay.
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
        Plane monitorPlane = new Plane(
            Vector3.forward, transform.position);

        if (monitorPlane.Raycast(ray, out float distance) &&
            hitArea.OverlapPoint(ray.GetPoint(distance)))
        {
            computerUI.OpenComputer();
        }
    }
}