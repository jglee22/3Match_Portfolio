using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>로비 UI — 재화 표시, Play/Shop/Inventory/Settings</summary>
public class LobbyManager : MonoBehaviour
{
    [Header("Optional — 비어 있으면 런타임 생성")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI gemText;
    public TextMeshProUGUI shuffleText;
    public Button playButton;
    public Button shopButton;
    public Button rewardButton;
    public Button inventoryButton;
    public Button settingsButton;

    GameObject inventoryPanel;
    TextMeshProUGUI inventoryShuffleText;

    void Awake()
    {
        EnsureManagers();
    }

    void OnEnable()
    {
        CurrencyManager.Instance?.RefreshUI();
    }

    void Start()
    {
        EnsureUI();
        WireButtons();
        CurrencyManager.Instance?.RefreshUI();
    }

    void EnsureManagers()
    {
        if (GameDataManager.Instance == null)
            new GameObject("GameDataManager").AddComponent<GameDataManager>();

        if (CurrencyManager.Instance == null)
            new GameObject("CurrencyManager").AddComponent<CurrencyManager>();

        if (ShopManager.Instance == null)
            new GameObject("ShopManager").AddComponent<ShopManager>();

        if (GameServicesBootstrap.Instance == null)
            GameServicesBootstrap.Ensure();

        if (AdRewardManager.Instance == null)
            new GameObject("AdRewardManager").AddComponent<AdRewardManager>();
    }

    void EnsureUI()
    {
        if (goldText != null && gemText != null && playButton != null)
        {
            CurrencyManager.Instance?.Bind(goldText, gemText, shuffleText);
            return;
        }

        BuildRuntimeUI();
        CurrencyManager.Instance?.Bind(goldText, gemText, shuffleText);
    }

    void BuildRuntimeUI()
    {
        if (FindAnyObjectByType<Canvas>() != null && playButton != null)
            return;

        var canvasGo = new GameObject("LobbyCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var bg = CreateImage(canvasGo.transform, "Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        bg.color = new Color(0.1f, 0.14f, 0.22f, 1f);

        var topBar = CreateTopStretchBar(canvasGo.transform, "TopBar", 96f);
        var topBarBg = topBar.gameObject.AddComponent<Image>();
        topBarBg.color = new Color(0f, 0f, 0f, 0.35f);
        topBarBg.raycastTarget = false;

        goldText = CreateBarLabel(topBar, "GoldText", "Gold: 0", true);
        shuffleText = CreateBarCenterLabel(topBar, "ShuffleText", "Shuffle: 0");
        gemText = CreateBarLabel(topBar, "GemText", "Gems: 0", false);

        var menu = CreateRect(canvasGo.transform, "Menu",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420f, 500f));

        playButton = CreateMenuButton(menu, "PlayButton", "Play", new Vector2(0.5f, 0.82f));
        shopButton = CreateMenuButton(menu, "ShopButton", "Shop", new Vector2(0.5f, 0.61f));
        rewardButton = CreateMenuButton(menu, "RewardButton", "Reward", new Vector2(0.5f, 0.4f));
        inventoryButton = CreateMenuButton(menu, "InventoryButton", "Inventory", new Vector2(0.5f, 0.19f));
        settingsButton = CreateMenuButton(menu, "SettingsButton", "Settings", new Vector2(0.5f, -0.02f));

        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }
    }

