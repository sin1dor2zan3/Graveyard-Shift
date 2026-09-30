using UnityEngine;

public class PackingGrid : MonoBehaviour
{
    private const int GridSize = 5;
    private const float CellSize = 1f;

    private readonly DraggableItem[,] occupiedCells =
        new DraggableItem[GridSize, GridSize];

    private Vector2Int GetOrigin(
        DraggableItem item,
        Vector3 position)
    {
        float left = transform.position.x - GridSize * CellSize / 2f;
        float bottom = transform.position.y - GridSize * CellSize / 2f;

        return new Vector2Int(
            Mathf.RoundToInt(
                (position.x - left) / CellSize - item.Width / 2f
            ),
            Mathf.RoundToInt(
                (position.y - bottom) / CellSize - item.Height / 2f
            )
        );
    }

    public bool CanPlace(DraggableItem item, Vector3 position)
    {
        Vector2Int origin = GetOrigin(item, position);

        if (origin.x < 0 || origin.y < 0 ||
            origin.x + item.Width > GridSize ||
            origin.y + item.Height > GridSize)
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
            {
                occupiedCells[x, y] = item;
            }
        }

        float left = transform.position.x - GridSize * CellSize / 2f;
        float bottom = transform.position.y - GridSize * CellSize / 2f;

        snappedPosition = new Vector3(
            left + (origin.x + item.Width / 2f) * CellSize,
            bottom + (origin.y + item.Height / 2f) * CellSize,
            0f
        );

        return true;
    }

    public void RemoveItem(DraggableItem item)
    {
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                if (occupiedCells[x, y] == item)
                    occupiedCells[x, y] = null;
            }
        }
    }

    public bool IsPacked(DraggableItem item)
    {
        if (item == null)
            return false;

        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
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

        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
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
        if (x < 0 || x >= GridSize || y < 0 || y >= GridSize)
            return false;

        return occupiedCells[x, y] == item;
    }
}