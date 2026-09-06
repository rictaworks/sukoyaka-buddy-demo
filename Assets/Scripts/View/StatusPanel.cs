using SukoyakaBuddy.Model;
using UnityEngine;
using UnityEngine.UI;

namespace SukoyakaBuddy.View
{
    /// <summary>
    /// 段階・成長タイプ・仮想日番号・経験値・4ゲージ・連続日数を表示する（requirements.md 3.1節）。
    /// 一般コンテンツのためですます調で統一する（CLAUDE.md 口調方針）。
    /// </summary>
    public class StatusPanel
    {
        private const string LabelStreakFormat = "連続{0}日";
        private const string LabelDayFormat = "仮想日 {0}日目";
        private const string LabelExpWithRemainFormat = "経験値 {0}（次の段階まであと{1}）";
        private const string LabelExpAtMaxFormat = "経験値 {0}（最終段階です）";
        private const string LabelGrowthTypeUndecided = "成長タイプ：未確定";
        private const string LabelGrowthTypeFormat = "成長タイプ：{0}";

        private Text _stageText;
        private Text _growthTypeText;
        private Text _dayText;
        private Text _expText;
        private Text _streakText;
        private Gauge _powerGauge;
        private Gauge _energyGauge;
        private Gauge _bodyGauge;
        private Gauge _fatigueGauge;

