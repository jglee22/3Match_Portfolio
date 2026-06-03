using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance;

    public float totalTime = 60f;
    public TextMeshProUGUI timerText;

    private float remainingTime;
    private bool isRunning;

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
        if (!isRunning || GameManager.Instance.isGameOver) return;

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
            GameManager.Instance.EndGameClear();
        else
            GameManager.Instance.EndGameFail();
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = $"Time: {Mathf.CeilToInt(remainingTime)}";
    }

    public void StartTimer()
    {
        remainingTime = totalTime;
        isRunning = true;
        UpdateTimerUI();
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void SetTotalTime(float seconds)
    {
        totalTime = seconds;
        remainingTime = seconds;
        UpdateTimerUI();
    }
}
