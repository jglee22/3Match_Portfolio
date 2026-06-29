using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Mock 리워드 광고 UI — IAdService 연동, 보상 지급·저장</summary>
public class AdRewardManager : MonoBehaviour
{
    public static AdRewardManager Instance { get; private set; }

    [SerializeField] float playbackSeconds = 2f;
    [SerializeField] bool simulateFailure;
    [SerializeField] [Range(0f, 1f)] float failureChance = 0.1f;

    Canvas rootCanvas;
    GameObject rewardPanel;
    GameObject playbackPanel;
    GameObject resultPanel;
    TextMeshProUGUI playbackMessageText;
    TextMeshProUGUI resultMessageText;

    IAdService adService;
    AdRewardOfferData pendingOffer;
    bool playbackCancelled;
    bool isPlayingAd;
    bool playbackFinished;
    bool rewardGranted;
    readonly System.Collections.Generic.List<Button> watchButtons = new System.Collections.Generic.List<Button>();
    Button rewardCloseButton;
    GameObject inputBlocker;

    public bool IsPlayingAd => isPlayingAd;

    public static bool BlocksLobbyInput =>
        Instance != null && Instance.isPlayingAd;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        RefreshAdService();
    }

    void Start()
    {
        RefreshAdService();
    }

    void RefreshAdService()
    {
        adService = GameServicesBootstrap.Instance != null
            ? GameServicesBootstrap.Instance.AdService
            : new MockAdService(playbackSeconds, simulateFailure, failureChance);
    }

    public void OpenRewards()
    {
        if (isPlayingAd) return;

        EnsureUI();
        rewardPanel.transform.SetAsLastSibling();
        rewardPanel.SetActive(true);
        resultPanel.SetActive(false);
        playbackPanel.SetActive(false);
    }

    public void CloseRewards()
    {
        if (isPlayingAd) return;

        if (rewardPanel != null)
            rewardPanel.SetActive(false);
        if (resultPanel != null)
            resultPanel.SetActive(false);
        if (playbackPanel != null)
            playbackPanel.SetActive(false);
        pendingOffer = default;
    }

    void EnsureUI()
    {
        if (rewardPanel != null) return;

        rootCanvas = FindAnyObjectByType<Canvas>();
        if (rootCanvas == null)
            rootCanvas = CreateOverlayCanvas();

        rewardPanel = CreatePanel(rootCanvas.transform, "AdRewardPanel", new Vector2(760f, 520f));
        CreateLabel(rewardPanel.transform, "Title", "Reward Ad", new Vector2(0.5f, 1f), new Vector2(0f, -36f),
            new Vector2(600f, 56f), 40, TextAlignmentOptions.Center, FontStyles.Bold);

        CreateLabel(rewardPanel.transform, "Subtitle", "광고 시청 후 보상을 받습니다 (Mock)",
            new Vector2(0.5f, 1f), new Vector2(0f, -88f), new Vector2(640f, 40f), 24,
            TextAlignmentOptions.Center, FontStyles.Normal);

        float startY = 80f;
        float gap = 130f;
        for (int i = 0; i < AdRewardCatalog.Offers.Length; i++)
        {
            AdRewardOfferData offer = AdRewardCatalog.Offers[i];
            CreateOfferRow(rewardPanel.transform, offer, startY - i * gap);
        }

        CreateButton(rewardPanel.transform, "CloseButton", "닫기", new Vector2(0.5f, 0f), new Vector2(0f, 36f),
            new Vector2(220f, 64f), new Color(0.35f, 0.38f, 0.45f, 1f), CloseRewards);
        rewardCloseButton = rewardPanel.transform.Find("CloseButton")?.GetComponent<Button>();

        inputBlocker = CreateInputBlocker(rootCanvas.transform);

        playbackPanel = CreatePanel(rootCanvas.transform, "AdPlaybackPanel", new Vector2(560f, 280f));
        playbackMessageText = CreateLabel(playbackPanel.transform, "PlaybackMessage", "광고 재생 중...",
            new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(480f, 100f), 32,
            TextAlignmentOptions.Center, FontStyles.Bold);

        CreateButton(playbackPanel.transform, "CancelAdButton", "취소", new Vector2(0.5f, 0.2f), Vector2.zero,
            new Vector2(180f, 56f), new Color(0.45f, 0.2f, 0.2f, 1f), CancelPlayback);

        resultPanel = CreatePanel(rootCanvas.transform, "AdResultPanel", new Vector2(520f, 240f));
        resultMessageText = CreateLabel(resultPanel.transform, "ResultMessage", string.Empty,
            new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(440f, 100f), 30,
            TextAlignmentOptions.Center, FontStyles.Bold);

        CreateButton(resultPanel.transform, "ResultOkButton", "확인", new Vector2(0.5f, 0.2f), Vector2.zero,
            new Vector2(180f, 56f), new Color(0.2f, 0.45f, 0.75f, 1f), CloseResult);

        rewardPanel.SetActive(false);
        playbackPanel.SetActive(false);
        resultPanel.SetActive(false);
    }

    void CreateOfferRow(Transform parent, AdRewardOfferData offer, float y)
    {
        var row = new GameObject(offer.id, typeof(RectTransform), typeof(Image));
        row.transform.SetParent(parent, false);
        var rect = row.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(660f, 108f);
        row.GetComponent<Image>().color = new Color(0.14f, 0.2f, 0.3f, 0.95f);

        CreateLabel(row.transform, "Name", offer.displayName, new Vector2(0f, 0.5f), new Vector2(24f, 18f),
            new Vector2(220f, 40f), 30, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);

        CreateLabel(row.transform, "Reward", offer.RewardLabel, new Vector2(0f, 0.5f), new Vector2(24f, -18f),
            new Vector2(260f, 34f), 24, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);

        CreateLabel(row.transform, "Hint", "Mock Ad · 2초", new Vector2(0.5f, 0.5f), new Vector2(-40f, 0f),
            new Vector2(220f, 40f), 24, TextAlignmentOptions.MidlineRight, FontStyles.Normal);

        CreateButton(row.transform, "WatchButton", "광고 시청", new Vector2(1f, 0.5f), new Vector2(-24f, 0f),
            new Vector2(140f, 56f), new Color(0.85f, 0.55f, 0.15f, 1f), () => StartRewardAd(offer));

        Transform watchTransform = row.transform.Find("WatchButton");
        if (watchTransform != null && watchTransform.TryGetComponent(out Button watchButton))
            watchButtons.Add(watchButton);
    }

    void SetRewardUiLocked(bool locked)
    {
        foreach (Button button in watchButtons)
        {
            if (button != null)
                button.interactable = !locked;
        }

        if (rewardCloseButton != null)
            rewardCloseButton.interactable = !locked;

        if (inputBlocker != null)
        {
            inputBlocker.SetActive(locked);
            if (locked)
                inputBlocker.transform.SetAsLastSibling();
        }
    }

    void StartRewardAd(AdRewardOfferData offer)
    {
        if (isPlayingAd) return;

        pendingOffer = offer;
        playbackCancelled = false;
        playbackFinished = false;
        rewardGranted = false;
        isPlayingAd = true;

        SetRewardUiLocked(true);
        if (rewardPanel != null)
            rewardPanel.SetActive(false);

        playbackMessageText.text = $"광고 재생 중...\n{offer.displayName}";
        playbackPanel.transform.SetAsLastSibling();
        playbackPanel.SetActive(true);
        resultPanel.SetActive(false);

        StartCoroutine(PlayRewardAdRoutine());
    }

    IEnumerator PlayRewardAdRoutine()
    {
        yield return adService.PlayRewardedAd(OnPlaybackFinished, () => playbackCancelled);
    }

    void OnPlaybackFinished(AdPlaybackResult result)
    {
        if (playbackFinished) return;
        playbackFinished = true;
        isPlayingAd = false;

        playbackPanel.SetActive(false);
        SetRewardUiLocked(false);

        switch (result)
        {
            case AdPlaybackResult.Success:
                string grantMessage = null;
                if (!rewardGranted && AdRewardService.TryGrantReward(pendingOffer, out grantMessage))
                {
                    rewardGranted = true;
                    CurrencyManager.Instance?.RefreshUI();
                    ShowResult(true, AdRewardService.FormatResultMessage(result, grantMessage));
                }
                else if (!rewardGranted)
                {
                    ShowResult(false, AdRewardService.FormatResultMessage(AdPlaybackResult.Failure, grantMessage));
                }
                break;

            case AdPlaybackResult.Failure:
                ShowResult(false, AdRewardService.FormatResultMessage(result));
                break;

            case AdPlaybackResult.Cancel:
                ShowResult(false, AdRewardService.FormatResultMessage(result));
                break;
        }

        pendingOffer = default;
    }

    void CancelPlayback()
    {
        if (!isPlayingAd) return;
        playbackCancelled = true;
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
        if (rewardPanel != null)
            rewardPanel.SetActive(true);
    }

    static GameObject CreateInputBlocker(Transform parent)
    {
        var go = new GameObject("AdInputBlocker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = go.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.01f);
        image.raycastTarget = true;
        go.SetActive(false);
        return go;
    }

    static Canvas CreateOverlayCanvas()
    {
        var canvasGo = new GameObject("AdRewardCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
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
        go.GetComponent<Button>().onClick.AddListener(onClick);

        CreateLabel(go.transform, "Text", label, new Vector2(0.5f, 0.5f), Vector2.zero, size,
            26, TextAlignmentOptions.Center, FontStyles.Bold);
    }
}
