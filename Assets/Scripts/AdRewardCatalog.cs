public static class AdRewardCatalog
{
    public static readonly AdRewardOfferData GoldAd = new AdRewardOfferData
    {
        id = "ad_gold",
        displayName = "Gold 광고",
        rewardGold = 100,
    };

    public static readonly AdRewardOfferData GemAd = new AdRewardOfferData
    {
        id = "ad_gem",
        displayName = "Gem 광고",
        rewardGems = 1,
    };

    public static readonly AdRewardOfferData[] Offers = { GoldAd, GemAd };
}

public struct AdRewardOfferData
{
    public string id;
    public string displayName;
    public int rewardGold;
    public int rewardGems;

    public string RewardLabel
    {
        get
        {
            if (rewardGold > 0 && rewardGems > 0)
                return $"+{rewardGold:N0} Gold, +{rewardGems} Gem";
            if (rewardGold > 0)
                return $"+{rewardGold:N0} Gold";
            if (rewardGems > 0)
                return $"+{rewardGems} Gem";
            return string.Empty;
        }
    }
}
