using System.Collections.Generic;
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
    readonly Dictionary<string, TextMeshProUGUI> inventoryCountTexts = new();

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

        if (SettingsManager.Instance == null)
            new GameObject("SettingsManager").AddComponent<SettingsManager>();
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

        var ui = HyperCasualUIAssets.Instance;

        var canvasGo = new GameObject("LobbyCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var bg = CreateImage(canvasGo.transform, "Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        bg.color = new Color(0.1f, 0.14f, 0.22f, 1f);

        var topBar = RuntimeUIBuilder.CreateStretchBar(canvasGo.transform, "TopBar", 96f, ui?.hudBar);
        goldText = RuntimeUIBuilder.CreateCurrencySlot(topBar, "GoldText", ui?.iconCoin, alignLeft: true);
        shuffleText = RuntimeUIBuilder.CreateCenterCurrencySlot(topBar, "ShuffleText", ui?.iconShuffle);
        gemText = RuntimeUIBuilder.CreateCurrencySlot(topBar, "GemText", ui?.iconGem, alignLeft: false);

        var menuFrameGo = new GameObject("MenuFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        menuFrameGo.transform.SetParent(canvasGo.transform, false);
        var menuFrameRect = menuFrameGo.GetComponent<RectTransform>();
        menuFrameRect.anchorMin = new Vector2(0.5f, 0.5f);
        menuFrameRect.anchorMax = new Vector2(0.5f, 0.5f);
        menuFrameRect.pivot = new Vector2(0.5f, 0.5f);
        menuFrameRect.anchoredPosition = new Vector2(0f, -32f);

        const float playButtonHeight = 88f;
        const float buttonHeight = 72f;
        const float buttonGap = 18f;
        const int buttonCount = 5;
        const float panelPadX = 56f;
        const float panelPadY = 64f;
        float stackHeight = playButtonHeight + buttonGap + (buttonCount - 1) * buttonHeight + (buttonCount - 2) * buttonGap;
        float buttonWidth = RuntimeUIBuilder.GetMaxButtonWidth(buttonHeight,
            UIButtonStyle.Green, UIButtonStyle.Blue, UIButtonStyle.Orange, UIButtonStyle.Grey);
        float playButtonWidth = Mathf.Max(buttonWidth, RuntimeUIBuilder.GetMaxButtonWidth(playButtonHeight, UIButtonStyle.Green));

        menuFrameRect.sizeDelta = new Vector2(playButtonWidth + panelPadX * 2f, stackHeight + panelPadY * 2f);
        var menuFrame = menuFrameGo.GetComponent<Image>();
        if (ui?.menuPanel != null)
            RuntimeUIBuilder.ApplySprite(menuFrame, ui.menuPanel, preserveAspect: true);
        else
            menuFrame.color = new Color(0.12f, 0.18f, 0.28f, 0.92f);

        var menuGo = new GameObject("Menu", typeof(RectTransform));
        menuGo.transform.SetParent(menuFrameGo.transform, false);
        var menu = menuGo.GetComponent<RectTransform>();
        menu.anchorMin = Vector2.zero;
        menu.anchorMax = Vector2.one;
        menu.offsetMin = new Vector2(panelPadX, panelPadY);
        menu.offsetMax = new Vector2(-panelPadX, -panelPadY);

        float cursorY = stackHeight * 0.5f;

        cursorY -= playButtonHeight * 0.5f;
        playButton = RuntimeUIBuilder.CreateMenuButton(menu, "PlayButton", "Play",
            new Vector2(0f, cursorY), playButtonHeight, playButtonWidth, UIButtonStyle.Green);
        cursorY -= playButtonHeight * 0.5f + buttonGap;

        cursorY -= buttonHeight * 0.5f;
        shopButton = RuntimeUIBuilder.CreateMenuButton(menu, "ShopButton", "Shop",
            new Vector2(0f, cursorY), buttonHeight, buttonWidth, UIButtonStyle.Blue);
        cursorY -= buttonHeight * 0.5f + buttonGap;

        cursorY -= buttonHeight * 0.5f;
        rewardButton = RuntimeUIBuilder.CreateMenuButton(menu, "RewardButton", "Reward",
            new Vector2(0f, cursorY), buttonHeight, buttonWidth, UIButtonStyle.Orange);
        cursorY -= buttonHeight * 0.5f + buttonGap;

        cursorY -= buttonHeight * 0.5f;
        inventoryButton = RuntimeUIBuilder.CreateMenuButton(menu, "InventoryButton", "Inventory",
            new Vector2(0f, cursorY), buttonHeight, buttonWidth, UIButtonStyle.Blue);
        cursorY -= buttonHeight * 0.5f + buttonGap;

        cursorY -= buttonHeight * 0.5f;
        settingsButton = RuntimeUIBuilder.CreateMenuButton(menu, "SettingsButton", "Settings",
            new Vector2(0f, cursorY), buttonHeight, buttonWidth, UIButtonStyle.Grey);

        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            new GameObject("EventSystem",
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
            settingsButton.onClick.AddListener(() =>
            {
                if (AdRewardManager.BlocksLobbyInput) return;
                SettingsManager.Instance?.OpenSettings();
            });
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
        RefreshInventoryCounts();
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

    void RefreshInventoryCounts()
    {
        if (inventoryCountTexts.TryGetValue("shuffle", out TextMeshProUGUI shuffleCount))
            shuffleCount.text = $"보유 x{GameDataManager.Instance?.ShuffleItems ?? 0}";
    }

    GameObject CreateInventoryPanel(Transform parent)
    {
        var ui = HyperCasualUIAssets.Instance;
        const float rowHeight = 112f;
        const float rowGap = 12f;
        const int itemCount = 1;
        float headerHeight = RuntimeUIBuilder.ModalHeaderHeight + RuntimeUIBuilder.ModalSubtitleExtra;
        float listContentHeight = itemCount * rowHeight + (itemCount - 1) * rowGap;
        float listViewportHeight = Mathf.Min(listContentHeight, RuntimeUIBuilder.ModalScrollMaxHeight);
        float innerHeight = headerHeight + listViewportHeight + 8f + RuntimeUIBuilder.ModalFooterHeight;

        var dim = RuntimeUIBuilder.CreateModal(parent, "InventoryPanel",
            RuntimeUIBuilder.BuildModalOuterSize(520f, innerHeight), ui?.popupPanel);

        var card = RuntimeUIBuilder.GetPanelContent(dim);
        RuntimeUIBuilder.CreatePopupTitle(card, "Title", "Inventory");
        RuntimeUIBuilder.CreatePopupSubtitle(card, "Subtitle", "게임 중 사용할 아이템");

        var scrollContent = RuntimeUIBuilder.CreateModalListScroll(card, "InventoryScroll",
            headerHeight + 4f, RuntimeUIBuilder.ModalFooterHeight + 4f, listContentHeight);

        CreateShuffleInventoryRow(scrollContent, ui, rowHeight, rowGap);

        RuntimeUIBuilder.CreateModalCloseButton(card, CloseInventory);

        return dim;
    }

    void CreateShuffleInventoryRow(Transform scrollContent, HyperCasualUIAssets ui, float rowHeight, float rowGap)
    {
        const float horizontalPad = 4f;
        var row = RuntimeUIBuilder.CreateScrollListRowStretch(scrollContent, "ShuffleRow", rowHeight, 0, rowGap,
            horizontalPad);
        RuntimeUIBuilder.CreateRowIcon(row, ui?.iconShuffle, 16f, 56f);

        const float textLeft = 84f;
        RuntimeUIBuilder.CreateLabel(row, "ItemName", "Shuffle Item", new Vector2(0f, 0.5f), new Vector2(textLeft, 20f),
            new Vector2(200f, 34f), 28, TextAlignmentOptions.MidlineLeft, FontStyles.Bold, RuntimeUIBuilder.Popup.Body);

        RuntimeUIBuilder.CreateLabel(row, "ItemDesc", "보드가 막혔을 때 블록을 섞습니다", new Vector2(0f, 0.5f), new Vector2(textLeft, -20f),
            new Vector2(260f, 32f), 20, TextAlignmentOptions.MidlineLeft, FontStyles.Normal, RuntimeUIBuilder.Popup.Body);

        var countText = RuntimeUIBuilder.CreateLabel(row, "Count", string.Empty, new Vector2(1f, 1f), new Vector2(-12f, -10f),
            new Vector2(100f, 32f), 24, TextAlignmentOptions.TopRight, FontStyles.Bold,
            RuntimeUIBuilder.Popup.InventoryCount);
        countText.rectTransform.pivot = new Vector2(1f, 1f);
        inventoryCountTexts["shuffle"] = countText;
    }

    static RectTransform CreateRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPos, Vector2 sizeDelta)
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

    static Image CreateImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax)
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
}
