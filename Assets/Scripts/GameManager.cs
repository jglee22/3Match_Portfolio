using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public Button restartButton;

    public bool isGameOver = false;
    public int goalScore = 1500;

    private bool isGameEnded = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        restartButton.onClick.AddListener(RestartGame);
    }

    /// <summary>목표 점수 달성 등 — 즉시 클리어</summary>
    public void EndGameClear()
    {
        EndGame(cleared: true, reason: "goal");
    }

    /// <summary>시간 종료 시 목표 미달 등 — 실패</summary>
    public void EndGameFail()
    {
        EndGame(cleared: false, reason: "time");
    }

    void EndGame(bool cleared, string reason)
    {
        if (isGameEnded) return;
        isGameEnded = true;
        isGameOver = true;

        if (GameTimer.Instance != null)
            GameTimer.Instance.StopTimer();

        int finalScore = ScoreManager.Instance.GetScore();

        if (cleared)
        {
            finalScoreText.text = $"클리어!\n점수: {finalScore} / {goalScore}";
            if (SoundManager.Instance != null && SoundManager.Instance.clearBGM != null)
                SoundManager.Instance.PlayBGM(SoundManager.Instance.clearBGM);
        }
        else
        {
            finalScoreText.text = $"실패\n점수: {finalScore} / {goalScore}";
            if (SoundManager.Instance != null && SoundManager.Instance.failBGM != null)
                SoundManager.Instance.PlayBGM(SoundManager.Instance.failBGM);
        }

        gameOverPanel.SetActive(true);
        Debug.Log($"[GameManager] 종료 — {(cleared ? "클리어" : "실패")} ({reason})");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
