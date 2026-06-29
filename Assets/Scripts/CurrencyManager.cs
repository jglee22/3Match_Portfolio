using TMPro;
using UnityEngine;

/// <summary>재화 UI 갱신</summary>
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    public TextMeshProUGUI goldText;
    public TextMeshProUGUI gemText;
    public TextMeshProUGUI shuffleText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.OnCurrencyChanged += RefreshUI;
            GameDataManager.Instance.OnInventoryChanged += RefreshUI;
        }
        RefreshUI();
    }

    void OnDisable()
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.OnCurrencyChanged -= RefreshUI;
            GameDataManager.Instance.OnInventoryChanged -= RefreshUI;
        }
    }

    public void Bind(TextMeshProUGUI gold, TextMeshProUGUI gem, TextMeshProUGUI shuffle = null)
    {
        goldText = gold;
        gemText = gem;
        shuffleText = shuffle;
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (GameDataManager.Instance == null) return;

        if (goldText != null)
            goldText.text = $"Gold: {GameDataManager.Instance.Gold:N0}";

        if (gemText != null)
            gemText.text = $"Gems: {GameDataManager.Instance.Gems:N0}";

        if (shuffleText != null)
            shuffleText.text = $"Shuffle: {GameDataManager.Instance.ShuffleItems}";
    }
}
