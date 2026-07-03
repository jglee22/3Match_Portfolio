using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>설정 팝업 — BGM/SFX 토글, 데이터 초기화</summary>
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    Canvas rootCanvas;
    GameObject settingsPanel;
    GameObject resetConfirmPanel;
    TextMeshProUGUI bgmToggleLabel;
    TextMeshProUGUI sfxToggleLabel;
    Image bgmToggleImage;
    Image sfxToggleImage;

    const float RowWidth = RuntimeUIBuilder.ModalRowWidth;
    const float RowHeight = 72f;
    const float RowGap = 12f;
    const float ActionButtonHeight = 52f;

    const int SettingsUiVersion = 2;
    static int builtSettingsUiVersion;
    TextMeshProUGUI resetConfirmMessageText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void OpenSettings()
    {
        if (AdRewardManager.BlocksLobbyInput) return;

        EnsureUI();
        RefreshToggleLabels();
        settingsPanel.transform.SetAsLastSibling();
        settingsPanel.SetActive(true);
        resetConfirmPanel.SetActive(false);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        if (resetConfirmPanel != null)
            resetConfirmPanel.SetActive(false);
    }

    void EnsureUI()
    {
        if (settingsPanel != null && builtSettingsUiVersion >= SettingsUiVersion) return;

        if (settingsPanel != null)
        {
            Destroy(settingsPanel);
            settingsPanel = null;
            resetConfirmPanel = null;
            resetConfirmMessageText = null;
        }

        builtSettingsUiVersion = SettingsUiVersion;

        var ui = HyperCasualUIAssets.Instance;
        const int rowCount = 3;
        float headerHeight = RuntimeUIBuilder.ModalHeaderHeight + RuntimeUIBuilder.ModalSubtitleExtra;
        float innerHeight = headerHeight + rowCount * RowHeight
            + (rowCount - 1) * RowGap + RuntimeUIBuilder.ModalFooterHeight;

        rootCanvas = FindAnyObjectByType<Canvas>();
        if (rootCanvas == null)
            return;

        settingsPanel = RuntimeUIBuilder.CreateModal(rootCanvas.transform, "SettingsPanel",
            RuntimeUIBuilder.BuildModalOuterSize(520f, innerHeight), ui?.popupPanel);
        var content = RuntimeUIBuilder.GetPanelContent(settingsPanel);
        RuntimeUIBuilder.CreatePopupTitle(content, "Title", "Settings");
        RuntimeUIBuilder.CreatePopupSubtitle(content, "Subtitle", "게임 설정");

        float firstRowY = RuntimeUIBuilder.GetModalRowY(0, RowHeight, RowGap, innerHeight, headerHeight);
        CreateToggleRow(content, "BgmRow", "BGM", firstRowY, ToggleBgm, out bgmToggleLabel, out bgmToggleImage);
        CreateToggleRow(content, "SfxRow", "SFX", firstRowY - (RowHeight + RowGap), ToggleSfx, out sfxToggleLabel, out sfxToggleImage);

        float resetRowY = firstRowY - 2f * (RowHeight + RowGap);
        var resetRow = RuntimeUIBuilder.CreateListRow(content, "ResetRow", new Vector2(RowWidth, RowHeight), resetRowY);
        float resetButtonWidth = RuntimeUIBuilder.GetRowActionButtonWidth(ActionButtonHeight, UIButtonStyle.Red, 220f);
        RuntimeUIBuilder.CreateButton(resetRow, "ResetButton", "데이터 초기화", new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(resetButtonWidth, ActionButtonHeight), UIButtonStyle.Red, OpenResetConfirm);

        RuntimeUIBuilder.CreateModalCloseButton(content, CloseSettings);

        const float confirmMessageArea = 108f;
        const float confirmWidth = 580f;
        resetConfirmPanel = RuntimeUIBuilder.CreateModal(settingsPanel.transform, "SettingsResetConfirmPanel",
            RuntimeUIBuilder.BuildOverlayModalOuterSize(confirmWidth, confirmMessageArea), ui?.popupPanel);
        var confirmContent = RuntimeUIBuilder.GetPanelContent(resetConfirmPanel);
        float confirmInnerWidth = RuntimeUIBuilder.GetModalInnerWidth(confirmWidth);
        float confirmInnerHeight = confirmMessageArea + RuntimeUIBuilder.ModalOverlayFooterHeight;
        resetConfirmMessageText = RuntimeUIBuilder.CreateLabel(confirmContent, "ConfirmMessage",
            "저장 데이터를 초기화하시겠습니까?", new Vector2(0.5f, 1f), new Vector2(0f, -24f),
            new Vector2(confirmInnerWidth - 32f, 88f), 28, TextAlignmentOptions.Center, FontStyles.Bold,
            RuntimeUIBuilder.Popup.Body);
        RuntimeUIBuilder.LayoutOverlayMessage(resetConfirmMessageText, confirmInnerWidth, confirmInnerHeight);

        var buttonPos = RuntimeUIBuilder.GetOverlayButtonPosition();
        RuntimeUIBuilder.CreateButton(confirmContent, "ConfirmYesButton", "확인", new Vector2(0.3f, 0f), buttonPos,
            new Vector2(160f, 52f), UIButtonStyle.Red, ConfirmReset);
        RuntimeUIBuilder.CreateButton(confirmContent, "ConfirmNoButton", "취소", new Vector2(0.7f, 0f), buttonPos,
            new Vector2(160f, 52f), UIButtonStyle.Grey, CloseResetConfirm);

        settingsPanel.SetActive(false);
        resetConfirmPanel.SetActive(false);
    }

    void CreateToggleRow(Transform parent, string rowName, string label, float y, UnityEngine.Events.UnityAction onToggle,
        out TextMeshProUGUI stateLabel, out Image buttonImage)
    {
        var row = RuntimeUIBuilder.CreateListRow(parent, rowName, new Vector2(RowWidth, RowHeight), y);
        RuntimeUIBuilder.CreateLabel(row, "Label", label, new Vector2(0f, 0.5f), new Vector2(24f, 0f),
            new Vector2(120f, 36f), 28, TextAlignmentOptions.MidlineLeft, FontStyles.Bold, RuntimeUIBuilder.Popup.Body);

        float buttonWidth = RuntimeUIBuilder.GetRowActionButtonWidth(ActionButtonHeight, UIButtonStyle.Green, 132f);
        var button = RuntimeUIBuilder.CreateButton(row, "ToggleButton", "ON", new Vector2(1f, 0.5f), new Vector2(-16f, 0f),
            new Vector2(buttonWidth, ActionButtonHeight), UIButtonStyle.Green, onToggle);
        buttonImage = button.GetComponent<Image>();
        stateLabel = button.GetComponentInChildren<TextMeshProUGUI>();
    }

    void RefreshToggleLabels()
    {
        ApplyToggleVisual(bgmToggleLabel, bgmToggleImage, SoundManager.IsBgmEnabled);
        ApplyToggleVisual(sfxToggleLabel, sfxToggleImage, SoundManager.IsSfxEnabled);
    }

    static void ApplyToggleVisual(TextMeshProUGUI label, Image image, bool enabled)
    {
        if (label != null)
            label.text = enabled ? "ON" : "OFF";
        if (image != null)
            RuntimeUIBuilder.StyleActionButton(image, enabled ? UIButtonStyle.Green : UIButtonStyle.Grey);
    }

    void ToggleBgm()
    {
        SoundManager.SetBgmEnabled(!SoundManager.IsBgmEnabled);
        ApplyToggleVisual(bgmToggleLabel, bgmToggleImage, SoundManager.IsBgmEnabled);
    }

    void ToggleSfx()
    {
        SoundManager.SetSfxEnabled(!SoundManager.IsSfxEnabled);
        ApplyToggleVisual(sfxToggleLabel, sfxToggleImage, SoundManager.IsSfxEnabled);
    }

    void OpenResetConfirm()
    {
        RuntimeUIBuilder.ApplyModalMessageText(resetConfirmMessageText, "저장 데이터를 초기화하시겠습니까?",
            RuntimeUIBuilder.ModalMessageKind.Confirm);
        RuntimeUIBuilder.BringModalToFront(resetConfirmPanel);
    }

    void CloseResetConfirm()
    {
        resetConfirmPanel.SetActive(false);
    }

    void ConfirmReset()
    {
        GameDataManager.Instance?.ResetToDefaults();
        CurrencyManager.Instance?.RefreshUI();
        CloseResetConfirm();
        CloseSettings();
    }
}
