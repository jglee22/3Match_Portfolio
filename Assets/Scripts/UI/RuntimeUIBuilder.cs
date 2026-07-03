using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>런타임 UI 생성 — Hyper Casual UI 스프라이트 적용</summary>
public static class RuntimeUIBuilder
{
    static readonly Color FallbackDim = new(0f, 0f, 0f, 0.72f);
    static readonly Color FallbackPanel = new(0.1f, 0.14f, 0.22f, 0.98f);
    static readonly Color FallbackRow = new(0.14f, 0.2f, 0.3f, 0.95f);

    public static class Popup
    {
        public static readonly Color PanelFill = new(0.96f, 0.97f, 0.99f, 1f);
        public static readonly Color PanelTint = new(1f, 1f, 1f, 1f);
        public static readonly Color Title = new(0.04f, 0.1f, 0.24f, 1f);
        public static readonly Color Body = new(0.12f, 0.17f, 0.28f, 1f);
        public static readonly Color Subtitle = new(0.14f, 0.2f, 0.34f, 1f);
        public static readonly Color Muted = new(0.34f, 0.4f, 0.5f, 1f);
        public static readonly Color Accent = new(0.08f, 0.38f, 0.72f, 1f);
        public static readonly Color Row = new(0.8f, 0.87f, 0.95f, 0.98f);
        public static readonly Color RowHighlight = new(0.72f, 0.82f, 0.98f, 1f);
        public static readonly Color RowAccent = new(0.92f, 0.58f, 0.12f, 1f);
        public static readonly Color Success = new(0.12f, 0.45f, 0.28f, 1f);
        public static readonly Color Error = new(0.72f, 0.22f, 0.22f, 1f);
        public static readonly Color ClearTitle = new(0.1f, 0.48f, 0.32f, 1f);
        public static readonly Color FailTitle = new(0.78f, 0.2f, 0.2f, 1f);
        public static readonly Color RewardGold = new(0.84f, 0.54f, 0.02f, 1f);
        public static readonly Color RewardGem = new(0.5f, 0.24f, 0.92f, 1f);
        public static readonly Color RewardShuffle = new(0.06f, 0.62f, 0.7f, 1f);
        public static readonly Color RewardShuffleOnPanel = new(0.02f, 0.26f, 0.48f, 1f);
        public static readonly Color ArtPanelTitle = new(0.90f, 0.16f, 0.18f, 1f);
        public static readonly Color ArtPanelClearTitle = new(0.04f, 0.58f, 0.36f, 1f);
        public static readonly Color ModalSuccessTitle = new(0.02f, 0.64f, 0.38f, 1f);
        public static readonly Color ArtPanelBody = new(0.22f, 0.14f, 0.18f, 1f);
        public static readonly Color ArtPanelReward = new(0.62f, 0.32f, 0.04f, 1f);
        public static readonly Color ArtPanelClearReward = new(0.72f, 0.48f, 0.04f, 1f);
        public static readonly Color InventoryCount = new(0.06f, 0.28f, 0.36f, 1f);
    }

    public const float ModalInsetX = 52f;
    public const float ModalInsetTop = 88f;
    public const float ModalInsetBottom = 72f;
    public const float ModalRowWidth = 520f;
    public const float ModalHeaderHeight = 88f;
    public const float ModalSubtitleExtra = 40f;
    public const float ModalFooterHeight = 96f;
    public const float ModalCloseButtonY = 32f;
    public const float ModalScrollMaxHeight = 288f;

    public static Vector2 BuildModalOuterSize(float outerWidth, float innerHeight) =>
        new(outerWidth, innerHeight + ModalInsetTop + ModalInsetBottom);

    public const float ModalOverlayFooterHeight = 88f;

    public static Vector2 BuildOverlayModalOuterSize(float outerWidth, float messageAreaHeight) =>
        BuildModalOuterSize(outerWidth, messageAreaHeight + ModalOverlayFooterHeight);

    public static float GetModalInnerWidth(float outerWidth) =>
        outerWidth - ModalInsetX * 2f;

    public static float GetListFirstRowCenterY(float innerHeight, float rowHeight, float headerHeight) =>
        innerHeight * 0.5f - headerHeight - rowHeight * 0.5f;

