using System;
using UnityEngine;
using UnityEngine.UI;

namespace SukoyakaBuddy.View
{
    /// <summary>
    /// 「はじめから」の確認表示（requirements.md 3.2節手順5・10.3節 CONFIRM_RESET）。
    /// ネイティブのconfirm()相当は使わず、UI内で完結させる（CLAUDE.md コーディング規約）。
    /// </summary>
    public class ConfirmResetDialog
    {
        private const string Title = "はじめから";
        private const string Message = "これまでの記録をすべて削除して、たまごから始めます。よろしいですか？";
        private const string ConfirmLabel = "はじめから やり直す";
        private const string CancelLabel = "キャンセル";

        private GameObject _root;
        private Action _onConfirm;
        private Action _onCancel;

        public void Build(Transform parent, UiTheme theme)
        {
            var overlay = UiFactory.CreatePanel(parent, "ConfirmResetOverlay", null, new Color(0f, 0f, 0f, 0.45f));
            UiFactory.StretchFull(overlay.rectTransform);
            _root = overlay.gameObject;

            var panel = UiFactory.CreatePanel(overlay.transform, "Panel", theme.RoundedRectSprite, UiPalette.Panel);
            var panelRect = panel.rectTransform;
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(460f, 220f);
            panelRect.anchoredPosition = Vector2.zero;

            var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.spacing = UiPalette.Spacing;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childAlignment = TextAnchor.UpperCenter;

            var titleText = UiFactory.CreateText(panel.transform, "Title", theme, Title,
                UiPalette.FontSizeTitle, UiPalette.TextPrimary, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiFactory.SetLayoutSize(titleText.gameObject, preferredHeight: 30f);

            var messageText = UiFactory.CreateText(panel.transform, "Message", theme, Message,
                UiPalette.FontSizeBody, UiPalette.TextSecondary, TextAnchor.MiddleCenter);
            UiFactory.SetLayoutSize(messageText.gameObject, flexibleHeight: 1f, preferredHeight: 60f);

            var buttonRow = UiFactory.CreateRect(panel.transform, "Buttons");
            var buttonLayout = buttonRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = UiPalette.Spacing;
            buttonLayout.childForceExpandWidth = true;
            buttonLayout.childControlWidth = true;
            buttonLayout.childControlHeight = true;
            UiFactory.SetLayoutSize(buttonRow.gameObject, preferredHeight: UiPalette.ButtonHeight);

            var cancelButton = UiFactory.CreateButton(buttonRow, "CancelButton", theme, CancelLabel,
                UiPalette.PanelAlt, UiPalette.TextPrimary, UiPalette.FontSizeButton, out _);
            UiFactory.SetLayoutSize(cancelButton.gameObject, flexibleWidth: 1f);
            cancelButton.onClick.AddListener(HandleCancel);

            var confirmButton = UiFactory.CreateButton(buttonRow, "ConfirmButton", theme, ConfirmLabel,
                UiPalette.AccentDanger, UiPalette.TextOnAccent, UiPalette.FontSizeButton, out _);
            UiFactory.SetLayoutSize(confirmButton.gameObject, flexibleWidth: 1f);
            confirmButton.onClick.AddListener(HandleConfirm);

            _root.SetActive(false);
        }

        public void Show(Action onConfirm, Action onCancel)
        {
            _onConfirm = onConfirm;
            _onCancel = onCancel;
            _root.SetActive(true);
        }

        public void Hide() => _root.SetActive(false);

        private void HandleConfirm()
        {
            Hide();
            _onConfirm?.Invoke();
        }

        private void HandleCancel()
        {
            Hide();
            _onCancel?.Invoke();
        }
    }
}
