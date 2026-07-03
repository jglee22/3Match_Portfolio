using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Mock 상점 — 상품 목록, 구매 확인, 지급·저장</summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    Canvas rootCanvas;
    GameObject shopPanel;
    GameObject confirmPanel;
    GameObject resultPanel;
    TextMeshProUGUI confirmMessageText;
    TextMeshProUGUI resultMessageText;
    ShopProduct pendingProduct;
    Transform shopCloseButton;
    readonly MockShopService fallbackShopService = new MockShopService();

    const int ShopUiVersion = 3;
    static int builtShopUiVersion;

    const float RowWidth = RuntimeUIBuilder.ModalRowWidth;
    const float RowHeight = 96f;
    const float RowGap = 16f;
    const float ActionButtonHeight = 52f;

    IShopService ShopService =>
        GameServicesBootstrap.Instance != null
            ? GameServicesBootstrap.Instance.ShopService
            : fallbackShopService;

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

    public void OpenShop()
    {
        if (AdRewardManager.BlocksLobbyInput) return;

        EnsureUI();
        shopPanel.transform.SetAsLastSibling();
        shopPanel.SetActive(true);
        confirmPanel.SetActive(false);
        resultPanel.SetActive(false);
    }

    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
        if (resultPanel != null)
            resultPanel.SetActive(false);
        pendingProduct = null;
    }

    void EnsureUI()
    {
        if (shopPanel != null && builtShopUiVersion >= ShopUiVersion) return;

        if (shopPanel != null)
        {
            Destroy(shopPanel);
            shopPanel = null;
            confirmPanel = null;
            resultPanel = null;
            shopCloseButton = null;
        }

        builtShopUiVersion = ShopUiVersion;

        var ui = HyperCasualUIAssets.Instance;
        int productCount = ShopCatalog.Products.Count;
        float innerHeight = RuntimeUIBuilder.ModalHeaderHeight + productCount * RowHeight
            + (productCount - 1) * RowGap + RuntimeUIBuilder.ModalFooterHeight;

        rootCanvas = FindAnyObjectByType<Canvas>();
        if (rootCanvas == null)
        {
            var canvasGo = new GameObject("ShopCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            rootCanvas = canvasGo.GetComponent<Canvas>();
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        shopPanel = RuntimeUIBuilder.CreateModal(rootCanvas.transform, "ShopPanel",
            RuntimeUIBuilder.BuildModalOuterSize(720f, innerHeight), ui?.popupPanel);
        var shopContent = RuntimeUIBuilder.GetPanelContent(shopPanel);
        RuntimeUIBuilder.CreatePopupTitle(shopContent, "Title", "Shop");

        for (int i = 0; i < productCount; i++)
            CreateProductRow(shopContent, ShopCatalog.Products[i],
                RuntimeUIBuilder.GetModalRowY(i, RowHeight, RowGap, innerHeight, RuntimeUIBuilder.ModalHeaderHeight));

        RuntimeUIBuilder.CreateModalCloseButton(shopContent, CloseShop);
        shopCloseButton = shopContent.Find("CloseButton");

        const float confirmWidth = 700f;
        const float confirmMessageArea = 168f;
        float confirmInnerHeight = confirmMessageArea + RuntimeUIBuilder.ModalOverlayFooterHeight;
        float confirmInnerWidth = RuntimeUIBuilder.GetModalInnerWidth(confirmWidth);

        confirmPanel = RuntimeUIBuilder.CreateModal(shopPanel.transform, "ShopConfirmPanel",
            RuntimeUIBuilder.BuildOverlayModalOuterSize(confirmWidth, confirmMessageArea), ui?.popupPanel);
        var confirmContent = RuntimeUIBuilder.GetPanelContent(confirmPanel);
        confirmMessageText = RuntimeUIBuilder.CreateLabel(confirmContent, "ConfirmMessage", string.Empty,
            new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(confirmInnerWidth - 32f, 140f), 28,
            TextAlignmentOptions.Center, FontStyles.Bold, RuntimeUIBuilder.Popup.Body);
        RuntimeUIBuilder.LayoutOverlayMessage(confirmMessageText, confirmInnerWidth, confirmInnerHeight);

        var confirmButtonPos = RuntimeUIBuilder.GetOverlayButtonPosition();
        RuntimeUIBuilder.CreateButton(confirmContent, "ConfirmYesButton", "구매", new Vector2(0.3f, 0f), confirmButtonPos,
            new Vector2(160f, 52f), UIButtonStyle.Green, OnConfirmPurchase);

        RuntimeUIBuilder.CreateButton(confirmContent, "ConfirmNoButton", "취소", new Vector2(0.7f, 0f), confirmButtonPos,
            new Vector2(160f, 52f), UIButtonStyle.Red, CloseConfirm);

        const float resultWidth = 560f;
        const float resultMessageArea = 120f;
        float resultInnerHeight = resultMessageArea + RuntimeUIBuilder.ModalOverlayFooterHeight;
        float resultInnerWidth = RuntimeUIBuilder.GetModalInnerWidth(resultWidth);

        resultPanel = RuntimeUIBuilder.CreateModal(shopPanel.transform, "ShopResultPanel",
            RuntimeUIBuilder.BuildOverlayModalOuterSize(resultWidth, resultMessageArea), ui?.popupPanel);
        var resultContent = RuntimeUIBuilder.GetPanelContent(resultPanel);
        resultMessageText = RuntimeUIBuilder.CreateLabel(resultContent, "ResultMessage", string.Empty,
            new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(resultInnerWidth - 32f, 100f), 30,
            TextAlignmentOptions.Center, FontStyles.Bold, RuntimeUIBuilder.Popup.Body);
        RuntimeUIBuilder.LayoutOverlayMessage(resultMessageText, resultInnerWidth, resultInnerHeight);

        RuntimeUIBuilder.CreateButton(resultContent, "ResultOkButton", "확인", new Vector2(0.5f, 0f), confirmButtonPos,
            new Vector2(160f, 52f), UIButtonStyle.Blue, CloseResult);

        confirmPanel.SetActive(false);
        resultPanel.SetActive(false);
        shopPanel.SetActive(false);
    }

    void CreateProductRow(Transform parent, ShopProduct product, float y)
    {
        var ui = HyperCasualUIAssets.Instance;
        var row = RuntimeUIBuilder.CreateListRow(parent, product.id, new Vector2(RowWidth, RowHeight), y);

        Sprite rowIcon = RuntimeUIBuilder.GetRewardIcon(ui, product.rewardGold, product.rewardGems, product.rewardShuffle);
        RuntimeUIBuilder.CreateRowIcon(row, rowIcon, 20f);

        const float textLeft = 84f;
        float buttonWidth = RuntimeUIBuilder.GetRowActionButtonWidth(ActionButtonHeight, UIButtonStyle.Blue, 132f);
        const float buttonMargin = 16f;
        const float textWidth = 200f;
        float rewardInset = buttonWidth + buttonMargin + 8f;

        RuntimeUIBuilder.CreateLabel(row, "Name", product.displayName, new Vector2(0f, 0.5f), new Vector2(textLeft, 16f),
            new Vector2(textWidth, 34f), 28, TextAlignmentOptions.MidlineLeft, FontStyles.Bold, RuntimeUIBuilder.Popup.Body);

        RuntimeUIBuilder.CreateLabel(row, "Price", product.PriceLabel, new Vector2(0f, 0.5f), new Vector2(textLeft, -18f),
            new Vector2(textWidth, 28f), 22, TextAlignmentOptions.MidlineLeft, FontStyles.Normal, RuntimeUIBuilder.Popup.Muted);

        RuntimeUIBuilder.CreateLabel(row, "Reward", product.RewardLabel, new Vector2(1f, 0.5f), new Vector2(-rewardInset, -18f),
            new Vector2(116f, 28f), 26, TextAlignmentOptions.MidlineRight, FontStyles.Bold,
            RuntimeUIBuilder.GetRewardColor(product.rewardGold, product.rewardGems, product.rewardShuffle));

        RuntimeUIBuilder.CreateButton(row, "BuyButton", "구매", new Vector2(1f, 0.5f), new Vector2(-buttonMargin, 0f),
            new Vector2(buttonWidth, ActionButtonHeight), UIButtonStyle.Blue, () => OpenConfirm(product));
    }

    void SetShopFooterVisible(bool visible)
    {
        if (shopCloseButton != null)
            shopCloseButton.gameObject.SetActive(visible);
    }

    void OpenConfirm(ShopProduct product)
    {
        pendingProduct = product;
        RuntimeUIBuilder.ApplyModalMessageText(confirmMessageText, product.ConfirmMessage,
            RuntimeUIBuilder.ModalMessageKind.Confirm, product.rewardGold, product.rewardGems, product.rewardShuffle);
        SetShopFooterVisible(false);
        RuntimeUIBuilder.BringModalToFront(confirmPanel);
    }

    void CloseConfirm()
    {
        confirmPanel.SetActive(false);
        SetShopFooterVisible(true);
        pendingProduct = null;
    }

    void OnConfirmPurchase()
    {
        if (pendingProduct == null) return;

        if (ShopService.TryPurchase(pendingProduct, out string message))
        {
            CurrencyManager.Instance?.RefreshUI();
            confirmPanel.SetActive(false);
            ShowResult(true, message);
            pendingProduct = null;
        }
        else
        {
            confirmPanel.SetActive(false);
            ShowResult(false, ShopService.FormatFailureMessage(message));
            pendingProduct = null;
        }
    }

    void ShowResult(bool success, string message)
    {
        RuntimeUIBuilder.ApplyModalMessageText(resultMessageText, message,
            success ? RuntimeUIBuilder.ModalMessageKind.Success : RuntimeUIBuilder.ModalMessageKind.Failure);
        SetShopFooterVisible(false);
        RuntimeUIBuilder.BringModalToFront(resultPanel);
    }

    void CloseResult()
    {
        resultPanel.SetActive(false);
        SetShopFooterVisible(true);
    }
}
