using UnityEngine;
using UnityEngine.UI;

namespace SukoyakaBuddy.View
{
    /// <summary>
    /// uGUI要素をコードから組み立てる共通ヘルパー（requirements.md 12.6節：シーン・UIはすべてコード生成、Editor GUI操作は使わない）。
    /// 生成した要素の参照は呼び出し側がフィールドに保持し、名前検索で取り直さないこと（不変条件6）。
    /// </summary>
    public static class UiFactory
    {
        public static RectTransform CreateRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        public static Image CreatePanel(Transform parent, string name, Sprite sprite, Color color)
        {
            var rect = CreateRect(parent, name);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = color;
            return image;
        }

        public static Image CreateShape(Transform parent, string name, Sprite sprite, Color color, Vector2 size)
        {
            var image = CreatePanel(parent, name, sprite, color);
            image.type = Image.Type.Simple;
            image.rectTransform.sizeDelta = size;
            return image;
        }

        public static Text CreateText(
            Transform parent,
            string name,
            UiTheme theme,
            string content,
            int fontSize,
            Color color,
            TextAnchor anchor,
            FontStyle style = FontStyle.Normal)
        {
            var rect = CreateRect(parent, name);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = theme.JapaneseFont;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = anchor;
            text.text = content;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        /// <summary>状態遷移（ホバー・押下・無効化）はUnity標準のColor Tintに任せる通常ボタン用。</summary>
        public static Button CreateButton(
            Transform parent,
            string name,
            UiTheme theme,
            string label,
            Color background,
            Color textColor,
            int fontSize,
            out Text labelText)
        {
            var image = CreatePanel(parent, name, theme.RoundedRectSprite, background);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.disabledColor = UiPalette.AccentDisabled;
            button.colors = colors;

            labelText = CreateText(image.transform, "Label", theme, label, fontSize, textColor, TextAnchor.MiddleCenter, FontStyle.Bold);
            StretchFull(labelText.rectTransform);
            return button;
        }

        /// <summary>選択状態を持つ「選択肢チップ」用。見た目はApplyVisual側が手動で制御するためTransitionはNoneにする。</summary>
        public static Button CreateChipButton(
            Transform parent,
            string name,
            UiTheme theme,
            string label,
            int fontSize,
            out Text labelText,
            out Image background)
        {
            background = CreatePanel(parent, name, theme.RoundedRectSprite, UiPalette.UnselectedColor);
            var button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;

            labelText = CreateText(background.transform, "Label", theme, label, fontSize, UiPalette.TextPrimary, TextAnchor.MiddleCenter);
            StretchFull(labelText.rectTransform);
            return button;
        }

        public static Toggle CreateToggle(Transform parent, string name, UiTheme theme, out Image checkImage)
        {
            var container = CreateRect(parent, name);
            var background = CreatePanel(container, "Background", theme.RoundedRectSprite, UiPalette.PanelAlt);
            background.rectTransform.sizeDelta = new Vector2(UiPalette.ToggleSize, UiPalette.ToggleSize);

            var toggle = container.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = background;
            toggle.transition = Selectable.Transition.None;

            checkImage = CreatePanel(background.transform, "Checkmark", theme.CircleSprite, UiPalette.AccentPrimary);
            checkImage.rectTransform.sizeDelta = new Vector2(UiPalette.ToggleSize * 0.6f, UiPalette.ToggleSize * 0.6f);
            checkImage.rectTransform.anchorMin = checkImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            checkImage.rectTransform.anchoredPosition = Vector2.zero;
            toggle.graphic = checkImage;

            var layout = container.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = UiPalette.ToggleSize;
            layout.preferredHeight = Mathf.Max(UiPalette.ToggleSize, UiPalette.MinTapSize);
            return toggle;
        }

        public static Slider CreateSlider(Transform parent, string name, UiTheme theme, float min, float max, float value)
        {
            var container = CreateRect(parent, name);
            var containerLayout = container.gameObject.AddComponent<LayoutElement>();
            containerLayout.preferredHeight = UiPalette.SliderHeight;
            containerLayout.flexibleWidth = 1f;

            var slider = container.gameObject.AddComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = false;
            slider.transition = Selectable.Transition.None;

            var track = CreatePanel(container, "Track", theme.RoundedRectSprite, UiPalette.TrackColor);
            track.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            track.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            track.rectTransform.sizeDelta = new Vector2(0f, UiPalette.GaugeBarHeight);
            track.rectTransform.anchoredPosition = Vector2.zero;

            var fillArea = CreateRect(container, "FillArea");
            fillArea.anchorMin = new Vector2(0f, 0.5f);
            fillArea.anchorMax = new Vector2(1f, 0.5f);
            fillArea.sizeDelta = new Vector2(0f, UiPalette.GaugeBarHeight);
            fillArea.anchoredPosition = Vector2.zero;

            var fill = CreatePanel(fillArea, "Fill", theme.RoundedRectSprite, UiPalette.AccentPrimary);
            fill.rectTransform.anchorMin = new Vector2(0f, 0f);
            fill.rectTransform.anchorMax = new Vector2(0f, 1f);
            fill.rectTransform.sizeDelta = new Vector2(0f, 0f);
            slider.fillRect = fill.rectTransform;

            var handleArea = CreateRect(container, "HandleArea");
            StretchFull(handleArea);
            var handle = CreatePanel(handleArea, "Handle", theme.CircleSprite, UiPalette.Panel);
            handle.rectTransform.sizeDelta = new Vector2(UiPalette.HandleSize, UiPalette.HandleSize);
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;

            slider.SetValueWithoutNotify(value);
            return slider;
        }

        public static ScrollRect CreateScrollView(Transform parent, string name, UiTheme theme, out RectTransform content)
        {
            var container = CreatePanel(parent, name, theme.RoundedRectSprite, UiPalette.Panel);
            var scrollRect = container.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 24f;

            var viewport = CreateRect(container.transform, "Viewport");
            StretchFull(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();

            content = CreateRect(viewport, "Content");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;

            var layoutGroup = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.spacing = UiPalette.Spacing;
            layoutGroup.padding = new RectOffset(
                (int)UiPalette.PanelPadding, (int)UiPalette.PanelPadding,
                (int)UiPalette.PanelPadding, (int)UiPalette.PanelPadding);

            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport;
            scrollRect.content = content;
            return scrollRect;
        }

        public static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        public static LayoutElement SetLayoutSize(
            GameObject go,
            float? preferredWidth = null,
            float? preferredHeight = null,
            float? minWidth = null,
            float? minHeight = null,
            float? flexibleWidth = null,
            float? flexibleHeight = null)
        {
            var element = go.GetComponent<LayoutElement>();
            if (element == null) element = go.AddComponent<LayoutElement>();
            if (preferredWidth.HasValue) element.preferredWidth = preferredWidth.Value;
            if (preferredHeight.HasValue) element.preferredHeight = preferredHeight.Value;
            if (minWidth.HasValue) element.minWidth = minWidth.Value;
            if (minHeight.HasValue) element.minHeight = minHeight.Value;
            if (flexibleWidth.HasValue) element.flexibleWidth = flexibleWidth.Value;
            if (flexibleHeight.HasValue) element.flexibleHeight = flexibleHeight.Value;
            return element;
        }
    }
}
