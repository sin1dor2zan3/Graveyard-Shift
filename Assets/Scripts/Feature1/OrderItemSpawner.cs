using System.Collections.Generic;
using UnityEngine;

public class OrderItemSpawner : MonoBehaviour
{
    [SerializeField] private PackingGrid packingGrid;
    [SerializeField] private DraggableItem[] itemPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    public List<DraggableItem> SpawnedItems { get; } =
        new List<DraggableItem>();

    public void ResetItems()
    {
        for (int i = 0; i < SpawnedItems.Count; i++)
        {
            DraggableItem item = SpawnedItems[i];

            if (item == null)
                continue;

            item.enabled = false;
            packingGrid.RemoveItem(item);

            Vector3 position = spawnPoints[i].position;
            position.z = 0f;
            item.transform.position = position;

            item.enabled = true;
        }
    }
    private void Start()
    {
        OrderSession session = OrderSession.Instance;

        if (session == null || session.CurrentOrder == null)
        {
            Debug.LogError("Start from MainMenu to load an order.");
            return;
        }

        ItemDefinition[] items = session.CurrentOrder.items;

        if (packingGrid == null || itemPrefabs == null ||
            spawnPoints == null || spawnPoints.Length < items.Length)
        {
            Debug.LogError("Assign the grid, prefabs, and enough spawn points.");
            return;
        }

        // Check every reference before spawning anything.
        DraggableItem[] selectedPrefabs =
            new DraggableItem[items.Length];

        for (int i = 0; i < items.Length; i++)
        {
            if (spawnPoints[i] == null || items[i] == null)
            {
                Debug.LogError("An item or spawn point is missing.");
                return;
            }

            selectedPrefabs[i] = FindPrefab(items[i]);

            if (selectedPrefabs[i] == null)
            {
                Debug.LogError(
                    $"No prefab assigned for {items[i].itemName}.");
                return;
            }
        }

        for (int i = 0; i < items.Length; i++)
        {
            Vector3 position = spawnPoints[i].position;
            position.z = 0f;

            DraggableItem item = Instantiate(
                selectedPrefabs[i],
                position,
                Quaternion.identity
            );

            item.AssignGrid(packingGrid);
            SpawnedItems.Add(item);
        }
    }

    private DraggableItem FindPrefab(ItemDefinition definition)
    {
        foreach (DraggableItem prefab in itemPrefabs)
        {
            if (prefab != null && prefab.Definition == definition)
                return prefab;
        }

        return null;
    }
}