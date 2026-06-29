using System;

[Serializable]
public class ShopProduct
{
    public string id;
    public string displayName;
    public ShopPriceType priceType;
    public int priceGold;
    public int rewardGold;
    public int rewardGems;
    public int rewardShuffle;

    public string PriceLabel =>
        priceType == ShopPriceType.Free ? "무료 (Mock)" : $"{priceGold:N0} Gold";

    public string RewardLabel
    {
        get
        {
            if (rewardGold > 0) return $"+{rewardGold:N0} Gold";
            if (rewardGems > 0) return $"+{rewardGems} Gems";
            if (rewardShuffle > 0) return $"+{rewardShuffle} Shuffle";
            return string.Empty;
        }
    }

    public string ConfirmMessage =>
        $"{displayName}을(를) 구매하시겠습니까?\n가격: {PriceLabel}\n보상: {RewardLabel}";
}

public enum ShopPriceType
{
    Free,
    Gold
}
