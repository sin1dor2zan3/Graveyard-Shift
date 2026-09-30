using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider2D))]
public class DraggableItem : MonoBehaviour
{
    [SerializeField] private PackingGrid packingGrid;

    [SerializeField, Min(1)] private int width = 1;
    [SerializeField, Min(1)] private int height = 1;

    public int Width => width;
    public int Height => height;

    private static DraggableItem heldItem;

    private Camera mainCamera;
    private BoxCollider2D itemCollider;
    private SpriteRenderer spriteRenderer;

    private bool isDragging;
    private Vector3 grabOffset;
    private Vector3 positionBeforeDrag;

    private int widthBeforeDrag;
    private int heightBeforeDrag;
    private int originalSortingOrder;

    private int startingWidth;
    private int startingHeight;

    private void Awake()
    {
        mainCamera = Camera.main;
        itemCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        startingWidth = width;
        startingHeight = height;
    }

    private void OnEnable()
    {
        SetSize(startingWidth, startingHeight);
    }

    private void Update()
    {
        if (Mouse.current == null || mainCamera == null)
            return;

        Vector2 screenPosition = Mouse.current.position.ReadValue();

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(
            new Vector3(
                screenPosition.x,
                screenPosition.y,
                -mainCamera.transform.position.z
            )
        );

        mousePosition.z = 0f;

        if (Mouse.current.leftButton.wasPressedThisFrame &&
            heldItem == null &&
            itemCollider.OverlapPoint(mousePosition))
        {
            BeginDrag(mousePosition);
        }

        if (isDragging)
        {
            if (Keyboard.current != null &&
                Keyboard.current.rKey.wasPressedThisFrame)
            {
                RotateItem();
            }

            transform.position = mousePosition + grabOffset;

            if (!Mouse.current.leftButton.isPressed)
            {
                FinishDrag();
            }
        }
    }

    private void BeginDrag(Vector3 mousePosition)
    {
        heldItem = this;
        isDragging = true;

        positionBeforeDrag = transform.position;
        widthBeforeDrag = width;
        heightBeforeDrag = height;

        grabOffset = transform.position - mousePosition;

        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
            spriteRenderer.sortingOrder = 10;
        }
    }

    private void RotateItem()
    {
        if (width == height)
            return;

        SetSize(height, width);

        grabOffset = new Vector3(
            -grabOffset.y,
            grabOffset.x,
            0f
        );
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

    private void EndDrag()
    {
        isDragging = false;

        if (heldItem == this)
        {
            heldItem = null;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
        }
    }

    private void OnDisable()
    {
        if (isDragging)
        {
            RestorePreviousPlacement();
            EndDrag();
        }

        if (packingGrid != null)
        {
            packingGrid.RemoveItem(this);
        }
    }
}