    public static float GetModalRowY(int index, float rowHeight, float rowGap, float innerHeight, float headerHeight) =>
        GetListFirstRowCenterY(innerHeight, rowHeight, headerHeight) - index * (rowHeight + rowGap);

    public static Sprite GetRewardIcon(HyperCasualUIAssets ui, int rewardGold, int rewardGems, int rewardShuffle)
    {
        if (ui == null) return null;
        if (rewardGems > 0) return ui.iconGem;
        if (rewardShuffle > 0) return ui.iconShuffle;
        return ui.iconCoin;
    }

    public static Color GetRewardColor(int rewardGold, int rewardGems, int rewardShuffle)
    {
        if (rewardGems > 0) return Popup.RewardGem;
        if (rewardShuffle > 0) return Popup.RewardShuffle;
        return Popup.RewardGold;
    }

    static readonly Color[] FallbackButtonColors =
    {
        new(0.2f, 0.55f, 0.35f, 1f),
        new(0.2f, 0.45f, 0.75f, 1f),
        new(0.85f, 0.55f, 0.15f, 1f),
        new(0.35f, 0.38f, 0.45f, 1f),
        new(0.45f, 0.2f, 0.2f, 1f)
    };

    public static HyperCasualUIAssets Assets => HyperCasualUIAssets.Instance;
    public static bool HasSprites => Assets != null;

    public static void ApplySprite(Image image, Sprite sprite, bool preserveAspect = false)
    {
        if (image == null || sprite == null) return;
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.color = Color.white;
        image.preserveAspect = preserveAspect;
    }

    public static void ApplyPanelArt(Image image, Sprite sprite, bool preserveAspect = true)
    {
        if (image == null || sprite == null) return;
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.color = Color.white;
        image.preserveAspect = preserveAspect;
        image.raycastTarget = true;
    }

    public static void ApplyButtonSprite(Image image, Sprite sprite)
    {
        if (image == null || sprite == null) return;
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = false;

        Vector4 border = sprite.border;
        if (border.x > 0f || border.z > 0f)
            image.type = Image.Type.Sliced;
        else
            image.type = Image.Type.Simple;
    }

    public static Vector2 GetSpriteSize(Sprite sprite, float targetWidth)
    {
        if (sprite == null)
            return new Vector2(targetWidth, targetWidth * 0.25f);

        float aspect = sprite.rect.height / sprite.rect.width;
        return new Vector2(targetWidth, targetWidth * aspect);
    }

    public static float GetSpriteWidthForHeight(Sprite sprite, float height)
    {
        if (sprite == null)
            return height * 3.2f;

        return height * (sprite.rect.width / sprite.rect.height);
    }

    public static float GetMaxButtonWidth(float height, params UIButtonStyle[] styles)
    {
        float max = 0f;
        foreach (UIButtonStyle style in styles)
        {
            Sprite sprite = Assets != null ? Assets.GetButton(style) : null;
            max = Mathf.Max(max, GetSpriteWidthForHeight(sprite, height));
        }

        return max > 0f ? max : height * 3.2f;
    }

