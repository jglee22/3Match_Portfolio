using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public float totalTime = 60f;                 // 총 제한 시간 (초)
    public TextMeshProUGUI timerText;             // TMP 텍스트 UI
    private float remainingTime;
    private bool isRunning = false;

    public System.Action OnTimeOver;              // 시간 종료 이벤트

    void Start()
    {
        StartTimer();

        OnTimeOver = () =>
        {
            FindAnyObjectByType<GameManager>().isGameOver = true;
            FindAnyObjectByType<GameManager>().ShowGameOver();
        };
    }

    void Update()
    {
        if (!isRunning) return;

        remainingTime -= Time.deltaTime;
        if (remainingTime < 0)
        {
            remainingTime = 0;
            isRunning = false;
            OnTimeOver?.Invoke(); // 시간 종료 처리
        }

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
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
}
