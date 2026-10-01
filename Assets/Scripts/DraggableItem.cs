using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider2D))]
public class DraggableItem : MonoBehaviour
{
    [SerializeField] private PackingGrid packingGrid;
    [SerializeField, Min(1)] private int width = 1;
    [SerializeField, Min(1)] private int height = 1;

    [Header("Placement Preview")]
    [SerializeField]
    private Color validColor =
        new Color(0.45f, 0.9f, 0.65f, 1f);

    [SerializeField]
    private Color invalidColor =
        new Color(1f, 0.45f, 0.45f, 1f);

    public int Width => width;
    public int Height => height;
    public static DraggableItem SelectedItem { get; private set; }

    private static DraggableItem heldItem;

    private Camera mainCamera;
    private BoxCollider2D itemCollider;
    private SpriteRenderer spriteRenderer;

    private bool isDragging;
    private bool usingTouch;
    private Vector3 grabOffset;
    private Vector3 positionBeforeDrag;

    private int widthBeforeDrag;
    private int heightBeforeDrag;
    private int originalSortingOrder;
    private int startingWidth;
    private int startingHeight;
    private Color originalColor;

    private void Awake()
    {
        mainCamera = Camera.main;
        itemCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        startingWidth = width;
        startingHeight = height;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void OnEnable()
    {
        SetSize(startingWidth, startingHeight);
        RestoreColor();
    }

    private void Update()
    {
        if (mainCamera == null)
            return;

        if (!isDragging)
        {
            if (Touchscreen.current != null &&
                Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                TryBeginDrag(
                    Touchscreen.current.primaryTouch.position.ReadValue(),
                    true
                );
            }
            else if (Mouse.current != null &&
                     Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryBeginDrag(Mouse.current.position.ReadValue(), false);
            }
        }

        if (!isDragging)
            return;

        Vector2 screenPosition;
        bool pressed;

        if (usingTouch)
        {
            if (Touchscreen.current == null)
            {
                CancelDrag();
                return;
            }

            var touch = Touchscreen.current.primaryTouch;
            screenPosition = touch.position.ReadValue();
            pressed = touch.press.isPressed;

            if (touch.phase.ReadValue() ==
                UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                CancelDrag();
                return;
            }
        }
        else
        {
            if (Mouse.current == null)
            {
                CancelDrag();
                return;
            }

            screenPosition = Mouse.current.position.ReadValue();
            pressed = Mouse.current.leftButton.isPressed;
        }

        if (!usingTouch &&
            Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame &&
            width != height)
        {
            SetSize(height, width);
            grabOffset = new Vector3(-grabOffset.y, grabOffset.x, 0f);
        }

        transform.position = ScreenToWorld(screenPosition) + grabOffset;

        if (spriteRenderer != null)
        {
            bool fits = packingGrid != null &&
                packingGrid.CanPlace(this, transform.position);

            spriteRenderer.color = fits ? validColor : invalidColor;
        }

        if (!pressed)
            FinishDrag();
    }

    private Vector3 ScreenToWorld(Vector2 screenPosition)
    {
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(
            new Vector3(
                screenPosition.x,
                screenPosition.y,
                -mainCamera.transform.position.z
            )
        );

        worldPosition.z = 0f;
        return worldPosition;
    }

    private bool IsOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        var pointer = new PointerEventData(EventSystem.current);
        pointer.position = screenPosition;

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        return results.Count > 0;
    }

    private void TryBeginDrag(Vector2 screenPosition, bool touchInput)
    {
        if (heldItem != null || IsOverUI(screenPosition))
            return;

        Vector3 worldPosition = ScreenToWorld(screenPosition);

        if (!itemCollider.OverlapPoint(worldPosition))
            return;

        heldItem = this;
        SelectedItem = this;
        isDragging = true;
        usingTouch = touchInput;

        positionBeforeDrag = transform.position;
        widthBeforeDrag = width;
        heightBeforeDrag = height;
        grabOffset = transform.position - worldPosition;

        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
            spriteRenderer.sortingOrder = 10;
        }
    }

    public void RotateSelected()
    {
        if (!isActiveAndEnabled || heldItem != null || width == height)
            return;

        int oldWidth = width;
        int oldHeight = height;

        bool wasPacked = packingGrid != null &&
            packingGrid.IsPacked(this);

        SetSize(oldHeight, oldWidth);

        if (!wasPacked)
            return;

        if (packingGrid.TryPlace(
            this,
            transform.position,
            out Vector3 snappedPosition))
        {
            transform.position = snappedPosition;
        }
        else
        {
            // Keep the previous orientation if rotation won't fit.
            SetSize(oldWidth, oldHeight);
        }
    }

    private void SetSize(int newWidth, int newHeight)
    {
        width = newWidth;
        height = newHeight;
        transform.localScale = new Vector3(width, height, 1f);
    }

    private void FinishDrag()
    {
        if (packingGrid != null &&
            packingGrid.TryPlace(
                this,
                transform.position,
                out Vector3 snappedPosition))
        {
            transform.position = snappedPosition;
        }
        else
        {
            RestorePreviousPlacement();
        }

        EndDrag();
    }

    private void RestorePreviousPlacement()
    {
        SetSize(widthBeforeDrag, heightBeforeDrag);
        transform.position = positionBeforeDrag;
    }

    private void RestoreColor()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    private void EndDrag()
    {
        isDragging = false;

        if (heldItem == this)
            heldItem = null;

        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = originalSortingOrder;

        RestoreColor();
    }

    private void CancelDrag()
    {
        RestorePreviousPlacement();
        EndDrag();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && isDragging)
            CancelDrag();
    }

    private void OnDisable()
    {
        if (isDragging)
            CancelDrag();

        if (SelectedItem == this)
            SelectedItem = null;

        RestoreColor();

        if (packingGrid != null)
            packingGrid.RemoveItem(this);
    }
}