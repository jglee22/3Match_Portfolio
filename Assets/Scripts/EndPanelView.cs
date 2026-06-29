using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>클리어/실패 결과 패널 레이아웃·연출</summary>
public class EndPanelView : MonoBehaviour
{
    static readonly Color DimColor = new(0f, 0f, 0f, 0.68f);
    static readonly Color ClearCardColor = new(0.07f, 0.13f, 0.22f, 0.96f);
    static readonly Color FailCardColor = new(0.2f, 0.07f, 0.09f, 0.96f);
    static readonly Color ClearTitleColor = new(0.45f, 0.95f, 0.78f);
    static readonly Color FailTitleColor = new(1f, 0.42f, 0.42f);

    Image dimOverlay;
    RectTransform resultCard;
    Image cardBackground;
    CanvasGroup canvasGroup;

    TextMeshProUGUI titleText;
    TextMeshProUGUI scoreText;
    TextMeshProUGUI buttonText;

    bool layoutReady;

    public void Configure(TextMeshProUGUI title, TextMeshProUGUI score, TextMeshProUGUI buttonLabel)
    {
        titleText = title;
        scoreText = score;
        buttonText = buttonLabel;
        EnsureLayout();
    }

    void EnsureLayout()
    {
        if (layoutReady) return;
        layoutReady = true;

        dimOverlay = GetComponent<Image>();
        if (dimOverlay != null)
        {
            dimOverlay.color = DimColor;
            dimOverlay.raycastTarget = true;
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        resultCard = transform.Find("ResultCard") as RectTransform;
        if (resultCard == null)
        {
            var cardGo = new GameObject("ResultCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            resultCard = cardGo.GetComponent<RectTransform>();
            resultCard.SetParent(transform, false);
            resultCard.SetAsFirstSibling();

            resultCard.anchorMin = new Vector2(0.5f, 0.5f);
            resultCard.anchorMax = new Vector2(0.5f, 0.5f);
            resultCard.pivot = new Vector2(0.5f, 0.5f);
            resultCard.anchoredPosition = Vector2.zero;
            resultCard.sizeDelta = new Vector2(520f, 320f);

            cardBackground = cardGo.GetComponent<Image>();
            if (dimOverlay != null && dimOverlay.sprite != null)
            {
                cardBackground.sprite = dimOverlay.sprite;
                cardBackground.type = Image.Type.Sliced;
            }
            cardBackground.color = ClearCardColor;
        }
        else
        {
            cardBackground = resultCard.GetComponent<Image>();
        }

        ReparentToCard(titleText?.rectTransform, new Vector2(0f, 88f), new Vector2(460f, 64f), 42f);
        ReparentToCard(scoreText?.rectTransform, new Vector2(0f, 8f), new Vector2(460f, 72f), 30f);
        ReparentToCard(buttonText?.transform.parent as RectTransform, new Vector2(0f, -88f), new Vector2(240f, 72f), 28f);
    }

    void ReparentToCard(RectTransform target, Vector2 anchoredPos, Vector2 size, float fontSize)
    {
        if (target == null) return;

        target.SetParent(resultCard, false);
        target.anchorMin = new Vector2(0.5f, 0.5f);
        target.anchorMax = new Vector2(0.5f, 0.5f);
        target.pivot = new Vector2(0.5f, 0.5f);
        target.anchoredPosition = anchoredPos;
        target.sizeDelta = size;

        var tmp = target.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp != null)
        {
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
        }
    }

    public void Present(bool cleared, bool isAllClear, string title, string scoreLine, string buttonLabel)
    {
        EnsureLayout();

        if (titleText != null)
        {
            titleText.text = title;
            titleText.color = cleared ? ClearTitleColor : FailTitleColor;
        }

        if (scoreText != null)
            scoreText.text = scoreLine;

        if (buttonText != null)
            buttonText.text = buttonLabel;

        if (cardBackground != null)
            cardBackground.color = cleared ? ClearCardColor : FailCardColor;

        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        resultCard.localScale = Vector3.one * 0.9f;

        canvasGroup.DOKill();
        resultCard.DOKill();
        canvasGroup.DOFade(1f, 0.35f).SetLink(gameObject);
        resultCard.DOScale(1f, 0.45f).SetEase(Ease.OutBack).SetLink(gameObject);
    }

    public void StopAllTweens()
    {
        canvasGroup?.DOKill();
        resultCard?.DOKill();
        dimOverlay?.DOKill();
    }

    public Tween FadeOut(float duration)
    {
        EnsureLayout();

        canvasGroup.DOKill();
        resultCard.DOKill();
        dimOverlay?.DOKill();

        var seq = DOTween.Sequence();
        seq.Join(canvasGroup.DOFade(0f, duration));
        seq.Join(resultCard.DOScale(0.82f, duration).SetEase(Ease.InBack));

        if (dimOverlay != null)
        {
            Color c = dimOverlay.color;
            seq.Join(dimOverlay.DOColor(new Color(c.r, c.g, c.b, 0f), duration));
        }

        return seq.SetLink(gameObject).OnComplete(() =>
        {
            if (this == null) return;

            gameObject.SetActive(false);
            canvasGroup.alpha = 1f;
            resultCard.localScale = Vector3.one;
            if (dimOverlay != null)
            {
                Color c = dimOverlay.color;
                dimOverlay.color = new Color(c.r, c.g, c.b, DimColor.a);
            }
        });
    }

    void OnDisable()
    {
        StopAllTweens();
    }

    void OnDestroy()
    {
        StopAllTweens();
    }
}
