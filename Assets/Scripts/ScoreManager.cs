using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public TextMeshProUGUI scoreText;  // 점수 텍스트
    private int currentScore = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        scoreText.text = $"Score: {currentScore} / {GameManager.Instance.goalScore}";
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        scoreText.text = $"Score: {currentScore} / {GameManager.Instance.goalScore}";
    }

    public void ResetScore()
    {
        currentScore = 0;
        scoreText.text = $"Score: {currentScore}";
    }
    public int GetScore()
    {
        return currentScore;
    }
}
