using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public TextMeshProUGUI scoreText;
    private int currentScore;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        RefreshScoreUI();
    }

    public void AddScore(int amount)
    {
        if (GameManager.Instance.isGameOver) return;

        currentScore += amount;
        RefreshScoreUI();

        if (currentScore >= GameManager.Instance.goalScore)
            GameManager.Instance.RequestClearWhenReady();
    }

    public void ResetScore()
    {
        currentScore = 0;
        RefreshScoreUI();
    }

    public int GetScore() => currentScore;

    public void RefreshUI() => RefreshScoreUI();

    void RefreshScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {currentScore} / {GameManager.Instance.goalScore}";
    }
}
