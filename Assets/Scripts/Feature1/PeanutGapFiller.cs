using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PackingGrid), typeof(PackingPeanuts))]
public class PeanutGapFiller : MonoBehaviour
{
    [SerializeField] private Sprite[] peanutSprites;
    [SerializeField] private SpriteRenderer boxArtwork;
    [SerializeField] private int sortingOrder = 2;

    private PackingGrid grid;
    private PackingPeanuts cushioning;

    private readonly Dictionary<Vector2Int, GameObject> visuals =
        new Dictionary<Vector2Int, GameObject>();

    private readonly HashSet<DraggableItem> paddedItems =
        new HashSet<DraggableItem>();

    private readonly List<Vector2Int> remove =
        new List<Vector2Int>();

    private static readonly Vector2Int[] directions =
    {
        Vector2Int.left, Vector2Int.right,
        Vector2Int.up, Vector2Int.down
    };

    private void Awake()
    {
        grid = GetComponent<PackingGrid>();
        cushioning = GetComponent<PackingPeanuts>();
    }

    private void LateUpdate()
    {
        if (!SpritesReady())
        {
            ClearVisuals();
            return;
        }

        paddedItems.Clear();

        // Find gifts touching actual deposited cushioning.
        for (int x = 0; x < grid.GridSize; x++)
        {
            for (int y = 0; y < grid.GridSize; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                DraggableItem item = grid.GetItemAt(cell);

                if (item == null)
                    continue;

                foreach (Vector2Int direction in directions)
                {
                    if (cushioning.HasCushioning(cell + direction))
                    {
                        paddedItems.Add(item);
                        break;
                    }
                }
            }
        }
        // Spread visual filler through touching gifts.
        // This does not change actual cushioning or protection.
        bool addedItem;

        do
        {
            addedItem = false;

            for (int x = 0; x < grid.GridSize; x++)
            {
                for (int y = 0; y < grid.GridSize; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    DraggableItem item = grid.GetItemAt(cell);

                    if (item == null || paddedItems.Contains(item))
                        continue;

                    foreach (Vector2Int direction in directions)
                    {
                        DraggableItem neighbor =
                            grid.GetItemAt(cell + direction);

                        if (neighbor != null &&
                            paddedItems.Contains(neighbor))
                        {
                            paddedItems.Add(item);
                            addedItem = true;
                            break;
                        }
                    }
                }
            }
        }
        while (addedItem);

        // Remove filler when gifts move or cushioning is cleared.
        remove.Clear();

        foreach (var entry in visuals)
        {
            Vector2Int cell = entry.Key;

            bool inside =
                cell.x >= 0 && cell.x < grid.GridSize &&
                cell.y >= 0 && cell.y < grid.GridSize;

            DraggableItem item = inside
                ? grid.GetItemAt(cell)
                : null;

            if (item == null || !paddedItems.Contains(item))
                remove.Add(cell);
        }

        foreach (Vector2Int cell in remove)
        {
            visuals[cell].SetActive(false);
            Destroy(visuals[cell]);
            visuals.Remove(cell);
        }

        // Fill only the reserved cells of cushioned gifts.
        for (int x = 0; x < grid.GridSize; x++)
        {
            for (int y = 0; y < grid.GridSize; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                DraggableItem item = grid.GetItemAt(cell);

                if (item != null &&
                    paddedItems.Contains(item) &&
                    !visuals.ContainsKey(cell))
                {
                    visuals.Add(cell, CreateCluster(cell));
                }
            }
        }
    }

    private bool SpritesReady()
    {
        if (peanutSprites == null || peanutSprites.Length == 0)
            return false;

        foreach (Sprite sprite in peanutSprites)
        {
            if (sprite == null)
                return false;
        }

        return true;
    }

    private GameObject CreateCluster(Vector2Int cell)
    {
        GameObject cluster = new GameObject("VisualPeanutFiller");
        cluster.transform.SetParent(transform, false);
        cluster.transform.position = grid.GetCellCenter(cell);

        Vector3 parentScale = transform.lossyScale;
        cluster.transform.localScale = new Vector3(
            1f / parentScale.x,
            1f / parentScale.y,
            1f
        );

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Sprite sprite =
                    peanutSprites[Random.Range(0, peanutSprites.Length)];

                GameObject peanut = new GameObject("VisualPeanut");
                peanut.transform.SetParent(cluster.transform, false);

                Quaternion rotation = Quaternion.Euler(
                    0f, 0f, Random.Range(0f, 360f));

                float scale = Random.Range(0.38f, 0.46f) /
                    Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);

                Vector3 offset = new Vector3(
                    x * 0.28f + Random.Range(-0.025f, 0.025f),
                    y * 0.28f + Random.Range(-0.025f, 0.025f),
                    0f
                );

                peanut.transform.localRotation = rotation;
                peanut.transform.localScale = Vector3.one * scale;
                peanut.transform.localPosition =
                    offset - rotation * (sprite.bounds.center * scale);

                SpriteRenderer renderer =
                    peanut.AddComponent<SpriteRenderer>();

                renderer.sprite = sprite;
                renderer.sortingOrder = sortingOrder;

                if (boxArtwork != null)
                    renderer.sortingLayerID = boxArtwork.sortingLayerID;
            }
        }

        return cluster;
    }

    private void OnDisable()
    {
        ClearVisuals();
    }

    private void ClearVisuals()
    {
        foreach (GameObject cluster in visuals.Values)
        {
            if (cluster == null)
                continue;

            cluster.SetActive(false);
            Destroy(cluster);
        }

        visuals.Clear();
    }
}