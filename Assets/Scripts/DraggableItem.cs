using UnityEngine;
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

            UpdatePreview();

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

    private void UpdatePreview()
    {
        if (spriteRenderer == null)
            return;

        bool fits = packingGrid != null &&
            packingGrid.CanPlace(this, transform.position);

        spriteRenderer.color = fits ? validColor : invalidColor;
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

    private void OnDisable()
    {
        if (isDragging)
        {
            RestorePreviousPlacement();
            EndDrag();
        }

        RestoreColor();

        if (packingGrid != null)
            packingGrid.RemoveItem(this);
    }
}