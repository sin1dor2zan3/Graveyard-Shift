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

    [Header("First Visit Story")]
    public string storyId;

    [TextArea(3, 8)]
    public string firstVisitStory;

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

    [System.Serializable]
    public class SeparationRule
    {
        public ItemDefinition first;
        public ItemDefinition second;
    }

    [Header("Items That Must Stay Apart")]
    public SeparationRule[] separationRules;

    [Header("Delivery Requirements")]
    public bool requiresExpress;
    public bool requiresWard;
}