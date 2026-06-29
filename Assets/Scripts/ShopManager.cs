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
    readonly MockShopService fallbackShopService = new MockShopService();

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
        if (shopPanel != null) return;

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

        shopPanel = CreatePanel(rootCanvas.transform, "ShopPanel", new Vector2(760f, 620f));
        CreateLabel(shopPanel.transform, "Title", "Shop", new Vector2(0.5f, 1f), new Vector2(0f, -36f),
            new Vector2(600f, 56f), 40, TextAlignmentOptions.Center, FontStyles.Bold);

        float startY = 120f;
        float gap = 130f;
        int index = 0;
        foreach (ShopProduct product in ShopCatalog.Products)
        {
            CreateProductRow(shopPanel.transform, product, startY - index * gap);
            index++;
        }

        CreateButton(shopPanel.transform, "CloseButton", "닫기", new Vector2(0.5f, 0f), new Vector2(0f, 36f),
            new Vector2(220f, 64f), new Color(0.35f, 0.38f, 0.45f, 1f), CloseShop);

        confirmPanel = CreatePanel(rootCanvas.transform, "ShopConfirmPanel", new Vector2(560f, 320f));
        confirmMessageText = CreateLabel(confirmPanel.transform, "ConfirmMessage", string.Empty,
            new Vector2(0.5f, 0.58f), Vector2.zero, new Vector2(480f, 140f), 28,
            TextAlignmentOptions.Center, FontStyles.Normal);

        CreateButton(confirmPanel.transform, "ConfirmYesButton", "구매", new Vector2(0.3f, 0.18f), Vector2.zero,
            new Vector2(180f, 60f), new Color(0.2f, 0.55f, 0.35f, 1f), OnConfirmPurchase);

        CreateButton(confirmPanel.transform, "ConfirmNoButton", "취소", new Vector2(0.7f, 0.18f), Vector2.zero,
            new Vector2(180f, 60f), new Color(0.45f, 0.2f, 0.2f, 1f), CloseConfirm);

        resultPanel = CreatePanel(rootCanvas.transform, "ShopResultPanel", new Vector2(520f, 240f));
        resultMessageText = CreateLabel(resultPanel.transform, "ResultMessage", string.Empty,
            new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(440f, 100f), 30,
            TextAlignmentOptions.Center, FontStyles.Bold);

        CreateButton(resultPanel.transform, "ResultOkButton", "확인", new Vector2(0.5f, 0.2f), Vector2.zero,
            new Vector2(180f, 56f), new Color(0.2f, 0.45f, 0.75f, 1f), CloseResult);

        confirmPanel.SetActive(false);
        resultPanel.SetActive(false);
        shopPanel.SetActive(false);
    }

    void CreateProductRow(Transform parent, ShopProduct product, float y)
    {
        var row = new GameObject(product.id, typeof(RectTransform), typeof(Image));
        row.transform.SetParent(parent, false);
        var rect = row.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(660f, 108f);
        row.GetComponent<Image>().color = new Color(0.14f, 0.2f, 0.3f, 0.95f);

        CreateLabel(row.transform, "Name", product.displayName, new Vector2(0f, 0.5f), new Vector2(24f, 18f),
            new Vector2(220f, 40f), 30, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);

        CreateLabel(row.transform, "Price", product.PriceLabel, new Vector2(0f, 0.5f), new Vector2(24f, -18f),
            new Vector2(260f, 34f), 24, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);

        CreateLabel(row.transform, "Reward", product.RewardLabel, new Vector2(0.5f, 0.5f), new Vector2(-40f, 0f),
            new Vector2(220f, 40f), 26, TextAlignmentOptions.MidlineRight, FontStyles.Normal);

        CreateButton(row.transform, "BuyButton", "구매", new Vector2(1f, 0.5f), new Vector2(-24f, 0f),
            new Vector2(120f, 56f), new Color(0.2f, 0.45f, 0.75f, 1f), () => OpenConfirm(product));
    }

    void OpenConfirm(ShopProduct product)
    {
        pendingProduct = product;
        confirmMessageText.text = product.ConfirmMessage;
        confirmPanel.transform.SetAsLastSibling();
        confirmPanel.SetActive(true);
    }

    void CloseConfirm()
    {
        confirmPanel.SetActive(false);
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
        resultMessageText.text = message;
        resultMessageText.color = success
            ? new Color(0.55f, 0.95f, 0.7f)
            : new Color(1f, 0.5f, 0.5f);
        resultPanel.transform.SetAsLastSibling();
        resultPanel.SetActive(true);
    }

    void CloseResult()
    {
        resultPanel.SetActive(false);
    }

    static GameObject CreatePanel(Transform parent, string name, Vector2 size)
    {
        var dim = new GameObject($"{name}_Dim", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dim.transform.SetParent(parent, false);
        var dimRect = dim.GetComponent<RectTransform>();
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.offsetMin = Vector2.zero;
        dimRect.offsetMax = Vector2.zero;
        dim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);

        var panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(dim.transform, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        panel.GetComponent<Image>().color = new Color(0.1f, 0.14f, 0.22f, 0.98f);
        return dim;
    }

    static TextMeshProUGUI CreateLabel(Transform parent, string name, string text, Vector2 anchor,
        Vector2 anchoredPos, Vector2 size, int fontSize, TextAlignmentOptions align, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        var label = go.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.alignment = align;
        label.color = Color.white;
        label.overflowMode = TextOverflowModes.Overflow;
        return label;
    }

    static void CreateButton(Transform parent, string name, string label, Vector2 anchor, Vector2 anchoredPos,
        Vector2 size, Color color, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        go.GetComponent<Image>().color = color;
        var button = go.GetComponent<Button>();
        button.onClick.AddListener(onClick);

        CreateLabel(go.transform, "Text", label, new Vector2(0.5f, 0.5f), Vector2.zero, size,
            26, TextAlignmentOptions.Center, FontStyles.Bold);
    }
}
