/// <summary>Mock 상점 — 무료·골드 구매 시뮬레이션 (실 IAP 미연동)</summary>
public class MockShopService : IShopService
{
    public bool CanPurchase(ShopProduct product, out string reason)
    {
        reason = null;
        if (product == null)
        {
            reason = "상품 정보가 없습니다.";
            return false;
        }

        if (GameDataManager.Instance == null)
        {
            reason = "데이터를 불러올 수 없습니다.";
            return false;
        }

        if (product.priceType == ShopPriceType.Gold && GameDataManager.Instance.Gold < product.priceGold)
        {
            reason = $"Gold가 부족합니다.\n(필요: {product.priceGold:N0} / 보유: {GameDataManager.Instance.Gold:N0})";
            return false;
        }

        return true;
    }

    public bool TryPurchase(ShopProduct product, out string resultMessage)
    {
        if (!CanPurchase(product, out resultMessage))
            return false;

        var data = GameDataManager.Instance;

        if (product.priceType == ShopPriceType.Gold)
            data.SpendGold(product.priceGold);

        if (product.rewardGold > 0)
            data.AddGold(product.rewardGold);

        if (product.rewardGems > 0)
            data.AddGems(product.rewardGems);

        if (product.rewardShuffle > 0)
            data.AddShuffleItems(product.rewardShuffle);

        resultMessage = $"구매 성공!\n{product.RewardLabel}";
        return true;
    }

    public string FormatFailureMessage(string reason) => $"구매 실패\n{reason}";
}
