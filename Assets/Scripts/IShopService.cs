/// <summary>상점 결제 추상화 — Mock / Unity IAP 등 구현체 교체</summary>
public interface IShopService
{
    bool CanPurchase(ShopProduct product, out string reason);
    bool TryPurchase(ShopProduct product, out string resultMessage);
    string FormatFailureMessage(string reason);
}
