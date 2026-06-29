using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>3Match 씬 — Shuffle Item 사용 버튼</summary>
public class ShuffleItemUI : MonoBehaviour
{
    GridManager gridManager;
    Button shuffleButton;
    TextMeshProUGUI buttonLabel;
    TextMeshProUGUI feedbackText;
    float feedbackTimer;

    void Awake()
    {
        gridManager = FindAnyObjectByType<GridManager>();
    }

    void Start()
    {
        if (GameDataManager.Instance == null)
            new GameObject("GameDataManager").AddComponent<GameDataManager>();

        EnsureUI();
        RefreshLabel();

        if (GameDataManager.Instance != null)
            GameDataManager.Instance.OnInventoryChanged += RefreshLabel;
    }

    void OnDestroy()
    {
        if (GameDataManager.Instance != null)
            GameDataManager.Instance.OnInventoryChanged -= RefreshLabel;
    }

    void Update()
    {
        if (feedbackText == null || feedbackTimer <= 0f) return;

        feedbackTimer -= Time.deltaTime;
        if (feedbackTimer <= 0f)
            feedbackText.gameObject.SetActive(false);
    }

    void EnsureUI()
    {
        var canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null || shuffleButton != null) return;

        var go = new GameObject("ShuffleButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(canvas.transform, false);

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-24f, -24f);
        rect.sizeDelta = new Vector2(240f, 56f);

        go.GetComponent<Image>().color = new Color(0.22f, 0.48f, 0.32f, 1f);
        shuffleButton = go.GetComponent<Button>();
        shuffleButton.onClick.AddListener(OnShuffleClicked);

        buttonLabel = CreateText(go.transform, "Label", "Shuffle (0)", 24);

        var feedbackGo = new GameObject("ShuffleFeedback", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        feedbackGo.transform.SetParent(canvas.transform, false);
        var fbRect = feedbackGo.GetComponent<RectTransform>();
        fbRect.anchorMin = new Vector2(1f, 1f);
        fbRect.anchorMax = new Vector2(1f, 1f);
        fbRect.pivot = new Vector2(1f, 1f);
        fbRect.anchoredPosition = new Vector2(-24f, -88f);
        fbRect.sizeDelta = new Vector2(320f, 40f);

        feedbackText = feedbackGo.GetComponent<TextMeshProUGUI>();
        feedbackText.fontSize = 22;
        feedbackText.alignment = TextAlignmentOptions.MidlineRight;
        feedbackText.color = new Color(1f, 0.55f, 0.55f);
        feedbackText.gameObject.SetActive(false);
    }

    void OnShuffleClicked()
    {
        if (gridManager == null)
            gridManager = FindAnyObjectByType<GridManager>();

        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
        {
            ShowFeedback("게임 종료 후에는 사용할 수 없습니다.");
            return;
        }

        if (gridManager != null && gridManager.IsProcessing)
        {
            ShowFeedback("연쇄 처리 중에는 사용할 수 없습니다.");
            return;
        }

        if (gridManager != null && gridManager.TryUseShuffleItem())
        {
            RefreshLabel();
            ShowFeedback("셔플 사용!", success: true);
            return;
        }

        ShowFeedback("Shuffle Item이 없습니다.");
    }

    void RefreshLabel()
    {
        if (buttonLabel == null) return;
        int count = GameDataManager.Instance != null ? GameDataManager.Instance.ShuffleItems : 0;
        buttonLabel.text = $"Shuffle ({count})";
    }

    void ShowFeedback(string message, bool success = false)
    {
        if (feedbackText == null) return;

        feedbackText.text = message;
        feedbackText.color = success
            ? new Color(0.55f, 0.95f, 0.7f)
            : new Color(1f, 0.55f, 0.55f);
        feedbackText.gameObject.SetActive(true);
        feedbackTimer = 2f;
    }

    static TextMeshProUGUI CreateText(Transform parent, string name, string text, int fontSize)
    {
        var labelGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(parent, false);
        var labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        var label = labelGo.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        return label;
    }
}
