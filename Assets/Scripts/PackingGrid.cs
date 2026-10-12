using UnityEngine;

public class PackingGrid : MonoBehaviour
{
    private int gridSize = 5;
    private DraggableItem[,] occupiedCells =
        new DraggableItem[5, 5];

    public void SetBoxSize(int newSize)
    {
        gridSize = Mathf.Clamp(newSize, 3, 5);
        occupiedCells = new DraggableItem[gridSize, gridSize];

        transform.localScale = new Vector3(gridSize, gridSize, 1f);

        RebuildGridLines();
    }

    private void RebuildGridLines()
    {
        Transform oldLines = transform.Find("GridLines");

        if (oldLines != null)
        {
            oldLines.gameObject.SetActive(false);
            Destroy(oldLines.gameObject);
        }

        GameObject lines = new GameObject("GridLines");
        lines.transform.SetParent(transform, false);
        lines.SetActive(showGridLines);

        SpriteRenderer boxRenderer = GetComponent<SpriteRenderer>();

        if (boxRenderer == null || boxRenderer.sprite == null)
            return;

        for (int i = 1; i < gridSize; i++)
        {
            float position = -0.5f + (float)i / gridSize;
            float thickness = 0.03f / gridSize;

            CreateLine(
                lines.transform,
                boxRenderer,
                new Vector3(position, 0f, 0f),
                new Vector3(thickness, 1f, 1f)
            );

            CreateLine(
                lines.transform,
                boxRenderer,
                new Vector3(0f, position, 0f),
                new Vector3(1f, thickness, 1f)
            );
        }
    }

    private void CreateLine(
        Transform parent,
        SpriteRenderer boxRenderer,
        Vector3 position,
        Vector3 scale)
    {
        GameObject line = new GameObject("GridLine");
        line.transform.SetParent(parent, false);
        line.transform.localPosition = position;
        line.transform.localScale = scale;

        SpriteRenderer renderer = line.AddComponent<SpriteRenderer>();
        renderer.sprite = boxRenderer.sprite;
        renderer.color = new Color(0.42f, 0.29f, 0.17f, 1f);
        renderer.sortingLayerID = boxRenderer.sortingLayerID;
        renderer.sortingLayerID = boxArtwork != null
            ? boxArtwork.sortingLayerID
            : boxRenderer.sortingLayerID;

        renderer.sortingOrder = boxArtwork != null
            ? boxArtwork.sortingOrder + 1
            : boxRenderer.sortingOrder + 1;
    }

    private Vector2Int GetOrigin(
        DraggableItem item,
        Vector3 position)
    {
        float left = transform.position.x - gridSize / 2f;
        float bottom = transform.position.y - gridSize / 2f;

        return new Vector2Int(
            Mathf.RoundToInt(position.x - left - item.Width / 2f),
            Mathf.RoundToInt(position.y - bottom - item.Height / 2f)
        );
    }

    public bool CanPlace(DraggableItem item, Vector3 position)
    {
        if (!isActiveAndEnabled || item == null)
            return false;

        Vector2Int origin = GetOrigin(item, position);

        if (origin.x < 0 || origin.y < 0 ||
            origin.x + item.Width > gridSize ||
            origin.y + item.Height > gridSize)
        {
            return false;
        }

        for (int x = origin.x; x < origin.x + item.Width; x++)
        {
            for (int y = origin.y; y < origin.y + item.Height; y++)
            {
                DraggableItem occupant = occupiedCells[x, y];

                if (occupant != null && occupant != item)
                    return false;
            }
        }

        return true;
    }

    public bool TryPlace(
        DraggableItem item,
        Vector3 dropPosition,
        out Vector3 snappedPosition)
    {
        snappedPosition = dropPosition;

        if (!CanPlace(item, dropPosition))
            return false;

        Vector2Int origin = GetOrigin(item, dropPosition);
        RemoveItem(item);

        for (int x = origin.x; x < origin.x + item.Width; x++)
        {
            for (int y = origin.y; y < origin.y + item.Height; y++)
                occupiedCells[x, y] = item;
        }

        float left = transform.position.x - gridSize / 2f;
        float bottom = transform.position.y - gridSize / 2f;

        snappedPosition = new Vector3(
            left + origin.x + item.Width / 2f,
            bottom + origin.y + item.Height / 2f,
            0f
        );

        return true;
    }

    public void RemoveItem(DraggableItem item)
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (occupiedCells[x, y] == item)
                    occupiedCells[x, y] = null;
            }
        }
    }

    public bool IsPacked(DraggableItem item)
    {
        if (!isActiveAndEnabled || item == null)
            return false;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (occupiedCells[x, y] == item)
                    return true;
            }
        }

        return false;
    }

    public bool AreNeighbors(
        DraggableItem first,
        DraggableItem second)
    {
        if (first == null || second == null || first == second)
            return false;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (occupiedCells[x, y] != first)
                    continue;

                if (CellContains(x - 1, y, second) ||
                    CellContains(x + 1, y, second) ||
                    CellContains(x, y - 1, second) ||
                    CellContains(x, y + 1, second))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool CellContains(int x, int y, DraggableItem item)
    {
        if (x < 0 || x >= gridSize || y < 0 || y >= gridSize)
            return false;

        return occupiedCells[x, y] == item;
    }

    public int GridSize => gridSize;

    public bool TryGetEmptyCell(Vector3 worldPosition, out Vector2Int cell)
    {
        float left = transform.position.x - gridSize / 2f;
        float bottom = transform.position.y - gridSize / 2f;

        cell = new Vector2Int(
            Mathf.FloorToInt(worldPosition.x - left),
            Mathf.FloorToInt(worldPosition.y - bottom)
        );

        if (!isActiveAndEnabled ||
            cell.x < 0 || cell.x >= gridSize ||
            cell.y < 0 || cell.y >= gridSize)
        {
            return false;
        }

        return occupiedCells[cell.x, cell.y] == null;
    }

    public Vector3 GetCellCenter(Vector2Int cell)
    {
        return new Vector3(
            transform.position.x - gridSize / 2f + cell.x + 0.5f,
            transform.position.y - gridSize / 2f + cell.y + 0.5f,
            0f
        );
    }

    public bool IsCellOccupied(Vector2Int cell)
    {
        if (cell.x < 0 || cell.x >= gridSize ||
            cell.y < 0 || cell.y >= gridSize)
        {
            return true;
        }

        return occupiedCells[cell.x, cell.y] != null;
    }
    [SerializeField] private SpriteRenderer boxArtwork;

    private bool showGridLines;

    private void Awake()
    {
        showGridLines = PlayerPrefs.GetInt("ShowPackingGrid", 0) == 1;
    }

    public void ToggleGridLines()
    {
        showGridLines = !showGridLines;

        PlayerPrefs.SetInt("ShowPackingGrid", showGridLines ? 1 : 0);
        PlayerPrefs.Save();

        Transform lines = transform.Find("GridLines");

        if (lines != null)
            lines.gameObject.SetActive(showGridLines);
    }
}