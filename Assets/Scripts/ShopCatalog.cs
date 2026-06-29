using System.Collections.Generic;

public static class ShopCatalog
{
    public static IReadOnlyList<ShopProduct> Products { get; } = new List<ShopProduct>
    {
        new ShopProduct
        {
            id = "gold_pack",
            displayName = "Gold Pack",
            priceType = ShopPriceType.Free,
            rewardGold = 500
        },
        new ShopProduct
        {
            id = "gem_pack",
            displayName = "Gem Pack",
            priceType = ShopPriceType.Free,
            rewardGems = 10
        },
        new ShopProduct
        {
            id = "shuffle_item",
            displayName = "Shuffle Item",
            priceType = ShopPriceType.Gold,
            priceGold = 300,
            rewardShuffle = 1
        }
    };
}
