using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class PeanutScoop : MonoBehaviour
{
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite fullSprite;
    [SerializeField] private Sprite largeEmptySprite;
    [SerializeField] private Sprite largeFullSprite;
    [SerializeField] private Collider2D peanutBin;
    [SerializeField] private PackingPeanuts packingPeanuts;

    public bool IsFull { get; private set; }

    private SpriteRenderer artwork;
    private BoxCollider2D scoopCollider;
    private Camera mainCamera;
    private Vector3 restingPosition;
    private Vector3 grabOffset;
    private bool dragging;
    private bool usingTouch;

    private void Awake()
    {
        artwork = GetComponent<SpriteRenderer>();
        scoopCollider = GetComponent<BoxCollider2D>();
        mainCamera = Camera.main;
        restingPosition = transform.position;

        SetFull(false);
    }

    private void Update()
    {
        if (PauseMenu.IsPaused || OrderPopupController.IsOpen)
        {
            ReturnToRest();
            return;
        }

        if (mainCamera == null)
            return;

        if (!dragging)
        {
            if (Touchscreen.current != null &&
                Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                TryPickUp(
                    Touchscreen.current.primaryTouch.position.ReadValue(),
                    true);
            }
            else if (Mouse.current != null &&
                     Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryPickUp(Mouse.current.position.ReadValue(), false);
            }
        }

        if (!dragging)
            return;

        Vector2 pointer;
        bool held;

        if (usingTouch)
        {
            if (Touchscreen.current == null)
            {
                ReturnToRest();
                return;
            }

            var touch = Touchscreen.current.primaryTouch;

            if (touch.phase.ReadValue() ==
                UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                ReturnToRest();
                return;
            }

            pointer = touch.position.ReadValue();
            held = touch.press.isPressed;
        }
        else
        {
            if (Mouse.current == null)
            {
                ReturnToRest();
                return;
            }

            pointer = Mouse.current.position.ReadValue();
            held = Mouse.current.leftButton.isPressed;
        }

        transform.position = ToWorld(pointer) + grabOffset;

        if (!IsFull && peanutBin != null &&
            peanutBin.OverlapPoint(transform.position))
        {
            SetFull(true);
        }

        if (!held)
        {
            if (IsFull && packingPeanuts != null &&
                packingPeanuts.TryDeposit(transform.position))
            {
                Empty();
            }

            ReturnToRest();
        }
    }

    private void TryPickUp(Vector2 pointer, bool touch)
    {
        if (IsOverUI(pointer))
            return;

        Vector3 worldPosition = ToWorld(pointer);

        if (!scoopCollider.OverlapPoint(worldPosition))
            return;

        dragging = true;
        usingTouch = touch;
        grabOffset = transform.position - worldPosition;
    }

    private Vector3 ToWorld(Vector2 pointer)
    {
        Vector3 position = mainCamera.ScreenToWorldPoint(
            new Vector3(pointer.x, pointer.y,
                -mainCamera.transform.position.z));

        position.z = transform.position.z;
        return position;
    }

    private bool IsOverUI(Vector2 pointer)
    {
        if (EventSystem.current == null)
            return false;

        var eventData = new PointerEventData(EventSystem.current)
        {
            position = pointer
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    public void Empty()
    {
        SetFull(false);
    }

    private void SetFull(bool full)
    {
        IsFull = full;

        Sprite selectedSprite;

        if (PlayerProgress.HasLargerScoop)
        {
            selectedSprite = full
                ? largeFullSprite
                : largeEmptySprite;
        }
        else
        {
            selectedSprite = full ? fullSprite : emptySprite;
        }

        // Fall back to the original artwork if large art is unassigned.
        if (selectedSprite == null)
            selectedSprite = full ? fullSprite : emptySprite;

        artwork.sprite = selectedSprite;

        if (selectedSprite != null)
        {
            scoopCollider.size = selectedSprite.bounds.size;
            scoopCollider.offset = selectedSprite.bounds.center;
        }
    }

    private void ReturnToRest()
    {
        if (!dragging)
            return;

        dragging = false;
        transform.position = restingPosition;
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused)
            ReturnToRest();
    }

    private void OnDisable()
    {
        ReturnToRest();
    }
}