    public static GameObject CreateModal(Transform parent, string name, Vector2 size, Sprite panelSprite,
        float dimAlpha = 0.82f)
    {
        var dim = new GameObject($"{name}_Dim", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dim.transform.SetParent(parent, false);
        Stretch(dim.GetComponent<RectTransform>());
        var dimImage = dim.GetComponent<Image>();
        dimImage.color = new Color(0f, 0f, 0f, dimAlpha);
        dimImage.raycastTarget = true;

        var backing = new GameObject("PanelBacking", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        backing.transform.SetParent(dim.transform, false);
        var backingRect = backing.GetComponent<RectTransform>();
        backingRect.anchorMin = new Vector2(0.5f, 0.5f);
        backingRect.anchorMax = new Vector2(0.5f, 0.5f);
        backingRect.pivot = new Vector2(0.5f, 0.5f);
        backingRect.sizeDelta = size;
        var backingImage = backing.GetComponent<Image>();
        backingImage.color = Popup.PanelFill;
        backingImage.raycastTarget = true;

        if (panelSprite != null)
        {
            backingImage.enabled = false;

            var frame = new GameObject("PanelFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            frame.transform.SetParent(dim.transform, false);
            var frameRect = frame.GetComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0.5f, 0.5f);
            frameRect.anchorMax = new Vector2(0.5f, 0.5f);
            frameRect.pivot = new Vector2(0.5f, 0.5f);
            frameRect.sizeDelta = size;
            var frameImage = frame.GetComponent<Image>();
            ApplyPanelArt(frameImage, panelSprite, preserveAspect: false);
        }

        var content = new GameObject(name, typeof(RectTransform));
        content.transform.SetParent(dim.transform, false);
        var rect = content.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = panelSprite != null
            ? new Vector2(size.x - ModalInsetX * 2f, size.y - ModalInsetTop - ModalInsetBottom)
            : size;

        return dim;
    }

    public static Transform GetPanelContent(GameObject modalRoot) =>
        modalRoot.transform.GetChild(modalRoot.transform.childCount - 1);

    public static void BringModalToFront(GameObject modalRoot)
    {
        if (modalRoot == null) return;
        modalRoot.SetActive(true);
        modalRoot.transform.SetAsLastSibling();
    }

    public static void LayoutOverlayMessage(TextMeshProUGUI label, float innerWidth, float innerHeight,
        float topInset = 24f, float horizontalPad = 16f)
    {
        if (label == null) return;

        var rect = label.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -topInset);
        rect.sizeDelta = new Vector2(innerWidth - horizontalPad * 2f, innerHeight - ModalOverlayFooterHeight - topInset - 8f);
        label.enableWordWrapping = true;
        label.overflowMode = TextOverflowModes.Overflow;
    }

    public static Vector2 GetOverlayButtonPosition(float bottomInset = 36f) =>
        new Vector2(0f, bottomInset);

    public static RectTransform CreateStretchBar(Transform parent, string name, float height, Sprite barSprite)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, height);

