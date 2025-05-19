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

    public int goalScore = 1500; // 목표 점수
    private bool isGameEnded = false; // 클리어 or 실패 시 중복 처리 방지

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        restartButton.onClick.AddListener(() =>
        {
            RestartGame();
        });
    }

    public void ShowGameOver()
    {
        int finalScore = ScoreManager.Instance.GetScore(); // 점수 가져오기
        finalScoreText.text = $"최종 점수: {finalScore}";
        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ShowGameClear()
    {
        int finalScore = ScoreManager.Instance.GetScore();
        finalScoreText.text = $"클리어! 점수: {finalScore}";
        gameOverPanel.SetActive(true);
    }
}