    void WireButtons()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayClicked);
        }

        if (shopButton != null)
        {
            shopButton.onClick.RemoveAllListeners();
            shopButton.onClick.AddListener(() =>
            {
                if (AdRewardManager.BlocksLobbyInput) return;
                ShopManager.Instance?.OpenShop();
            });
        }

        if (rewardButton != null)
        {
            rewardButton.onClick.RemoveAllListeners();
            rewardButton.onClick.AddListener(() =>
            {
                if (AdRewardManager.BlocksLobbyInput) return;
                AdRewardManager.Instance?.OpenRewards();
            });
        }

        if (inventoryButton != null)
        {
            inventoryButton.onClick.RemoveAllListeners();
            inventoryButton.onClick.AddListener(OpenInventory);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(() => Debug.Log("[Lobby] Settings — 준비 중"));
        }
    }

    void OnPlayClicked()
    {
        if (AdRewardManager.BlocksLobbyInput) return;
        SceneLoader.LoadGame();
    }

    void OpenInventory()
    {
        if (AdRewardManager.BlocksLobbyInput) return;

        EnsureInventoryPanel();
        inventoryShuffleText.text =
            $"Shuffle Item\n\n보유: {GameDataManager.Instance?.ShuffleItems ?? 0}개";
        inventoryPanel.SetActive(true);
        inventoryPanel.transform.SetAsLastSibling();
    }

    void CloseInventory()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    void EnsureInventoryPanel()
    {
        if (inventoryPanel != null) return;

        var canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        inventoryPanel = CreateInventoryPanel(canvas.transform);
        inventoryPanel.SetActive(false);
    }

    GameObject CreateInventoryPanel(Transform parent)
    {
        var dim = new GameObject("InventoryPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dim.transform.SetParent(parent, false);
        var dimRect = dim.GetComponent<RectTransform>();
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.offsetMin = Vector2.zero;
        dimRect.offsetMax = Vector2.zero;
        dim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);

        var card = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        card.transform.SetParent(dim.transform, false);
        var cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(480f, 280f);
        card.GetComponent<Image>().color = new Color(0.1f, 0.14f, 0.22f, 0.98f);

        CreateLabel(card.transform, "Title", "Inventory",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(400f, 50f),
            TextAlignmentOptions.Center);

        inventoryShuffleText = CreateLabel(card.transform, "ShuffleCount", string.Empty,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 120f),
            TextAlignmentOptions.Center);
        inventoryShuffleText.fontSize = 34;

        var closeGo = new GameObject("CloseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        closeGo.transform.SetParent(card.transform, false);
        var closeRect = closeGo.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0f);
        closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.anchoredPosition = new Vector2(0f, 36f);
        closeRect.sizeDelta = new Vector2(180f, 56f);
        closeGo.GetComponent<Image>().color = new Color(0.35f, 0.38f, 0.45f, 1f);
        closeGo.GetComponent<Button>().onClick.AddListener(CloseInventory);
        CreateLabel(closeGo.transform, "Text", "닫기", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            TextAlignmentOptions.Center).fontSize = 28;

        return dim;
    }

    static TextMeshProUGUI CreateBarCenterLabel(RectTransform bar, string name, string text)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(bar, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(320f, 0f);

        var label = go.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = 32;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.overflowMode = TextOverflowModes.Overflow;
        label.enableWordWrapping = false;
        return label;
    }

    static RectTransform CreateTopStretchBar(Transform parent, string name, float height)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, height);
        return rect;
    }

    static TextMeshProUGUI CreateBarLabel(RectTransform bar, string name, string text, bool alignLeft)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(bar, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(alignLeft ? 0f : 1f, 0f);
        rect.anchorMax = new Vector2(alignLeft ? 0f : 1f, 1f);
        rect.pivot = new Vector2(alignLeft ? 0f : 1f, 0.5f);
        rect.anchoredPosition = new Vector2(alignLeft ? 48f : -48f, 0f);
        rect.sizeDelta = new Vector2(560f, 0f);

        var label = go.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = 36;
        label.alignment = alignLeft ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.MidlineRight;
        label.color = Color.white;
        label.overflowMode = TextOverflowModes.Overflow;
        label.enableWordWrapping = false;
        return label;
    }

    static RectTransform CreateRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;
        return rect;
    }

    static Image CreateImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        return go.GetComponent<Image>();
    }

    static TextMeshProUGUI CreateLabel(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPos, Vector2 sizeDelta, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;

        var label = go.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = 32;
        label.alignment = align;
        label.color = Color.white;
        return label;
    }

    static Button CreateMenuButton(Transform parent, string name, string label, Vector2 anchorY)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, anchorY.y);
        rect.anchorMax = new Vector2(1f, anchorY.y);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, 72f);

        var image = go.GetComponent<Image>();
        image.color = new Color(0.2f, 0.45f, 0.75f, 1f);

        var button = go.GetComponent<Button>();

        var text = CreateLabel(go.transform, "Text", label, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            TextAlignmentOptions.Center);
        text.fontSize = 30;

        return button;
    }
}
