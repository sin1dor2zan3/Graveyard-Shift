using UnityEngine;

public class PackingGrid : MonoBehaviour
{
    private const int GridSize = 5;
    private const float CellSize = 1f;

    private readonly DraggableItem[,] occupiedCells =
        new DraggableItem[GridSize, GridSize];

    public bool TryPlace(
        DraggableItem item,
        Vector3 dropPosition,
        out Vector3 snappedPosition)
    {
        snappedPosition = dropPosition;

        float left = transform.position.x - GridSize * CellSize / 2f;
        float bottom = transform.position.y - GridSize * CellSize / 2f;

        int column = Mathf.RoundToInt(
            (dropPosition.x - left) / CellSize - item.Width / 2f
        );

        int row = Mathf.RoundToInt(
            (dropPosition.y - bottom) / CellSize - item.Height / 2f
        );

        if (column < 0 || row < 0 ||
            column + item.Width > GridSize ||
            row + item.Height > GridSize)
        {
            return false;
        }

        for (int x = column; x < column + item.Width; x++)
        {
            for (int y = row; y < row + item.Height; y++)
            {
                DraggableItem occupant = occupiedCells[x, y];

                if (occupant != null && occupant != item)
                    return false;
            }
        }

        RemoveItem(item);

        for (int x = column; x < column + item.Width; x++)
        {
            for (int y = row; y < row + item.Height; y++)
            {
                occupiedCells[x, y] = item;
            }
        }

        snappedPosition = new Vector3(
            left + (column + item.Width / 2f) * CellSize,
            bottom + (row + item.Height / 2f) * CellSize,
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

    public bool AreNeighbors(DraggableItem first, DraggableItem second)
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