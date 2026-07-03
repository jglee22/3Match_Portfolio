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

    const int AdRewardUiVersion = 2;
    static int builtAdRewardUiVersion;

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
        if (rewardPanel != null && builtAdRewardUiVersion >= AdRewardUiVersion) return;

        if (rewardPanel != null)
        {
            Destroy(rewardPanel);
            rewardPanel = null;
            playbackPanel = null;
            resultPanel = null;
            inputBlocker = null;
            rewardCloseButton = null;
            watchButtons.Clear();
        }

        builtAdRewardUiVersion = AdRewardUiVersion;

        var ui = HyperCasualUIAssets.Instance;

        rootCanvas = FindAnyObjectByType<Canvas>();
        if (rootCanvas == null)
            rootCanvas = CreateOverlayCanvas();

        const float rowHeight = 96f;
        const float rowGap = 16f;
        int offerCount = AdRewardCatalog.Offers.Length;
        float headerHeight = RuntimeUIBuilder.ModalHeaderHeight + RuntimeUIBuilder.ModalSubtitleExtra;
        float innerHeight = headerHeight + offerCount * rowHeight + (offerCount - 1) * rowGap
            + RuntimeUIBuilder.ModalFooterHeight;

        rewardPanel = RuntimeUIBuilder.CreateModal(rootCanvas.transform, "AdRewardPanel",
            RuntimeUIBuilder.BuildModalOuterSize(720f, innerHeight), ui?.popupPanel);
        var rewardContent = RuntimeUIBuilder.GetPanelContent(rewardPanel);
        RuntimeUIBuilder.CreatePopupTitle(rewardContent, "Title", "Reward Ad");
        RuntimeUIBuilder.CreatePopupSubtitle(rewardContent, "Subtitle", "광고 시청 후 보상을 받습니다");

        for (int i = 0; i < offerCount; i++)
            CreateOfferRow(rewardContent, AdRewardCatalog.Offers[i],
                RuntimeUIBuilder.GetModalRowY(i, rowHeight, rowGap, innerHeight, headerHeight));

        RuntimeUIBuilder.CreateModalCloseButton(rewardContent, CloseRewards);
        rewardCloseButton = rewardContent.Find("CloseButton")?.GetComponent<Button>();

        inputBlocker = CreateInputBlocker(rewardPanel.transform);

        const float playbackWidth = 580f;
        const float playbackMessageArea = 108f;
        float playbackInnerHeight = playbackMessageArea + RuntimeUIBuilder.ModalOverlayFooterHeight;
        float playbackInnerWidth = RuntimeUIBuilder.GetModalInnerWidth(playbackWidth);
        var playbackButtonPos = RuntimeUIBuilder.GetOverlayButtonPosition();

        playbackPanel = RuntimeUIBuilder.CreateModal(rewardPanel.transform, "AdPlaybackPanel",
            RuntimeUIBuilder.BuildOverlayModalOuterSize(playbackWidth, playbackMessageArea), ui?.popupPanel);
        var playbackContent = RuntimeUIBuilder.GetPanelContent(playbackPanel);
        playbackMessageText = RuntimeUIBuilder.CreateLabel(playbackContent, "PlaybackMessage", "광고 재생 중...",
            new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(playbackInnerWidth - 32f, 88f), 30,
            TextAlignmentOptions.Center, FontStyles.Bold, RuntimeUIBuilder.Popup.Body);
        RuntimeUIBuilder.LayoutOverlayMessage(playbackMessageText, playbackInnerWidth, playbackInnerHeight);

        RuntimeUIBuilder.CreateButton(playbackContent, "CancelAdButton", "취소", new Vector2(0.5f, 0f), playbackButtonPos,
            new Vector2(180f, 56f), UIButtonStyle.Red, CancelPlayback);

        const float resultWidth = 560f;
        const float resultMessageArea = 120f;
        float resultInnerHeight = resultMessageArea + RuntimeUIBuilder.ModalOverlayFooterHeight;
        float resultInnerWidth = RuntimeUIBuilder.GetModalInnerWidth(resultWidth);

        resultPanel = RuntimeUIBuilder.CreateModal(rewardPanel.transform, "AdResultPanel",
            RuntimeUIBuilder.BuildOverlayModalOuterSize(resultWidth, resultMessageArea), ui?.popupPanel);
        var resultContent = RuntimeUIBuilder.GetPanelContent(resultPanel);
        resultMessageText = RuntimeUIBuilder.CreateLabel(resultContent, "ResultMessage", string.Empty,
            new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(resultInnerWidth - 32f, 100f), 30,
            TextAlignmentOptions.Center, FontStyles.Bold, RuntimeUIBuilder.Popup.Body);
        RuntimeUIBuilder.LayoutOverlayMessage(resultMessageText, resultInnerWidth, resultInnerHeight);

        RuntimeUIBuilder.CreateButton(resultContent, "ResultOkButton", "확인", new Vector2(0.5f, 0f), playbackButtonPos,
            new Vector2(180f, 56f), UIButtonStyle.Blue, CloseResult);

        rewardPanel.SetActive(false);
        playbackPanel.SetActive(false);
        resultPanel.SetActive(false);
    }

    void CreateOfferRow(Transform parent, AdRewardOfferData offer, float y)
    {
        var ui = HyperCasualUIAssets.Instance;
        const float rowWidth = RuntimeUIBuilder.ModalRowWidth;
        const float rowHeight = 96f;
        const float actionHeight = 52f;
        var row = RuntimeUIBuilder.CreateHighlightedListRow(parent, offer.id, new Vector2(rowWidth, rowHeight), y);

        Sprite rowIcon = RuntimeUIBuilder.GetRewardIcon(ui, offer.rewardGold, offer.rewardGems, 0);
        RuntimeUIBuilder.CreateRowIcon(row, rowIcon, 20f);

        const float textLeft = 84f;
        RuntimeUIBuilder.CreateLabel(row, "Name", offer.displayName, new Vector2(0f, 0.5f), new Vector2(textLeft, 16f),
            new Vector2(220f, 34f), 28, TextAlignmentOptions.MidlineLeft, FontStyles.Bold, RuntimeUIBuilder.Popup.Body);

        RuntimeUIBuilder.CreateLabel(row, "Reward", offer.RewardLabel, new Vector2(0f, 0.5f), new Vector2(textLeft, -18f),
            new Vector2(280f, 32f), 28, TextAlignmentOptions.MidlineLeft, FontStyles.Bold,
            RuntimeUIBuilder.GetRewardColor(offer.rewardGold, offer.rewardGems, 0));

        float buttonWidth = RuntimeUIBuilder.GetRowActionButtonWidth(actionHeight, UIButtonStyle.Orange, 148f);
        RuntimeUIBuilder.CreateButton(row, "WatchButton", "광고 시청", new Vector2(1f, 0.5f), new Vector2(-16f, 0f),
            new Vector2(buttonWidth, actionHeight), UIButtonStyle.Orange, () => StartRewardAd(offer));

        Transform watchTransform = row.Find("WatchButton");
        if (watchTransform != null && watchTransform.TryGetComponent(out Button watchButton))
            watchButtons.Add(watchButton);
    }

    void SetRewardFooterVisible(bool visible)
    {
        if (rewardCloseButton == null) return;
        rewardCloseButton.gameObject.SetActive(visible);
        rewardCloseButton.interactable = visible;
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
            inputBlocker.SetActive(locked);
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
        SetRewardFooterVisible(false);

        playbackMessageText.text = $"광고 재생 중...\n{offer.displayName}";
        resultPanel.SetActive(false);
        RuntimeUIBuilder.BringModalToFront(playbackPanel);

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
        RuntimeUIBuilder.ApplyModalMessageText(resultMessageText, message,
            success ? RuntimeUIBuilder.ModalMessageKind.Success : RuntimeUIBuilder.ModalMessageKind.Failure);
        SetRewardUiLocked(true);
        SetRewardFooterVisible(false);
        RuntimeUIBuilder.BringModalToFront(resultPanel);
    }

    void CloseResult()
    {
        resultPanel.SetActive(false);
        SetRewardUiLocked(false);
        SetRewardFooterVisible(true);
    }

    static void StretchInputBlocker(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static GameObject CreateInputBlocker(Transform parent)
    {
        var go = new GameObject("AdInputBlocker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        StretchInputBlocker(rect);

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
}
