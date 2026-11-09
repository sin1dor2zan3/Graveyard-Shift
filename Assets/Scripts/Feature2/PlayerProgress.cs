using UnityEngine;

public static class PlayerProgress
{
    private const string LegacySaveKey = "GraveyardShift.Progress.v1";

    private static string SaveKey => SaveSlots.Key(LegacySaveKey);

    private static int loadedSlot = -1;

    private static void ImportLegacyProgress()
    {
        const string migrationKey =
            "GraveyardShift.LegacyProgressImported.v1";

        if (PlayerPrefs.GetInt(migrationKey, 0) == 1)
            return;

        string slotOneKey =
            SaveSlots.KeyForSlot(1, LegacySaveKey);

        if (PlayerPrefs.HasKey(LegacySaveKey) &&
            !PlayerPrefs.HasKey(slotOneKey))
        {
            PlayerPrefs.SetString(
                slotOneKey,
                PlayerPrefs.GetString(LegacySaveKey));
        }

        PlayerPrefs.SetInt(migrationKey, 1);
        PlayerPrefs.Save();
    }

    public static void RecordOrderResult(bool succeeded, ShiftCheckpoint checkpoint = null)
    {
        if (succeeded)
        {
            Data.successfulOrders++;
            Data.wallet += SuccessfulOrderPay;
        }
        else
        {
            Data.failedOrders++;
        }

        if (checkpoint != null)
        {
            Data.hasCheckpoint = true;
            Data.checkpoint =
                JsonUtility.FromJson<ShiftCheckpoint>(
                    JsonUtility.ToJson(checkpoint));

            if (checkpoint.ordersFinished)
            {
                Data.completedShifts = Mathf.Max(
                    Data.completedShifts,
                    checkpoint.nightNumber);
            }
        }

        Save();
    }

    [System.Serializable]
    public class ShiftCheckpoint
    {
        public int nightNumber;
        public int customerSet;

        public string[] orderNames;
        public int[] orderBudgets;

        // Index of the next uncompleted order.
        public int nextOrderIndex;

        public int successfulOrders;
        public int failedOrders;
        public int totalSpent;
        public int shiftEarnings;

        // All orders finished; closing dialogue/shop may remain.
        public bool ordersFinished;
    }
    [System.Serializable]
    private class SaveData
    {
        public bool hasCheckpoint;
        public int wallet;
        public int completedShifts;
        public bool largerScoop;
        public bool cheaperBoxes;
        public bool cheaperCushioning;
        public int successfulOrders;
        public int failedOrders;
        public ShiftCheckpoint checkpoint;
    }
    public static ShiftCheckpoint GetCheckpoint()
    {
        if (!Data.hasCheckpoint || Data.checkpoint == null)
            return null;

        return JsonUtility.FromJson<ShiftCheckpoint>(
            JsonUtility.ToJson(Data.checkpoint));
    }

    public static void SaveCheckpoint(ShiftCheckpoint checkpoint)
    {
        Data.checkpoint = checkpoint == null
            ? null
            : JsonUtility.FromJson<ShiftCheckpoint>(
                JsonUtility.ToJson(checkpoint));
        Data.hasCheckpoint = checkpoint != null;
        Save();
    }
    private static SaveData data;

    private static SaveData Data
    {
        get
        {
            if (data == null || loadedSlot != SaveSlots.ActiveSlot)
            {
                ImportLegacyProgress();

                data = null;
                loadedSlot = SaveSlots.ActiveSlot;

                string json = PlayerPrefs.GetString(SaveKey, "");

                try
                {
                    data = string.IsNullOrEmpty(json)
                        ? new SaveData()
                        : JsonUtility.FromJson<SaveData>(json);
                }
                catch (System.ArgumentException)
                {
                    Debug.LogWarning("Could not read progression save.");
                    data = new SaveData();
                }

                if (data == null)
                    data = new SaveData();
            }

            return data;
        }
    }

    public static int Wallet => Data.wallet;
    public static int CompletedShifts => Data.completedShifts;
    public static int SuccessfulOrders => Data.successfulOrders;
    public static int FailedOrders => Data.failedOrders;

    public static int TotalOrders =>
        Data.successfulOrders + Data.failedOrders;
    public static int CurrentNight => CompletedShifts + 1;

    public static bool HasLargerScoop => Data.largerScoop;
    public static bool HasCheaperBoxes => Data.cheaperBoxes;
    public static bool HasCheaperCushioning => Data.cheaperCushioning;

    public const int SuccessfulOrderPay = 5;
    public const int LargerScoopPrice = 10;
    public const int CheaperBoxesPrice = 15;
    public const int CheaperCushioningPrice = 10;

    public static void AddEarnings(int amount)
    {
        if (amount <= 0)
            return;

        Data.wallet += amount;
        Save();
    }

    public static void CompleteShift()
    {
        Data.completedShifts++;
        Save();
    }

    public static bool BuyLargerScoop()
    {
        if (Data.largerScoop || Data.wallet < LargerScoopPrice)
            return false;

        Data.wallet -= LargerScoopPrice;
        Data.largerScoop = true;
        Save();
        return true;
    }

    public static bool BuyCheaperBoxes()
    {
        if (Data.cheaperBoxes || Data.wallet < CheaperBoxesPrice)
            return false;

        Data.wallet -= CheaperBoxesPrice;
        Data.cheaperBoxes = true;
        Save();
        return true;
    }

    public static bool BuyCheaperCushioning()
    {
        if (Data.cheaperCushioning ||
            Data.wallet < CheaperCushioningPrice)
        {
            return false;
        }

        Data.wallet -= CheaperCushioningPrice;
        Data.cheaperCushioning = true;
        Save();
        return true;
    }

    public static int GetBoxPrice(int basePrice)
    {
        return Mathf.Max(
            0, basePrice - (HasCheaperBoxes ? 1 : 0));
    }

    public static int GetCushioningPrice(int basePrice)
    {
        return Mathf.Max(
            0, basePrice - (HasCheaperCushioning ? 1 : 0));
    }

    private static void Save()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Data));
        PlayerPrefs.Save();
    }
    public struct SlotSummary
    {
        public bool exists;
        public int night;
        public int successfulOrders;
        public int failedOrders;
        public int wallet;
    }

    public static SlotSummary GetSlotSummary(int slot)
    {
        ImportLegacyProgress();

        string key = SaveSlots.KeyForSlot(slot, LegacySaveKey);

        if (!PlayerPrefs.HasKey(key))
            return new SlotSummary();

        SaveData saved;

        try
        {
            saved = JsonUtility.FromJson<SaveData>(
                PlayerPrefs.GetString(key, ""));
        }
        catch (System.ArgumentException)
        {
            Debug.LogError($"Could not read Save Slot {slot}.");
            return new SlotSummary { exists = true };
        }

        if (saved == null)
            return new SlotSummary { exists = true };

        return new SlotSummary
        {
            exists = true,
            night = saved.hasCheckpoint && saved.checkpoint != null
                ? saved.checkpoint.nightNumber
                : saved.completedShifts + 1,
            successfulOrders = saved.successfulOrders,
            failedOrders = saved.failedOrders,
            wallet = saved.wallet
        };
    }
    public static void ResetAllTestProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        data = null;
        loadedSlot = -1;
        SaveSlots.SelectSlot(1);

        Debug.Log("All test saves, story flags, and preferences cleared.");
    }
}