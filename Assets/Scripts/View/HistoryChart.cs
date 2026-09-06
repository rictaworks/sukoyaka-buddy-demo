using System.Collections.Generic;
using SukoyakaBuddy.Model;
using UnityEngine;
using UnityEngine.UI;

namespace SukoyakaBuddy.View
{
    /// <summary>
    /// 直近7仮想日の運動・睡眠・食事スコアを棒グラフで表示する（requirements.md F10・3.1節）。
    /// 色だけで項目を区別しないよう、凡例に文言を添える。各日の列は手動配置とし、
    /// LayoutGroupの自動制御とRectTransformの直接操作を混在させない（レイアウト崩れの防止）。
    /// </summary>
    public class HistoryChart
    {
        private const string Title = "直近のふりかえり";
        private const string EmptyMessage = "記録はまだありません";
        private const float ChartAreaHeight = 150f;
        private const float BarAreaHeight = 110f;
        private const float BarWidth = 14f;
        private const float BarGap = 4f;
        private const float ColumnWidth = BarWidth * 3 + BarGap * 2 + 12f;
        private const string DayLabelFormat = "{0}日目";

        private RectTransform _columnsArea;
        private Text _emptyText;
        private readonly List<GameObject> _dayColumns = new List<GameObject>();
        private UiTheme _theme;

