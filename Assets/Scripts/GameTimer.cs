using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance;

    public float totalTime = 60f;
    public TextMeshProUGUI timerText;

    [SerializeField] float warningThreshold = 10f;
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color warningColor = new(1f, 0.28f, 0.28f);
    [SerializeField] Color warningFlashColor = new(1f, 0.92f, 0.55f);

    float remainingTime;
    bool isRunning;
    bool warningActive;
    Sequence warningPulseSequence;
    Tween warningColorTween;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartTimer();
    }

    void Update()
    {
        if (!isRunning || GameManager.Instance == null || GameManager.Instance.isGameOver) return;
        if (!IsTimerTextAlive()) return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isRunning = false;
            OnTimeExpired();
        }

        UpdateTimerUI();
    }

    void OnTimeExpired()
    {
        if (ScoreManager.Instance.GetScore() >= GameManager.Instance.goalScore)
            GameManager.Instance.RequestClearWhenReady();
        else
            GameManager.Instance.EndGameFail();
    }

    void UpdateTimerUI()
    {
        if (!IsTimerTextAlive()) return;

        int seconds = Mathf.CeilToInt(remainingTime);
        bool shouldWarn = isRunning && remainingTime <= warningThreshold && remainingTime > 0f;

        if (shouldWarn)
        {
            timerText.text = $"Time: {seconds} !";
            if (!warningActive)
                EnterWarningState();
        }
        else
        {
            timerText.text = $"Time: {seconds}";
            if (warningActive)
                ExitWarningState();
        }
    }

    void EnterWarningState()
    {
        if (!IsTimerTextAlive()) return;

        warningActive = true;
        timerText.color = warningColor;

        StopWarningTweens();

        warningPulseSequence = DOTween.Sequence()
            .Append(timerText.transform.DOScale(1.2f, 0.28f).SetEase(Ease.OutQuad))
            .Append(timerText.transform.DOScale(1f, 0.28f).SetEase(Ease.InQuad))
            .SetLoops(-1)
            .SetLink(timerText.gameObject);

        warningColorTween = timerText
            .DOColor(warningFlashColor, 0.32f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetLink(timerText.gameObject);
    }

    void ExitWarningState()
    {
        warningActive = false;
        ResetWarningVisual();
    }

    public void StartTimer()
    {
        remainingTime = totalTime;
        isRunning = true;
        ResetWarningVisual();
        UpdateTimerUI();
    }

    public void StopTimer()
    {
        isRunning = false;
        ResetWarningVisual();
    }

    public void SetTotalTime(float seconds)
    {
        totalTime = seconds;
        remainingTime = seconds;
        ResetWarningVisual();
        UpdateTimerUI();
    }

    public void StopAllTweens()
    {
        warningActive = false;
        StopWarningTweens();
    }

    void ResetWarningVisual()
    {
        warningActive = false;
        StopWarningTweens();

        if (!IsTimerTextAlive()) return;

        timerText.color = normalColor;
        timerText.transform.localScale = Vector3.one;
    }

    void StopWarningTweens()
    {
        warningPulseSequence?.Kill();
        warningColorTween?.Kill();
        warningPulseSequence = null;
        warningColorTween = null;

        if (!IsTimerTextAlive()) return;

        timerText.transform.DOKill();
        timerText.DOKill();
    }

    bool IsTimerTextAlive() => timerText != null;

    void OnDisable()
    {
        StopAllTweens();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
        StopAllTweens();
    }
}
