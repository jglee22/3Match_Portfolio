using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>클리어/실패 결과 패널 레이아웃·연출</summary>
public class EndPanelView : MonoBehaviour
{
    const float PanelTargetWidth = 560f;

    static readonly Color DimColor = new(0f, 0f, 0f, 0.82f);

    Image dimOverlay;
    RectTransform resultCard;
    RectTransform contentRoot;
    Image cardBackground;
    Image panelFrameImage;
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
            var cardGo = new GameObject("ResultCard", typeof(RectTransform));
            resultCard = cardGo.GetComponent<RectTransform>();
            resultCard.SetParent(transform, false);
            resultCard.SetAsFirstSibling();

            resultCard.anchorMin = new Vector2(0.5f, 0.5f);
            resultCard.anchorMax = new Vector2(0.5f, 0.5f);
            resultCard.pivot = new Vector2(0.5f, 0.5f);
            resultCard.anchoredPosition = Vector2.zero;
            resultCard.sizeDelta = new Vector2(PanelTargetWidth, 380f);
        }

        EnsureCardChrome();
        EnsureContentRoot();

        ReparentToCard(titleText?.rectTransform, new Vector2(0f, 64f), new Vector2(460f, 56f), 42f);
        ReparentToCard(scoreText?.rectTransform, new Vector2(0f, -8f), new Vector2(460f, 96f), 34f);
        ReparentToCard(buttonText?.transform.parent as RectTransform, new Vector2(0f, -92f), new Vector2(280f, 52f), 26f);

        if (buttonText != null)
        {
            buttonText.color = Color.white;
            var buttonImage = buttonText.transform.parent.GetComponent<Image>();
            if (buttonImage != null)
                RuntimeUIBuilder.StyleActionButton(buttonImage, UIButtonStyle.Green);
        }
    }

    void EnsureCardChrome()
    {
        var backing = resultCard.Find("PanelBacking") as RectTransform;
        if (backing == null)
        {
            var backingGo = new GameObject("PanelBacking", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            backing = backingGo.GetComponent<RectTransform>();
            backing.SetParent(resultCard, false);
            backing.SetAsFirstSibling();
            Stretch(backing);
            cardBackground = backingGo.GetComponent<Image>();
            cardBackground.color = RuntimeUIBuilder.Popup.PanelFill;
            cardBackground.raycastTarget = true;
        }
        else
        {
            cardBackground = backing.GetComponent<Image>();
        }

        var frame = resultCard.Find("PanelFrame") as RectTransform;
        if (frame == null)
        {
            var frameGo = new GameObject("PanelFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            frame = frameGo.GetComponent<RectTransform>();
            frame.SetParent(resultCard, false);
            frame.SetSiblingIndex(1);
            Stretch(frame);
            panelFrameImage = frameGo.GetComponent<Image>();
        }
        else
        {
            panelFrameImage = frame.GetComponent<Image>();
        }
    }

    void ApplyPanelArt(bool cleared)
    {
        var ui = HyperCasualUIAssets.Instance;
        Sprite panelSprite = cleared
            ? ui?.clearPanel ?? ui?.popupPanel
            : ui?.failPanel ?? ui?.popupPanel;

        if (panelSprite != null && panelFrameImage != null)
        {
            RuntimeUIBuilder.ApplyPanelArt(panelFrameImage, panelSprite);
            resultCard.sizeDelta = RuntimeUIBuilder.GetSpriteSize(panelSprite, PanelTargetWidth);
            if (cardBackground != null)
                cardBackground.enabled = false;
            return;
        }

        if (panelFrameImage != null)
        {
            panelFrameImage.sprite = null;
            panelFrameImage.enabled = false;
        }

        if (cardBackground != null)
        {
            cardBackground.enabled = true;
            cardBackground.color = RuntimeUIBuilder.Popup.PanelFill;
        }
    }

    void EnsureContentRoot()
    {
        contentRoot = resultCard.Find("Content") as RectTransform;
        if (contentRoot == null)
        {
            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentRoot = contentGo.GetComponent<RectTransform>();
            contentRoot.SetParent(resultCard, false);
            contentRoot.SetAsLastSibling();
            Stretch(contentRoot);
        }
    }

    void ReparentToCard(RectTransform target, Vector2 anchoredPos, Vector2 size, float fontSize, Color? color = null)
    {
        if (target == null) return;

        target.SetParent(contentRoot, false);
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
            if (color.HasValue)
                tmp.color = color.Value;
        }
    }

    void ApplyScoreText(string scoreLine, bool cleared)
    {
        if (scoreText == null) return;

        scoreText.richText = true;
        scoreText.fontStyle = FontStyles.Bold;
        scoreText.lineSpacing = -4f;

        const string rewardPrefix = "보상:";
        int rewardIndex = scoreLine.IndexOf(rewardPrefix, System.StringComparison.Ordinal);
        if (rewardIndex < 0)
        {
            scoreText.text = scoreLine;
            scoreText.color = RuntimeUIBuilder.Popup.ArtPanelBody;
            return;
        }

        string head = scoreLine.Substring(0, rewardIndex).TrimEnd();
        string reward = scoreLine.Substring(rewardIndex).Trim();
        string bodyHex = ColorUtility.ToHtmlStringRGB(RuntimeUIBuilder.Popup.ArtPanelBody);
        Color rewardColor = cleared
            ? RuntimeUIBuilder.Popup.ArtPanelClearReward
            : RuntimeUIBuilder.Popup.ArtPanelReward;
        string rewardHex = ColorUtility.ToHtmlStringRGB(rewardColor);
        scoreText.color = Color.white;
        scoreText.text = $"<size=105%><color=#{bodyHex}>{head}</color></size>\n<color=#{rewardHex}>{reward}</color>";
    }

    public void Present(bool cleared, bool isAllClear, string title, string scoreLine, string buttonLabel)
    {
        EnsureLayout();
        ApplyPanelArt(cleared);

        if (titleText != null)
        {
            titleText.text = title;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = cleared
                ? RuntimeUIBuilder.Popup.ArtPanelClearTitle
                : RuntimeUIBuilder.Popup.ArtPanelTitle;
            titleText.fontSize = cleared ? (isAllClear ? 44f : 42f) : 40f;
        }

        ApplyScoreText(scoreLine, cleared);

        if (buttonText != null)
            buttonText.text = buttonLabel;

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

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
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
