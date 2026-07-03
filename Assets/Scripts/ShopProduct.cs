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
        $"{AttachObjectParticle(displayName)} 구매하시겠습니까?\n가격: {PriceLabel}\n보상: {RewardLabel}";

    static string AttachObjectParticle(string noun)
    {
        if (string.IsNullOrEmpty(noun)) return noun;

        char last = noun[noun.Length - 1];
        if (last >= '\uAC00' && last <= '\uD7A3')
        {
            bool hasBatchim = (last - '\uAC00') % 28 != 0;
            return $"{noun}{(hasBatchim ? "을" : "를")}";
        }

        char lower = char.ToLowerInvariant(last);
        bool endsWithVowel = lower is 'a' or 'e' or 'i' or 'o' or 'u' or 'y';
        return $"{noun}{(endsWithVowel ? "를" : "을")}";
    }
}

public enum ShopPriceType
{
    Free,
    Gold
}