        public void Build(Transform parent, UiTheme theme)
        {
            var panel = UiFactory.CreatePanel(parent, "StatusPanel", theme.RoundedRectSprite, UiPalette.Panel);
            var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset((int)UiPalette.PanelPadding, (int)UiPalette.PanelPadding,
                (int)UiPalette.PanelPadding, (int)UiPalette.PanelPadding);
            layout.spacing = UiPalette.SpacingSmall;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            var headerRow = UiFactory.CreateRect(panel.transform, "HeaderRow");
            var headerLayout = headerRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            headerLayout.spacing = UiPalette.Spacing;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childControlWidth = true;
            UiFactory.SetLayoutSize(headerRow.gameObject, preferredHeight: 30f);

            _stageText = UiFactory.CreateText(headerRow, "StageText", theme, string.Empty,
                UiPalette.FontSizeTitle, UiPalette.TextPrimary, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiFactory.SetLayoutSize(_stageText.gameObject, flexibleWidth: 1f);

            _dayText = UiFactory.CreateText(headerRow, "DayText", theme, string.Empty,
                UiPalette.FontSizeHeading, UiPalette.TextSecondary, TextAnchor.MiddleRight);
            UiFactory.SetLayoutSize(_dayText.gameObject, preferredWidth: 160f);

            _growthTypeText = UiFactory.CreateText(panel.transform, "GrowthTypeText", theme, LabelGrowthTypeUndecided,
                UiPalette.FontSizeBody, UiPalette.TextSecondary, TextAnchor.MiddleLeft);
            UiFactory.SetLayoutSize(_growthTypeText.gameObject, preferredHeight: 24f);

            _expText = UiFactory.CreateText(panel.transform, "ExpText", theme, string.Empty,
                UiPalette.FontSizeBody, UiPalette.TextSecondary, TextAnchor.MiddleLeft);
            UiFactory.SetLayoutSize(_expText.gameObject, preferredHeight: 24f);

            var gaugeArea = UiFactory.CreateRect(panel.transform, "GaugeArea");
            var gaugeLayout = gaugeArea.gameObject.AddComponent<HorizontalLayoutGroup>();
            gaugeLayout.spacing = UiPalette.Spacing;
            gaugeLayout.childForceExpandWidth = true;
            gaugeLayout.childControlWidth = true;
            gaugeLayout.childControlHeight = true;
            UiFactory.SetLayoutSize(gaugeArea.gameObject, preferredHeight: 54f);

            _powerGauge = new Gauge();
            _powerGauge.Build(gaugeArea, theme, "ちから", UiPalette.PowerColor);
            _energyGauge = new Gauge();
            _energyGauge.Build(gaugeArea, theme, "げんき", UiPalette.EnergyColor);
            _bodyGauge = new Gauge();
            _bodyGauge.Build(gaugeArea, theme, "からだ", UiPalette.BodyColor);
            _fatigueGauge = new Gauge();
            _fatigueGauge.Build(gaugeArea, theme, "つかれ", UiPalette.FatigueColor);

            _streakText = UiFactory.CreateText(panel.transform, "StreakText", theme, string.Empty,
                UiPalette.FontSizeLabel, UiPalette.TextSecondary, TextAnchor.MiddleLeft);
            UiFactory.SetLayoutSize(_streakText.gameObject, preferredHeight: 22f);
        }

        public void Render(Character character)
        {
            var stage = character.GetStage();
            _stageText.text = DisplayNames.StageLabel(stage);
            _dayText.text = string.Format(LabelDayFormat, character.DayNo);

            _growthTypeText.text = character.GrowthType == GrowthType.Undecided
                ? LabelGrowthTypeUndecided
                : string.Format(LabelGrowthTypeFormat, DisplayNames.GrowthTypeLabel(character.GrowthType));

            _expText.text = stage == Stage.Master
                ? string.Format(LabelExpAtMaxFormat, character.Exp)
                : string.Format(LabelExpWithRemainFormat, character.Exp, character.ExpToNextStage());

            _powerGauge.SetValue(character.Power);
            _energyGauge.SetValue(character.Energy);
            _bodyGauge.SetValue(character.Body);
            _fatigueGauge.SetValue(character.Fatigue);

            _streakText.text = string.Format(LabelStreakFormat, character.Streak);
        }

        /// <summary>ラベル・トラック・塗り・数値を1組にしたゲージ。StatusPanel専用の内部部品。
        /// 塗り部分はanchorMax.xで比率を表すため、トラックの実ピクセル幅を問わずレイアウト確定と同時に正しく描画される。</summary>
        private sealed class Gauge
        {
            private RectTransform _fillRect;
            private Text _valueText;

            public void Build(Transform parent, UiTheme theme, string label, Color color)
            {
                var column = UiFactory.CreateRect(parent, $"Gauge_{label}");
                var layout = column.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.spacing = UiPalette.SpacingXSmall;
                layout.childForceExpandWidth = true;
                layout.childControlWidth = true;
                layout.childControlHeight = true;

                var labelText = UiFactory.CreateText(column, "Label", theme, label,
                    UiPalette.FontSizeSmall, UiPalette.TextSecondary, TextAnchor.MiddleLeft);
                UiFactory.SetLayoutSize(labelText.gameObject, preferredHeight: 18f);

                var track = UiFactory.CreatePanel(column, "Track", theme.RoundedRectSprite, UiPalette.TrackColor);
                UiFactory.SetLayoutSize(track.gameObject, preferredHeight: UiPalette.GaugeBarHeight);

                var fill = UiFactory.CreatePanel(track.transform, "Fill", theme.RoundedRectSprite, color);
                _fillRect = fill.rectTransform;
                _fillRect.anchorMin = new Vector2(0f, 0f);
                _fillRect.anchorMax = new Vector2(0f, 1f);
                _fillRect.offsetMin = Vector2.zero;
                _fillRect.offsetMax = Vector2.zero;

                _valueText = UiFactory.CreateText(column, "Value", theme, "0",
                    UiPalette.FontSizeSmall, UiPalette.TextPrimary, TextAnchor.MiddleRight);
                UiFactory.SetLayoutSize(_valueText.gameObject, preferredHeight: 16f);
            }

            public void SetValue(int value)
            {
                int clamped = Mathf.Clamp(value, 0, 100);
                _valueText.text = clamped.ToString();
                _fillRect.anchorMax = new Vector2(clamped / 100f, 1f);
            }
        }
    }
}
