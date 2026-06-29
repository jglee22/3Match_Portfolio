using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GridManager gridManager;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverTitleText;
    public TextMeshProUGUI finalScoreText;
    public Button restartButton;
    public TextMeshProUGUI restartButtonText;

    public bool isGameOver = false;
    public int goalScore = 1500;

    [SerializeField] float nextRoundTransitionSeconds = 0.75f;
    [SerializeField] bool testReturnToLobbyOnClear = true;

    int currentLevelIndex;
    bool isGameEnded;
    bool useNextRoundAction;
    bool pendingClear;
    bool isTransitioningRound;
    RunReward lastRunReward;
    EndPanelView endPanelView;
    Canvas screenTransitionCanvas;
    Image roundTransitionOverlay;
    static Sprite fullscreenSprite;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (gridManager == null)
            gridManager = FindAnyObjectByType<GridManager>();

        EnsurePersistentManagers();

        if (gameOverPanel != null)
        {
            endPanelView = gameOverPanel.GetComponent<EndPanelView>();
            if (endPanelView == null)
                endPanelView = gameOverPanel.AddComponent<EndPanelView>();
        }
    }

    void Start()
    {
        currentLevelIndex = gridManager != null ? gridManager.startLevelIndex : 0;
        restartButton.onClick.AddListener(OnPanelButtonClicked);
        endPanelView?.Configure(gameOverTitleText, finalScoreText, restartButtonText);
        EnsureShuffleItemUI();
    }

    void EnsurePersistentManagers()
    {
        if (GameDataManager.Instance == null)
            new GameObject("GameDataManager").AddComponent<GameDataManager>();

        if (CurrencyManager.Instance == null)
            new GameObject("CurrencyManager").AddComponent<CurrencyManager>();

        if (GameServicesBootstrap.Instance == null)
            GameServicesBootstrap.Ensure();
    }

    void EnsureShuffleItemUI()
    {
        if (FindAnyObjectByType<ShuffleItemUI>() == null)
            new GameObject("ShuffleItemUI").AddComponent<ShuffleItemUI>();
    }

    void OnPanelButtonClicked()
    {
        if (isTransitioningRound) return;

        if (useNextRoundAction)
            StartCoroutine(NextRoundRoutine());
        else
            StartCoroutine(ReturnToLobbyRoutine());
    }

    public void RequestClearWhenReady()
    {
        if (isGameEnded || pendingClear) return;

        pendingClear = true;
        TryFinalizePendingClear();
    }

    public void TryFinalizePendingClear()
    {
        if (!pendingClear || isGameEnded) return;
        if (gridManager != null && gridManager.IsProcessing) return;

        pendingClear = false;
        EndGameClear();
    }

    public void EndGameClear()
    {
        EndGame(cleared: true);
    }

    public void EndGameFail()
    {
        EndGame(cleared: false);
    }

    void EndGame(bool cleared)
    {
        if (isGameEnded) return;
        isGameEnded = true;
        isGameOver = true;
        pendingClear = false;

        if (GameTimer.Instance != null)
            GameTimer.Instance.StopTimer();

        int finalScore = ScoreManager.Instance.GetScore();
        bool hasNextRound = cleared && gridManager != null && gridManager.HasNextRound(currentLevelIndex);
        bool isAllClear = cleared && !hasNextRound && gridManager != null && gridManager.TotalRoundCount > 0;

        int roundNumber = currentLevelIndex + 1;
        lastRunReward = RewardCalculator.Calculate(cleared, finalScore, goalScore, isAllClear, roundNumber);
        ApplyRunReward(lastRunReward);

        string title;
        string buttonLabel;
        if (cleared)
        {
            bool canContinueRound = hasNextRound || gridManager == null || gridManager.TotalRoundCount == 0;
            useNextRoundAction = !testReturnToLobbyOnClear && canContinueRound;
            title = isAllClear ? "All Clear!" : "Round Clear!";
            buttonLabel = useNextRoundAction ? "다음 라운드" : "로비";

            if (SoundManager.Instance != null && SoundManager.Instance.clearBGM != null)
                SoundManager.Instance.PlayBGM(SoundManager.Instance.clearBGM);
        }
        else
        {
            useNextRoundAction = false;
            title = "Game Over!";
            buttonLabel = "로비";

            if (SoundManager.Instance != null && SoundManager.Instance.failBGM != null)
                SoundManager.Instance.PlayBGM(SoundManager.Instance.failBGM);
        }

        string scoreLine = BuildResultText(finalScore, cleared, hasNextRound, isAllClear, lastRunReward);

        if (endPanelView != null)
            endPanelView.Present(cleared, isAllClear, title, scoreLine, buttonLabel);
        else
            ApplyLegacyPanel(cleared, title, scoreLine, buttonLabel);
    }

    void ApplyLegacyPanel(bool cleared, string title, string scoreLine, string buttonLabel)
    {
        if (gameOverTitleText != null)
            gameOverTitleText.text = title;
        if (finalScoreText != null)
            finalScoreText.text = scoreLine;
        if (restartButtonText != null)
            restartButtonText.text = buttonLabel;
        gameOverPanel.SetActive(true);
    }

    void ApplyRunReward(RunReward reward)
    {
        if (!reward.HasReward || GameDataManager.Instance == null) return;

        GameDataManager.Instance.AddGold(reward.gold);
        if (reward.gems > 0)
            GameDataManager.Instance.AddGems(reward.gems);

        CurrencyManager.Instance?.RefreshUI();
    }

    string BuildResultText(int finalScore, bool cleared, bool hasNextRound, bool isAllClear, RunReward reward)
    {
        string scoreLine = BuildScoreLine(finalScore, cleared, hasNextRound, isAllClear);
        if (!reward.HasReward)
            return scoreLine;

        return $"{scoreLine}\n\n보상: {reward.ToDisplayLine()}";
    }

    string BuildScoreLine(int finalScore, bool cleared, bool hasNextRound, bool isAllClear)
    {
        if (gridManager != null && gridManager.TotalRoundCount > 0 && cleared)
        {
            if (hasNextRound)
                return $"라운드 {currentLevelIndex + 1} / {gridManager.TotalRoundCount}\n점수: {finalScore} / {goalScore}";

            return $"점수: {finalScore} / {goalScore}";
        }

        return $"점수: {finalScore} / {goalScore}";
    }

    IEnumerator NextRoundRoutine()
    {
        isTransitioningRound = true;
        if (restartButton != null)
            restartButton.interactable = false;

        float panelFade = nextRoundTransitionSeconds * 0.45f;
        float blackHold = nextRoundTransitionSeconds * 0.2f;
        float blackFadeIn = nextRoundTransitionSeconds * 0.35f;
        float blackFadeOut = nextRoundTransitionSeconds * 0.4f;

        yield return PlayPanelFadeOut(panelFade);
        yield return PlayBlackFadeIn(blackFadeIn);

        if (gridManager == null)
        {
            yield return PlayBlackFadeOut(blackFadeOut);
            FinishRoundTransition();
            yield break;
        }

        if (gridManager.TotalRoundCount > 0 && !gridManager.HasNextRound(currentLevelIndex))
        {
            yield return PlayBlackFadeOut(blackFadeOut);
            FinishRoundTransition();
            yield break;
        }

        if (gridManager.TotalRoundCount > 0)
            currentLevelIndex++;

        ResetSessionState();

        gridManager.LoadRound(Mathf.Clamp(currentLevelIndex, 0, Mathf.Max(0, gridManager.TotalRoundCount - 1)));
        ScoreManager.Instance.ResetScore();
        GameTimer.Instance?.StartTimer();
        ScoreManager.Instance?.RefreshUI();

        if (SoundManager.Instance != null && SoundManager.Instance.defaultBGM != null)
            SoundManager.Instance.PlayBGM(SoundManager.Instance.defaultBGM);

        if (blackHold > 0f)
            yield return new WaitForSeconds(blackHold);

        yield return PlayBlackFadeOut(blackFadeOut);
        FinishRoundTransition();
    }

    IEnumerator ReturnToLobbyRoutine()
    {
        isTransitioningRound = true;
        if (restartButton != null)
            restartButton.interactable = false;

        float panelFade = nextRoundTransitionSeconds * 0.45f;
        float blackFadeIn = nextRoundTransitionSeconds * 0.35f;

        yield return PlayPanelFadeOut(panelFade);
        yield return PlayBlackFadeIn(blackFadeIn);

        CleanupBeforeSceneLoad();
        SceneLoader.LoadLobby();
    }

    IEnumerator PlayPanelFadeOut(float duration)
    {
        if (endPanelView != null && gameOverPanel != null && gameOverPanel.activeInHierarchy)
            yield return WaitForTween(endPanelView.FadeOut(duration));
        else if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            yield return new WaitForSeconds(duration);
        }
    }

    IEnumerator PlayBlackFadeIn(float duration)
    {
        SetGameplayHudVisible(false);

        Image overlay = EnsureRoundTransitionOverlay();
        overlay.transform.SetAsLastSibling();
        overlay.DOKill();
        overlay.gameObject.SetActive(true);
        overlay.color = new Color(0f, 0f, 0f, 0f);
        yield return WaitForTween(overlay.DOFade(1f, duration).SetUpdate(true).SetLink(overlay.gameObject));
    }

    IEnumerator PlayBlackFadeOut(float duration)
    {
        if (roundTransitionOverlay == null)
        {
            SetGameplayHudVisible(true);
            yield break;
        }

        roundTransitionOverlay.DOKill();
        yield return WaitForTween(
            roundTransitionOverlay.DOFade(0f, duration).SetUpdate(true).SetLink(roundTransitionOverlay.gameObject));
        roundTransitionOverlay.gameObject.SetActive(false);
        SetGameplayHudVisible(true);
    }

    void SetGameplayHudVisible(bool visible)
    {
        if (GameTimer.Instance != null && GameTimer.Instance.timerText != null)
            GameTimer.Instance.timerText.gameObject.SetActive(visible);

        if (ScoreManager.Instance != null && ScoreManager.Instance.scoreText != null)
            ScoreManager.Instance.scoreText.gameObject.SetActive(visible);
    }

    void CleanupBeforeSceneLoad()
    {
        SetGameplayHudVisible(true);
        endPanelView?.StopAllTweens();
        GameTimer.Instance?.StopAllTweens();
        roundTransitionOverlay?.DOKill();
        DOTween.KillAll(complete: false);
    }

    IEnumerator WaitForTween(Tween tween)
    {
        if (tween == null) yield break;
        yield return tween.WaitForCompletion();
    }

    void FinishRoundTransition()
    {
        isTransitioningRound = false;
        if (restartButton != null)
            restartButton.interactable = true;
    }

    Image EnsureRoundTransitionOverlay()
    {
        if (roundTransitionOverlay != null)
            return roundTransitionOverlay;

        Canvas canvas = EnsureScreenTransitionCanvas();

        var overlayGo = new GameObject("ScreenFadeOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        overlayGo.transform.SetParent(canvas.transform, false);

        var rect = overlayGo.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;

        roundTransitionOverlay = overlayGo.GetComponent<Image>();
        roundTransitionOverlay.raycastTarget = true;
        roundTransitionOverlay.sprite = GetFullscreenSprite();
        roundTransitionOverlay.type = Image.Type.Simple;
        roundTransitionOverlay.color = new Color(0f, 0f, 0f, 0f);
        overlayGo.SetActive(false);
        return roundTransitionOverlay;
    }

    Canvas EnsureScreenTransitionCanvas()
    {
        if (screenTransitionCanvas != null)
            return screenTransitionCanvas;

        var canvasGo = new GameObject("ScreenTransitionCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        screenTransitionCanvas = canvasGo.GetComponent<Canvas>();
        screenTransitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        screenTransitionCanvas.overrideSorting = true;
        screenTransitionCanvas.sortingOrder = 1000;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1f;

        return screenTransitionCanvas;
    }

    static Sprite GetFullscreenSprite()
    {
        if (fullscreenSprite != null)
            return fullscreenSprite;

        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        fullscreenSprite = Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        return fullscreenSprite;
    }

    void ResetSessionState()
    {
        isGameEnded = false;
        isGameOver = false;
        useNextRoundAction = false;
        pendingClear = false;
    }

    public void ReturnToLobby()
    {
        StartCoroutine(ReturnToLobbyRoutine());
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
        CleanupBeforeSceneLoad();
    }
}
