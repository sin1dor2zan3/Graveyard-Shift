using UnityEngine;

public static class SaveSlots
{
    public const int SlotCount = 3;

    public static int ActiveSlot { get; private set; } = 1;

    public static bool SelectSlot(int slot)
    {
        if (slot < 1 || slot > SlotCount)
        {
            Debug.LogError("Save slot must be 1, 2, or 3.");
            return false;
        }

        ActiveSlot = slot;
        return true;
    }

    public static string Key(string originalKey)
    {
        return KeyForSlot(ActiveSlot, originalKey);
    }

    public static string KeyForSlot(int slot, string originalKey)
    {
        return $"GraveyardShift.Slot{slot}.{originalKey}";
    }
    public static string StoryKey(string originalKey)
    {
        string migrationKey =
            "GraveyardShift.StoryImported.v1." + originalKey;

        // Preserve old story progress in Slot 1 only.
        if (PlayerPrefs.GetInt(migrationKey, 0) == 0)
        {
            string slotOneKey = KeyForSlot(1, originalKey);

            if (PlayerPrefs.HasKey(originalKey) &&
                !PlayerPrefs.HasKey(slotOneKey))
            {
                PlayerPrefs.SetInt(
                    slotOneKey,
                    PlayerPrefs.GetInt(originalKey, 0));
            }

            PlayerPrefs.SetInt(migrationKey, 1);
            PlayerPrefs.Save();
        }

        return Key(originalKey);
    }
}
