using UnityEngine;

[CreateAssetMenu(
    fileName = "NewItem",
    menuName = "Graveyard Shift/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Identity")]
    public string itemName;

    [Header("Artwork")]
    public Sprite intactSprite;
    public Sprite damagedSprite;

    [Header("Packing")]
    [Min(1)] public int width = 1;
    [Min(1)] public int height = 1;
    [Tooltip("Enable for a left-and-bottom-edge L footprint.")]
    public bool isLShaped;
    [Min(0)] public int weightGrams = 100;

    [Header("Protection")]
    public bool isFragile;
}