        var image = go.GetComponent<Image>();
        if (barSprite != null)
            ApplySprite(image, barSprite);
        else
            image.color = new Color(0f, 0f, 0f, 0.35f);
        image.raycastTarget = false;
        return rect;
    }

    public static TextMeshProUGUI CreateCurrencySlot(Transform parent, string name, Sprite icon, bool alignLeft,
        float iconSize = 44f)
    {
        var row = new GameObject(name, typeof(RectTransform));
        row.transform.SetParent(parent, false);
        var rect = row.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(alignLeft ? 0f : 1f, 0f);
        rect.anchorMax = new Vector2(alignLeft ? 0f : 1f, 1f);
        rect.pivot = new Vector2(alignLeft ? 0f : 1f, 0.5f);
        rect.anchoredPosition = new Vector2(alignLeft ? 36f : -36f, 0f);
        rect.sizeDelta = new Vector2(280f, 0f);

        if (icon != null)
        {
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(row.transform, false);
            var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(alignLeft ? 0f : 1f, 0.5f);
            iconRect.anchorMax = new Vector2(alignLeft ? 0f : 1f, 0.5f);
            iconRect.pivot = new Vector2(alignLeft ? 0f : 1f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = new Vector2(iconSize, iconSize);
            ApplySprite(iconGo.GetComponent<Image>(), icon, preserveAspect: true);
        }

        var labelGo = new GameObject("Value", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(row.transform, false);
        var labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(alignLeft ? 0f : 1f, 0f);
        labelRect.anchorMax = new Vector2(alignLeft ? 0f : 1f, 1f);
        labelRect.pivot = new Vector2(alignLeft ? 0f : 1f, 0.5f);
        labelRect.anchoredPosition = new Vector2(alignLeft ? iconSize + 12f : -(iconSize + 12f), 0f);
        labelRect.sizeDelta = new Vector2(200f, 0f);

        var label = labelGo.GetComponent<TextMeshProUGUI>();
        label.text = "0";
        label.fontSize = 34;
        label.alignment = alignLeft ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.MidlineRight;
        label.color = Color.white;
        label.overflowMode = TextOverflowModes.Overflow;
        label.enableWordWrapping = false;
        return label;
    }

    public static TextMeshProUGUI CreateCenterCurrencySlot(Transform parent, string name, Sprite icon)
    {
        var row = new GameObject(name, typeof(RectTransform));
        row.transform.SetParent(parent, false);
        var rect = row.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(260f, 0f);

        if (icon != null)
        {
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(row.transform, false);
            var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = new Vector2(40f, 40f);
            ApplySprite(iconGo.GetComponent<Image>(), icon, preserveAspect: true);
        }

        var labelGo = new GameObject("Value", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(row.transform, false);
        var labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0f);
        labelRect.anchorMax = new Vector2(0.5f, 1f);
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = new Vector2(24f, 0f);
        labelRect.sizeDelta = new Vector2(180f, 0f);

        var label = labelGo.GetComponent<TextMeshProUGUI>();
        label.text = "0";
        label.fontSize = 32;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        return label;
    }

    public static RectTransform CreateListRow(Transform parent, string name, Vector2 size, float y, Color? fill = null)
    {
        var row = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        row.transform.SetParent(parent, false);
        var rect = row.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = size;
        row.GetComponent<Image>().color = fill ?? Popup.Row;
        return rect;
    }

    public static RectTransform CreateScrollListRow(Transform parent, string name, Vector2 size, int index,
        float rowHeight, float rowGap, Color? fill = null)
    {
        var row = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        row.transform.SetParent(parent, false);
        var rect = row.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -(index * (rowHeight + rowGap) + rowHeight * 0.5f));
        rect.sizeDelta = size;
        row.GetComponent<Image>().color = fill ?? Popup.Row;
        return rect;
    }

    public static RectTransform CreateScrollListRowStretch(Transform parent, string name, float rowHeight, int index,
        float rowGap, float horizontalPad = 0f, Color? fill = null)
    {
        var row = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        row.transform.SetParent(parent, false);
        var rect = row.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -index * (rowHeight + rowGap));
        rect.sizeDelta = new Vector2(-horizontalPad * 2f, rowHeight);
        row.GetComponent<Image>().color = fill ?? Popup.Row;
        return rect;
    }

    public static RectTransform CreateModalListScroll(Transform parent, string name, float topInset, float bottomInset,
        float contentHeight)
    {
        var scrollGo = new GameObject(name, typeof(RectTransform), typeof(ScrollRect));
        scrollGo.transform.SetParent(parent, false);
        var scrollRectTransform = scrollGo.GetComponent<RectTransform>();
        StretchWithInsets(scrollRectTransform, topInset, bottomInset);

        var scroll = scrollGo.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 24f;

        var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
            typeof(Mask));
        viewportGo.transform.SetParent(scrollGo.transform, false);
        var viewportRect = viewportGo.GetComponent<RectTransform>();
        Stretch(viewportRect);
        var viewportImage = viewportGo.GetComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
        viewportGo.GetComponent<Mask>().showMaskGraphic = false;

        var contentGo = new GameObject("Content", typeof(RectTransform));
        contentGo.transform.SetParent(viewportGo.transform, false);
        var contentRect = contentGo.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, contentHeight);

        scroll.viewport = viewportRect;
        scroll.content = contentRect;

        return contentRect;
    }

    static void StretchWithInsets(RectTransform rect, float top, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(0f, bottom);
        rect.offsetMax = new Vector2(0f, -top);
    }

    public static RectTransform CreateHighlightedListRow(Transform parent, string name, Vector2 size, float y)
    {
        var row = CreateListRow(parent, name, size, y, Popup.RowHighlight);

        var stripe = new GameObject("Accent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        stripe.transform.SetParent(row, false);
        var stripeRect = stripe.GetComponent<RectTransform>();
        stripeRect.anchorMin = new Vector2(0f, 0f);
        stripeRect.anchorMax = new Vector2(0f, 1f);
        stripeRect.pivot = new Vector2(0f, 0.5f);
        stripeRect.anchoredPosition = Vector2.zero;
        stripeRect.sizeDelta = new Vector2(8f, -12f);
        stripe.GetComponent<Image>().color = Popup.RowAccent;

        return row;
    }

    public static Image CreateRowIcon(Transform row, Sprite icon, float x, float size = 48f)
    {
        if (icon == null) return null;

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconGo.transform.SetParent(row, false);
        var iconRect = iconGo.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(x, 0f);
        iconRect.sizeDelta = new Vector2(size, size);
        var image = iconGo.GetComponent<Image>();
        ApplySprite(image, icon, preserveAspect: true);
        return image;
    }

    public static TextMeshProUGUI CreateLabel(Transform parent, string name, string text, Vector2 anchor,
        Vector2 anchoredPos, Vector2 size, int fontSize, TextAlignmentOptions align, FontStyles style = FontStyles.Normal,
        Color? color = null)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = GetTextPivot(align);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        var label = go.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.alignment = align;
        label.color = color ?? Color.white;
        label.overflowMode = TextOverflowModes.Overflow;
        label.enableWordWrapping = false;
        return label;
    }

    public static TextMeshProUGUI CreatePopupTitle(Transform parent, string name, string text) =>
        CreateLabel(parent, name, text, new Vector2(0.5f, 1f), new Vector2(0f, -36f),
            new Vector2(560f, 52f), 38, TextAlignmentOptions.Center, FontStyles.Bold, Popup.Title);

    public static TextMeshProUGUI CreatePopupSubtitle(Transform parent, string name, string text) =>
        CreateLabel(parent, name, text, new Vector2(0.5f, 1f), new Vector2(0f, -82f),
            new Vector2(620f, 36f), 24, TextAlignmentOptions.Center, FontStyles.Normal, Popup.Subtitle);

    static Vector2 GetAnchorPivot(Vector2 anchor)
    {
        float pivotX = Mathf.Approximately(anchor.x, 0f) ? 0f : Mathf.Approximately(anchor.x, 1f) ? 1f : 0.5f;
        float pivotY = Mathf.Approximately(anchor.y, 0f) ? 0f : Mathf.Approximately(anchor.y, 1f) ? 1f : 0.5f;
        return new Vector2(pivotX, pivotY);
    }

    public static float GetRowActionButtonWidth(float height, UIButtonStyle style, float maxWidth = 148f)
    {
        return Mathf.Min(GetMaxButtonWidth(height, style), maxWidth);
    }

    static Vector2 GetTextPivot(TextAlignmentOptions align)
    {
        if (align == TextAlignmentOptions.MidlineLeft || align == TextAlignmentOptions.TopLeft ||
            align == TextAlignmentOptions.BottomLeft || align == TextAlignmentOptions.Left)
            return new Vector2(0f, 0.5f);

        if (align == TextAlignmentOptions.MidlineRight || align == TextAlignmentOptions.TopRight ||
            align == TextAlignmentOptions.BottomRight || align == TextAlignmentOptions.Right)
            return new Vector2(1f, 0.5f);

        return new Vector2(0.5f, 0.5f);
    }

    public static Button CreateButton(Transform parent, string name, string label, Vector2 anchor, Vector2 anchoredPos,
        Vector2 size, UIButtonStyle style, UnityAction onClick, int fontSize = 26)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = GetAnchorPivot(anchor);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        var image = go.GetComponent<Image>();
        Sprite sprite = Assets != null ? Assets.GetButton(style) : null;
        if (sprite != null)
            ApplyButtonSprite(image, sprite);
        else
            image.color = FallbackButtonColors[(int)style];

        var button = go.GetComponent<Button>();
        button.onClick.AddListener(onClick);

        CreateLabel(go.transform, "Text", label, new Vector2(0.5f, 0.5f), Vector2.zero, size, fontSize,
            TextAlignmentOptions.Center, FontStyles.Bold);
        return button;
    }

    public static Button CreateModalCloseButton(Transform parent, UnityAction onClick)
    {
        float width = GetMaxButtonWidth(52f, UIButtonStyle.Grey);
        return CreateButton(parent, "CloseButton", "닫기", new Vector2(0.5f, 0f),
            new Vector2(0f, ModalCloseButtonY), new Vector2(width, 52f), UIButtonStyle.Grey, onClick);
    }

    public static Button CreateMenuButton(Transform parent, string name, string label, Vector2 centerPosition,
        float height, float width, UIButtonStyle style)
    {
        Sprite sprite = Assets != null ? Assets.GetButton(style) : null;
        var size = new Vector2(width, height);

        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = centerPosition;
        rect.sizeDelta = size;

        var image = go.GetComponent<Image>();
        if (sprite != null)
            ApplyButtonSprite(image, sprite);
        else
            image.color = FallbackButtonColors[(int)style];

        CreateLabel(go.transform, "Text", label, new Vector2(0.5f, 0.5f), Vector2.zero,
            size, 28, TextAlignmentOptions.Center, FontStyles.Bold);
        return go.GetComponent<Button>();
    }

    public static void StyleActionButton(Image image, UIButtonStyle style)
    {
        if (image == null) return;
        Sprite sprite = Assets != null ? Assets.GetButton(style) : null;
        if (sprite != null)
            ApplyButtonSprite(image, sprite);
        else
            image.color = FallbackButtonColors[(int)style];
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public enum ModalMessageKind
    {
        Confirm,
        Success,
        Failure
    }

    public static void ApplyModalMessageText(TextMeshProUGUI label, string message, ModalMessageKind kind,
        int rewardGold = 0, int rewardGems = 0, int rewardShuffle = 0)
    {
        if (label == null) return;

        label.richText = true;
        label.enableWordWrapping = true;
        label.fontStyle = FontStyles.Bold;
        label.lineSpacing = -4f;
        label.color = Color.white;

        if (string.IsNullOrEmpty(message))
        {
            label.text = string.Empty;
            return;
        }

        string[] lines = message.Split('\n');
        var parts = new System.Collections.Generic.List<string>();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].TrimEnd();
            if (string.IsNullOrEmpty(line)) continue;

            if (kind == ModalMessageKind.Confirm)
            {
                if (i == 0)
                {
                    parts.Add(WrapRichLine(line, Popup.Title, 108f));
                    continue;
                }

                if (line.StartsWith("가격:", System.StringComparison.Ordinal))
                {
                    parts.Add(WrapRichLine(line, Popup.Subtitle));
                    continue;
                }

                if (line.StartsWith("보상:", System.StringComparison.Ordinal))
                {
                    const string prefix = "보상: ";
                    string reward = line.Length > prefix.Length ? line.Substring(prefix.Length) : line;
                    Color rewardColor = GetModalRewardColor(rewardGold, rewardGems, rewardShuffle);
                    parts.Add(
                        $"<color=#{ColorUtility.ToHtmlStringRGB(Popup.Subtitle)}>{prefix}</color>" +
                        $"<color=#{ColorUtility.ToHtmlStringRGB(rewardColor)}>{reward}</color>");
                    continue;
                }

                parts.Add(WrapRichLine(line, Popup.Body));
                continue;
            }

            if (kind == ModalMessageKind.Success)
            {
                if (i == 0)
                {
                    parts.Add(WrapRichLine(line, Popup.ModalSuccessTitle, 112f));
                    continue;
                }

                parts.Add(WrapRichLine(line, GetRewardColorFromLabel(line)));
                continue;
            }

            Color failColor = i == 0 ? Popup.ArtPanelTitle : Popup.ArtPanelBody;
            float failSize = i == 0 ? 108f : 100f;
            parts.Add(WrapRichLine(line, failColor, failSize));
        }

        label.text = string.Join("\n", parts);
    }

    static string WrapRichLine(string line, Color color, float sizePercent = 100f)
    {
        string hex = ColorUtility.ToHtmlStringRGB(color);
        if (sizePercent > 100f)
            return $"<size={sizePercent:0}%><color=#{hex}>{line}</color></size>";
        return $"<color=#{hex}>{line}</color>";
    }

    static Color GetRewardColorFromLabel(string label)
    {
        if (label.IndexOf("Gold", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return Popup.RewardGold;
        if (label.IndexOf("Gem", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return Popup.RewardGem;
        if (label.IndexOf("Shuffle", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return Popup.RewardShuffleOnPanel;
        return Popup.ArtPanelReward;
    }

    static Color GetModalRewardColor(int rewardGold, int rewardGems, int rewardShuffle)
    {
        if (rewardGems > 0) return Popup.RewardGem;
        if (rewardShuffle > 0) return Popup.RewardShuffleOnPanel;
        return Popup.RewardGold;
    }
}
