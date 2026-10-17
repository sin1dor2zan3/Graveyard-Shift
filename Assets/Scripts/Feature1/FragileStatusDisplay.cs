using UnityEngine;

[RequireComponent(typeof(DraggableItem))]
public class FragileStatusDisplay : MonoBehaviour
{
    [SerializeField] private Sprite needsProtectionSprite;
    [SerializeField] private Sprite protectedSprite;
    [SerializeField, Min(0.1f)] private float iconSize = 0.45f;

    private DraggableItem item;
    private PackingPeanuts peanuts;
    private SpriteRenderer icon;

    private void Start()
    {
        item = GetComponent<DraggableItem>();

        peanuts = FindFirstObjectByType<PackingPeanuts>(
            FindObjectsInactive.Include);

        GameObject iconObject = new GameObject("ProtectionStatus");
        iconObject.transform.SetParent(transform, false);

        icon = iconObject.AddComponent<SpriteRenderer>();
        icon.sortingOrder = 8;

        SpriteRenderer artwork =
            GetComponentInChildren<SpriteRenderer>(true);

        if (artwork != null)
            icon.sortingLayerID = artwork.sortingLayerID;
    }

    private void LateUpdate()
    {
        if (icon == null)
            return;

        bool fragile = item.Definition != null &&
            item.Definition.isFragile;

        icon.enabled = fragile;

        if (!fragile)
            return;

        bool protectedNow = peanuts != null &&
            peanuts.IsProtected(item);

        icon.sprite = protectedNow
            ? protectedSprite
            : needsProtectionSprite;

        if (icon.sprite == null)
            return;

        Vector3 size = icon.sprite.bounds.size;
        float longestSide = Mathf.Max(size.x, size.y);

        if (longestSide <= 0f)
            return;

        float scale = iconSize / longestSide;
        icon.transform.localScale = Vector3.one * scale;

        Vector3 position = new Vector3(
            0f, item.Height / 2f + 0.35f, 0f);

        icon.transform.localPosition =
            position - icon.sprite.bounds.center * scale;
    }
}