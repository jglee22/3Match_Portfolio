using UnityEngine;

/// <summary>Hyper Casual UI Pack 스프라이트 참조 (Resources/HyperCasualUIAssets)</summary>
[CreateAssetMenu(fileName = "HyperCasualUIAssets", menuName = "UI/Hyper Casual UI Assets")]
public class HyperCasualUIAssets : ScriptableObject
{
    [Header("Panels")]
    public Sprite menuPanel;
    public Sprite popupPanel;
    public Sprite shopPanel;
    public Sprite hudBar;
    public Sprite rowBackground;
    public Sprite failPanel;
    public Sprite clearPanel;
    public Sprite rewardPanel;
    public Sprite inventoryPanel;

    [Header("Buttons")]
    public Sprite btnGreen;
    public Sprite btnBlue;
    public Sprite btnOrange;
    public Sprite btnGrey;
    public Sprite btnRed;

    [Header("Icons")]
    public Sprite iconCoin;
    public Sprite iconGem;
    public Sprite iconShuffle;
    public Sprite iconShop;
    public Sprite iconAds;

    static HyperCasualUIAssets instance;

    public static HyperCasualUIAssets Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load<HyperCasualUIAssets>("HyperCasualUIAssets");
            return instance;
        }
    }

    public Sprite GetButton(UIButtonStyle style) => style switch
    {
        UIButtonStyle.Green => btnGreen,
        UIButtonStyle.Blue => btnBlue,
        UIButtonStyle.Orange => btnOrange,
        UIButtonStyle.Grey => btnGrey,
        UIButtonStyle.Red => btnRed,
        _ => btnBlue
    };
}

public enum UIButtonStyle
{
    Green,
    Blue,
    Orange,
    Grey,
    Red
}
