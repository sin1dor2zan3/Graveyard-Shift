using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PackingGrid))]
public class PackingPeanuts : MonoBehaviour
{
    [SerializeField] private Sprite[] peanutSprites;
    [SerializeField] private SpriteRenderer boxArtwork;
    [SerializeField] private int sortingOrder = 2;

    private PackingGrid grid;
    private int previousSize;

    private readonly Dictionary<Vector2Int, GameObject> clusters =
        new Dictionary<Vector2Int, GameObject>();

    private readonly List<Vector2Int> cellsToRemove =
        new List<Vector2Int>();

    private static readonly Vector2[] positions =
{
    new Vector2(-0.28f, -0.28f),
    new Vector2( 0f,    -0.28f),
    new Vector2( 0.28f, -0.28f),

    new Vector2(-0.28f,  0f),
    new Vector2( 0f,     0f),
    new Vector2( 0.28f,  0f),

    new Vector2(-0.28f,  0.28f),
    new Vector2( 0f,     0.28f),
    new Vector2( 0.28f,  0.28f)
};

    private void Awake()
    {
        grid = GetComponent<PackingGrid>();
        previousSize = grid.GridSize;
    }

    private bool TryDepositCell(Vector3 worldPosition)
    {
        if (PauseMenu.IsPaused || OrderPopupController.IsOpen ||
            peanutSprites == null || peanutSprites.Length == 0)
        {
            return false;
        }

        foreach (Sprite sprite in peanutSprites)
        {
            if (sprite == null)
                return false;
        }

        if (previousSize != grid.GridSize)
        {
            ClearPeanuts();
            previousSize = grid.GridSize;
        }

        if (!grid.TryGetEmptyCell(worldPosition, out Vector2Int cell) ||
            clusters.ContainsKey(cell))
        {
            return false;
        }

        GameObject cluster = new GameObject(
            $"Peanuts_{cell.x}_{cell.y}");

        cluster.transform.SetParent(transform, false);
        cluster.transform.position = grid.GetCellCenter(cell);

        Vector3 parentScale = transform.lossyScale;
        cluster.transform.localScale = new Vector3(
            1f / parentScale.x,
            1f / parentScale.y,
            1f
        );

        foreach (Vector2 position in positions)
        {
            Sprite sprite = peanutSprites[
                Random.Range(0, peanutSprites.Length)];

            GameObject peanut = new GameObject("Peanut");
            peanut.transform.SetParent(cluster.transform, false);

            Vector2 offset = position + new Vector2(
                Random.Range(-0.025f, 0.025f),
                Random.Range(-0.025f, 0.025f)
            );

            peanut.transform.localRotation =
                Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            float size = Random.Range(0.38f, 0.46f);
            Vector3 boundsSize = sprite.bounds.size;
            float scale = size /
                Mathf.Max(boundsSize.x, boundsSize.y);

            peanut.transform.localScale = Vector3.one * scale;

            Vector3 centerOffset =
                peanut.transform.localRotation *
                (sprite.bounds.center * scale);

            peanut.transform.localPosition =
                new Vector3(offset.x, offset.y, 0f) - centerOffset;

            SpriteRenderer renderer =
                peanut.AddComponent<SpriteRenderer>();

            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;

            if (boxArtwork != null)
                renderer.sortingLayerID = boxArtwork.sortingLayerID;
        }

        clusters.Add(cell, cluster);
        return true;
    }

    private void LateUpdate()
    {
        if (previousSize != grid.GridSize)
        {
            ClearPeanuts();
            previousSize = grid.GridSize;
        }

        cellsToRemove.Clear();

        foreach (var entry in clusters)
        {
            if (grid.IsCellOccupied(entry.Key))
                cellsToRemove.Add(entry.Key);
        }

        foreach (Vector2Int cell in cellsToRemove)
        {
            clusters[cell].SetActive(false);
            Destroy(clusters[cell]);
            clusters.Remove(cell);
        }
    }

    public void ClearPeanuts()
    {
        foreach (GameObject cluster in clusters.Values)
        {
            cluster.SetActive(false);
            Destroy(cluster);
        }

        clusters.Clear();
    }
    public int UnprotectedCellCount
    {
        get
        {
            int count = 0;

            for (int x = 0; x < grid.GridSize; x++)
            {
                for (int y = 0; y < grid.GridSize; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);

                    if (!grid.IsCellOccupied(cell) &&
                        !clusters.ContainsKey(cell))
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    }
    public bool TryDeposit(Vector3 worldPosition)
    {
        if (PauseMenu.IsPaused || OrderPopupController.IsOpen ||
            !grid.TryGetEmptyCell(worldPosition, out Vector2Int target))
        {
            return false;
        }

        int radius = PlayerProgress.HasLargerScoop ? 2 : 1;
        bool deposited = false;

        for (int x = target.x - radius;
             x <= target.x + radius; x++)
        {
            for (int y = target.y - radius;
                 y <= target.y + radius; y++)
            {
                // Skip cells outside the box.
                if (x < 0 || x >= grid.GridSize ||
                    y < 0 || y >= grid.GridSize)
                {
                    continue;
                }

                Vector2Int cell = new Vector2Int(x, y);

                if (TryDepositCell(grid.GetCellCenter(cell)))
                    deposited = true;
            }
        }

        return deposited;
    }
    public bool IsProtected(DraggableItem item)
    {
        if (item == null || !grid.IsPacked(item))
            return false;

        Vector2Int[] directions =
        {
        Vector2Int.left,
        Vector2Int.right,
        Vector2Int.up,
        Vector2Int.down
    };

        for (int x = 0; x < grid.GridSize; x++)
        {
            for (int y = 0; y < grid.GridSize; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);

                if (grid.GetItemAt(cell) != item)
                    continue;

                foreach (Vector2Int direction in directions)
                {
                    Vector2Int neighbor = cell + direction;

                    // Interior cells of the same gift need no padding.
                    if (grid.GetItemAt(neighbor) == item)
                        continue;

                    // Walls and other gifts cannot replace cushioning.
                    if (!HasCushioning(neighbor))
                        return false;
                }
            }
        }

        return true;
    }

    public bool HasCushioning(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < grid.GridSize &&
            cell.y >= 0 && cell.y < grid.GridSize &&
            !grid.IsCellOccupied(cell) &&
            clusters.ContainsKey(cell);
    }
}