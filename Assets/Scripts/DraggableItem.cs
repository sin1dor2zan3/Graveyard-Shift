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

    [Header("Artwork")]
    [Tooltip("Assign a SpriteRenderer on a direct child named Artwork.")]
    [SerializeField] private SpriteRenderer artwork;
    [SerializeField, Range(0.1f, 1f)] private float artworkFill = 0.92f;

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

    private int turnsBeforeDrag;
    private int quarterTurns;
    private int originalSortingOrder;
    private int startingWidth;
    private int startingHeight;
    private Color originalColor;

    private void Awake()
    {
        mainCamera = Camera.main;
        itemCollider = GetComponent<BoxCollider2D>();
        SpriteRenderer placeholder = GetComponent<SpriteRenderer>();
        bool validArtwork = artwork != null && artwork.sprite != null &&
            artwork.transform.parent == transform;

        if (!validArtwork && artwork != null)
        {
            Debug.LogWarning("Artwork needs a sprite and must be a direct child of the item.", this);
            artwork = null;
        }

        spriteRenderer = validArtwork ? artwork : placeholder;
        if (validArtwork && placeholder != null)
            placeholder.enabled = false;

        startingWidth = width;
        startingHeight = height;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void OnEnable()
    {
        SetOrientation(0);
        RestoreColor();
    }

    private void Update()
    {
        if (OrderPopupController.IsOpen)
        {
            if (isDragging)
                CancelDrag();

            return;
        }

        if (PauseMenu.IsPaused)
            return;

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
            SetOrientation(quarterTurns + 1);
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
        turnsBeforeDrag = quarterTurns;
        grabOffset = transform.position - worldPosition;

        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
            spriteRenderer.sortingOrder = 10;
        }
    }

    public void RotateSelected()
    {
        if (OrderPopupController.IsOpen)
            return;

        if (PauseMenu.IsPaused || !isActiveAndEnabled || heldItem != null || width == height)
            return;

        int oldTurns = quarterTurns;

        bool wasPacked = packingGrid != null &&
            packingGrid.IsPacked(this);

        SetOrientation(quarterTurns + 1);

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
            SetOrientation(oldTurns);
        }
    }

    private void SetOrientation(int turns)
    {
        quarterTurns = (turns % 4 + 4) % 4;
        bool sideways = quarterTurns % 2 == 1;
        width = sideways ? startingHeight : startingWidth;
        height = sideways ? startingWidth : startingHeight;

        transform.localRotation = Quaternion.identity;
        itemCollider.offset = Vector2.zero;

        if (artwork == null)
        {
            transform.localScale = new Vector3(width, height, 1f);
            itemCollider.size = Vector2.one;
            return;
        }

        transform.localScale = Vector3.one;
        itemCollider.size = new Vector2(width, height);

        Bounds bounds = artwork.sprite.bounds;
        if (bounds.size.x <= 0f || bounds.size.y <= 0f)
            return;

        float fit = Mathf.Min(
            startingWidth / bounds.size.x,
            startingHeight / bounds.size.y
        ) * artworkFill;

        Quaternion rotation = Quaternion.Euler(0f, 0f, quarterTurns * 90f);
        artwork.transform.localRotation = rotation;
        artwork.transform.localScale = new Vector3(fit, fit, 1f);
        Vector3 center = bounds.center;
        center.z = 0f;
        artwork.transform.localPosition = -(rotation * (center * fit));
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
        SetOrientation(turnsBeforeDrag);
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