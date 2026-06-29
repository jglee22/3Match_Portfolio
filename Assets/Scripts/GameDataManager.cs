using System;
using UnityEngine;

/// <summary>골드·젬 등 영구 데이터 (PlayerPrefs)</summary>
public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    const string GoldKey = "player_gold";
    const string GemKey = "player_gems";
    const string ShuffleKey = "player_shuffle";

    public int Gold { get; private set; }
    public int Gems { get; private set; }
    public int ShuffleItems { get; private set; }

    public event Action OnCurrencyChanged;
    public event Action OnInventoryChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    void Load()
    {
        Gold = PlayerPrefs.GetInt(GoldKey, 1000);
        Gems = PlayerPrefs.GetInt(GemKey, 50);
        ShuffleItems = PlayerPrefs.GetInt(ShuffleKey, 0);
    }

    public void Save()
    {
        PlayerPrefs.SetInt(GoldKey, Gold);
        PlayerPrefs.SetInt(GemKey, Gems);
        PlayerPrefs.SetInt(ShuffleKey, ShuffleItems);
        PlayerPrefs.Save();
    }

    public void SetGold(int amount)
    {
        Gold = Mathf.Max(0, amount);
        Save();
        OnCurrencyChanged?.Invoke();
    }

    public void SetGems(int amount)
    {
        Gems = Mathf.Max(0, amount);
        Save();
        OnCurrencyChanged?.Invoke();
    }

    public void AddGold(int amount)
    {
        if (amount == 0) return;
        SetGold(Gold + amount);
    }

    public void AddGems(int amount)
    {
        if (amount == 0) return;
        SetGems(Gems + amount);
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (Gold < amount) return false;

        SetGold(Gold - amount);
        return true;
    }

    public void AddShuffleItems(int amount)
    {
        if (amount == 0) return;
        ShuffleItems = Mathf.Max(0, ShuffleItems + amount);
        Save();
        OnInventoryChanged?.Invoke();
    }

    public bool TryConsumeShuffleItem(int amount = 1)
    {
        if (amount <= 0 || ShuffleItems < amount) return false;

        ShuffleItems -= amount;
        Save();
        OnInventoryChanged?.Invoke();
        return true;
    }
}
