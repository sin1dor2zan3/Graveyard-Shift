using UnityEngine;

public static class PlayerProgress
{
    private const string SaveKey = "GraveyardShift.Progress.v1";

    [System.Serializable]
    private class SaveData
    {
        public int wallet;
        public int completedShifts;
        public bool largerScoop;
        public bool cheaperBoxes;
        public bool cheaperCushioning;
    }

    private static SaveData data;

    private static SaveData Data
    {
        get
        {
            if (data == null)
            {
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
}