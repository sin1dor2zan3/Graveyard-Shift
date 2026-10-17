using UnityEngine;

[CreateAssetMenu(
    fileName = "NewOrder",
    menuName = "Graveyard Shift/Order Definition")]
public class OrderDefinition : ScriptableObject
{
    [Header("Customer")]
    public string customerName;

    [TextArea(3, 6)]
    public string request;

    [Header("Destination")]
    public string recipient;
    public string address;

    [Header("Budget")]
    [Min(0)] public int budget = 12;

    [Header("Gifts")]
    public ItemDefinition[] items;

    [Header("Optional Adjacency Rule")]
    public ItemDefinition mustTouchFirst;
    public ItemDefinition mustTouchSecond;
}