        public void Build(Transform parent, UiTheme theme)
        {
            _theme = theme;
            var panel = UiFactory.CreatePanel(parent, "HistoryChart", theme.RoundedRectSprite, UiPalette.Panel);
            UiFactory.SetLayoutSize(panel.gameObject, flexibleHeight: 1f);
            var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset((int)UiPalette.PanelPadding, (int)UiPalette.PanelPadding,
                (int)UiPalette.PanelPadding, (int)UiPalette.PanelPadding);
            layout.spacing = UiPalette.SpacingSmall;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            var titleText = UiFactory.CreateText(panel.transform, "Title", theme, Title,
                UiPalette.FontSizeHeading, UiPalette.TextPrimary, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiFactory.SetLayoutSize(titleText.gameObject, preferredHeight: 26f);

            BuildLegend(panel.transform, theme);

            var chartArea = UiFactory.CreateRect(panel.transform, "ChartArea");
            UiFactory.SetLayoutSize(chartArea.gameObject, preferredHeight: ChartAreaHeight, flexibleHeight: 1f);

            _emptyText = UiFactory.CreateText(chartArea, "Empty", theme, EmptyMessage,
                UiPalette.FontSizeBody, UiPalette.TextSecondary, TextAnchor.MiddleCenter);
            UiFactory.StretchFull(_emptyText.rectTransform);

            _columnsArea = UiFactory.CreateRect(chartArea, "Columns");
            UiFactory.StretchFull(_columnsArea);
            var columnsLayout = _columnsArea.gameObject.AddComponent<HorizontalLayoutGroup>();
            columnsLayout.spacing = UiPalette.SpacingXSmall;
            columnsLayout.childAlignment = TextAnchor.LowerCenter;
            columnsLayout.childForceExpandWidth = false;
            columnsLayout.childForceExpandHeight = false;
            columnsLayout.childControlWidth = true;
            columnsLayout.childControlHeight = true;
        }

        public void Render(List<DayRecord> history)
        {
            foreach (var go in _dayColumns) Object.Destroy(go);
            _dayColumns.Clear();

            bool hasHistory = history != null && history.Count > 0;
            _emptyText.gameObject.SetActive(!hasHistory);
            _columnsArea.gameObject.SetActive(hasHistory);
            if (!hasHistory) return;

            foreach (var record in history)
            {
                _dayColumns.Add(BuildDayColumn(record));
            }
        }

        /// <summary>列自体はHorizontalLayoutGroupが幅・高さを制御するが、内部の棒・文字は列の矩形に対して
        /// 手動でアンカー配置する（自動レイアウトと手動配置を1つのRectTransriptionで混在させない）。</summary>
        private GameObject BuildDayColumn(DayRecord record)
        {
            var column = UiFactory.CreateRect(_columnsArea, $"Day_{record.DayNo}");
            UiFactory.SetLayoutSize(column.gameObject, preferredWidth: ColumnWidth, preferredHeight: ChartAreaHeight);

            float groupWidth = BarWidth * 3 + BarGap * 2;
            float startX = -groupWidth / 2f + BarWidth / 2f;
            float step = BarWidth + BarGap;

            BuildBar(column, record.Exercise, UiPalette.ExerciseColor, startX);
            BuildBar(column, record.Sleep, UiPalette.SleepColor, startX + step);
            BuildBar(column, record.Meal, UiPalette.MealColor, startX + step * 2);

            var dayLabel = UiFactory.CreateText(column, "DayLabel", _theme, string.Format(DayLabelFormat, record.DayNo),
                UiPalette.FontSizeSmall, UiPalette.TextSecondary, TextAnchor.MiddleCenter);
            dayLabel.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            dayLabel.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            dayLabel.rectTransform.pivot = new Vector2(0.5f, 0f);
            dayLabel.rectTransform.sizeDelta = new Vector2(ColumnWidth, 16f);
            dayLabel.rectTransform.anchoredPosition = Vector2.zero;

            return column.gameObject;
        }

        private void BuildBar(Transform parent, int score, Color color, float x)
        {
            float clamped = Mathf.Clamp(score, 0, 100);
            float barHeight = Mathf.Max(2f, BarAreaHeight * (clamped / 100f));
            const float barBottomOffset = 18f; // 日付ラベル分の余白

            var bar = UiFactory.CreateShape(parent, "Bar", _theme.RoundedRectSprite, color, new Vector2(BarWidth, barHeight));
            bar.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            bar.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            bar.rectTransform.pivot = new Vector2(0.5f, 0f);
            bar.rectTransform.anchoredPosition = new Vector2(x, barBottomOffset);

            var valueText = UiFactory.CreateText(parent, "Value", _theme, clamped.ToString("0"),
                UiPalette.FontSizeSmall, UiPalette.TextSecondary, TextAnchor.LowerCenter);
            valueText.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            valueText.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            valueText.rectTransform.pivot = new Vector2(0.5f, 0f);
            valueText.rectTransform.sizeDelta = new Vector2(BarWidth * 2.4f, 14f);
            valueText.rectTransform.anchoredPosition = new Vector2(x, barBottomOffset + barHeight + 2f);
        }

        private void BuildLegend(Transform parent, UiTheme theme)
        {
            var legendRow = UiFactory.CreateRect(parent, "Legend");
            var layout = legendRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = UiPalette.Spacing;
            layout.childForceExpandWidth = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            UiFactory.SetLayoutSize(legendRow.gameObject, preferredHeight: 20f);

            AddLegendItem(legendRow, theme, UiPalette.ExerciseColor, DisplayNames.ScoreCategoryLabel(ScoreCategory.Exercise));
            AddLegendItem(legendRow, theme, UiPalette.SleepColor, DisplayNames.ScoreCategoryLabel(ScoreCategory.Sleep));
            AddLegendItem(legendRow, theme, UiPalette.MealColor, DisplayNames.ScoreCategoryLabel(ScoreCategory.Meal));
        }

        private void AddLegendItem(Transform parent, UiTheme theme, Color color, string label)
        {
            var item = UiFactory.CreateRect(parent, $"Legend_{label}");
            var layout = item.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = UiPalette.SpacingXSmall;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            UiFactory.SetLayoutSize(item.gameObject, preferredWidth: 70f);

            var swatch = UiFactory.CreateShape(item, "Swatch", theme.RoundedRectSprite, color, new Vector2(12f, 12f));
            UiFactory.SetLayoutSize(swatch.gameObject, preferredWidth: 12f, preferredHeight: 12f);

            var text = UiFactory.CreateText(item, "Label", theme, label,
                UiPalette.FontSizeSmall, UiPalette.TextSecondary, TextAnchor.MiddleLeft);
            UiFactory.SetLayoutSize(text.gameObject, preferredWidth: 50f, preferredHeight: 16f);
        }
    }
}
