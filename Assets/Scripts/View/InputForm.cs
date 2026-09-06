using System;
using System.Collections.Generic;
using SukoyakaBuddy.Model;
using UnityEngine;
using UnityEngine.UI;

namespace SukoyakaBuddy.View
{
    /// <summary>
    /// 記録フォーム一式（requirements.md 3.1節）。UI側でも範囲・刻みを制限するが、
    /// 最終的な正規化はInputNormalizerが行う（UIの制限を信頼しない・12.2節）。
    /// </summary>
    public class InputForm
    {
        private const string TitleText = "記録フォーム";
        private const string ExerciseLabel = "運動時間";
        private const string ExerciseUnitFormat = "{0:0}分";
        private const string IntensityLabel = "運動の強さ";
        private const string SleepLabel = "睡眠時間";
        private const string SleepUnitFormat = "{0:0.0}時間";
        private const string BreakfastLabel = "朝ごはん";
        private const string LunchLabel = "昼ごはん";
        private const string DinnerLabel = "夕ごはん";
        private const string SnackLabel = "おやつの回数";
        private const string VegetableLabel = "野菜あり";
        private const string SnackUnitFormat = "{0}回";
        private const string SelectedMarker = "✓ ";

        private static readonly (Intensity value, string label)[] IntensityChoices =
        {
            (Intensity.Light, DisplayNames.IntensityLabel(Intensity.Light)),
            (Intensity.Normal, DisplayNames.IntensityLabel(Intensity.Normal)),
            (Intensity.Intense, DisplayNames.IntensityLabel(Intensity.Intense)),
        };

        private static readonly (MealAmount value, string label)[] MealAmountChoices =
        {
            (MealAmount.Skip, DisplayNames.MealAmountLabel(MealAmount.Skip)),
            (MealAmount.Light, DisplayNames.MealAmountLabel(MealAmount.Light)),
            (MealAmount.Full, DisplayNames.MealAmountLabel(MealAmount.Full)),
            (MealAmount.Overeat, DisplayNames.MealAmountLabel(MealAmount.Overeat)),
        };

        private static readonly (int value, string label)[] SnackChoices =
        {
            (0, string.Format(SnackUnitFormat, 0)),
            (1, string.Format(SnackUnitFormat, 1)),
            (2, string.Format(SnackUnitFormat, 2)),
            (3, string.Format(SnackUnitFormat, 3)),
        };

        private readonly SliderField _exerciseField = new SliderField();
        private readonly OptionGroup<Intensity> _intensityGroup = new OptionGroup<Intensity>();
        private readonly SliderField _sleepField = new SliderField();
        private readonly MealRow _breakfastRow = new MealRow();
        private readonly MealRow _lunchRow = new MealRow();
        private readonly MealRow _dinnerRow = new MealRow();
        private readonly OptionGroup<int> _snackGroup = new OptionGroup<int>();

        public void Build(Transform parent, UiTheme theme)
        {
            var scrollRect = UiFactory.CreateScrollView(parent, "InputFormScroll", theme, out var content);
            UiFactory.SetLayoutSize(scrollRect.gameObject, flexibleHeight: 1f);

            var titleText = UiFactory.CreateText(content, "Title", theme, TitleText,
                UiPalette.FontSizeHeading, UiPalette.TextPrimary, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiFactory.SetLayoutSize(titleText.gameObject, preferredHeight: 26f);

            _exerciseField.Build(content, theme, ExerciseLabel,
                GameConstants.ExerciseMinMinutes, GameConstants.ExerciseMaxMinutes, GameConstants.ExerciseStepMinutes,
                ExerciseUnitFormat, 0f);
            _intensityGroup.Build(content, theme, IntensityLabel, IntensityChoices, Intensity.Normal);
            _sleepField.Build(content, theme, SleepLabel,
                GameConstants.SleepMinHours, GameConstants.SleepMaxHours, GameConstants.SleepStepHours,
                SleepUnitFormat, GameConstants.DefaultSleepHours);

            _breakfastRow.Build(content, theme, BreakfastLabel);
            _lunchRow.Build(content, theme, LunchLabel);
            _dinnerRow.Build(content, theme, DinnerLabel);

            _snackGroup.Build(content, theme, SnackLabel, SnackChoices, 0);
        }

        public DailyInput Read()
        {
            return new DailyInput
            {
                ExerciseMinutes = Mathf.RoundToInt(_exerciseField.Value),
                Intensity = _intensityGroup.Selected,
                SleepHours = _sleepField.Value,
                Breakfast = _breakfastRow.Read(),
                Lunch = _lunchRow.Read(),
                Dinner = _dinnerRow.Read(),
                Snacks = _snackGroup.Selected,
                DayNo = 0
            };
        }

        public void Write(DailyInput input)
        {
            _exerciseField.SetValue(input.ExerciseMinutes);
            _intensityGroup.Select(input.Intensity, notify: false);
            _sleepField.SetValue(input.SleepHours);
            _breakfastRow.Write(input.Breakfast);
            _lunchRow.Write(input.Lunch);
            _dinnerRow.Write(input.Dinner);
            _snackGroup.Select(input.Snacks, notify: false);
        }

        public void ResetToDefault() => Write(DailyInput.Default(0));

        public void SetEnabled(bool enabled)
        {
            _exerciseField.SetInteractable(enabled);
            _intensityGroup.SetInteractable(enabled);
            _sleepField.SetInteractable(enabled);
            _breakfastRow.SetInteractable(enabled);
            _lunchRow.SetInteractable(enabled);
            _dinnerRow.SetInteractable(enabled);
            _snackGroup.SetInteractable(enabled);
        }

        /// <summary>ラベル・数値表示付きのスライダー1本（運動時間・睡眠時間で共用）。</summary>
        private sealed class SliderField
        {
            private Slider _slider;
            private Text _valueText;
            private float _step;
            private string _unitFormat;

            public float Value => _slider.value;

            public void Build(Transform parent, UiTheme theme, string title, float min, float max, float step, string unitFormat, float initial)
            {
                _step = step;
                _unitFormat = unitFormat;

                var row = UiFactory.CreateRect(parent, $"Field_{title}");
                var layout = row.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.spacing = UiPalette.SpacingXSmall;
                layout.childForceExpandWidth = true;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                UiFactory.SetLayoutSize(row.gameObject, preferredHeight: 66f);

                var headerRow = UiFactory.CreateRect(row, "Header");
                var headerLayout = headerRow.gameObject.AddComponent<HorizontalLayoutGroup>();
                headerLayout.childControlWidth = true;
                headerLayout.childControlHeight = true;
                headerLayout.childForceExpandWidth = false;
                UiFactory.SetLayoutSize(headerRow.gameObject, preferredHeight: 20f);

                var titleText = UiFactory.CreateText(headerRow, "Title", theme, title,
                    UiPalette.FontSizeLabel, UiPalette.TextPrimary, TextAnchor.MiddleLeft);
                UiFactory.SetLayoutSize(titleText.gameObject, flexibleWidth: 1f);

                _valueText = UiFactory.CreateText(headerRow, "Value", theme, string.Empty,
                    UiPalette.FontSizeLabel, UiPalette.TextSecondary, TextAnchor.MiddleRight);
                UiFactory.SetLayoutSize(_valueText.gameObject, preferredWidth: 90f);

                _slider = UiFactory.CreateSlider(row, "Slider", theme, min, max, initial);
                _slider.onValueChanged.AddListener(OnSliderChanged);

                SetValue(initial);
            }

            public void SetValue(float value)
            {
                _slider.SetValueWithoutNotify(value);
                _valueText.text = string.Format(_unitFormat, value);
            }

            public void SetInteractable(bool enabled) => _slider.interactable = enabled;

            private void OnSliderChanged(float raw)
            {
                float snapped = SnapToStep(raw, _step, _slider.minValue);
                if (!Mathf.Approximately(snapped, raw))
                {
                    _slider.SetValueWithoutNotify(snapped);
                }
                _valueText.text = string.Format(_unitFormat, snapped);
            }

            private static float SnapToStep(float value, float step, float min)
            {
                float stepsFromMin = Mathf.Round((value - min) / step);
                return min + stepsFromMin * step;
            }
        }

        /// <summary>朝・昼・夕1食分：量の選択＋野菜チェック。量が「抜き」のときは野菜チェックを無効化する（requirements.md 4.1節）。</summary>
        private sealed class MealRow
        {
            public readonly OptionGroup<MealAmount> AmountGroup = new OptionGroup<MealAmount>();
            private Toggle _vegetableToggle;

            public void Build(Transform parent, UiTheme theme, string title)
            {
                AmountGroup.Build(parent, theme, title, MealAmountChoices, MealAmount.Light);
                AmountGroup.Changed += OnAmountChanged;

                var vegRow = UiFactory.CreateRect(parent, $"Vegetable_{title}");
                var layout = vegRow.gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = UiPalette.SpacingSmall;
                layout.childAlignment = TextAnchor.MiddleLeft;
                layout.childForceExpandWidth = false;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                UiFactory.SetLayoutSize(vegRow.gameObject, preferredHeight: UiPalette.MinTapSize);

                _vegetableToggle = UiFactory.CreateToggle(vegRow, "VegToggle", theme, out _);

                var label = UiFactory.CreateText(vegRow, "VegLabel", theme, VegetableLabel,
                    UiPalette.FontSizeBody, UiPalette.TextPrimary, TextAnchor.MiddleLeft);
                UiFactory.SetLayoutSize(label.gameObject, flexibleWidth: 1f);
            }

            public MealEntry Read() => new MealEntry(AmountGroup.Selected, _vegetableToggle.isOn);

            public void Write(MealEntry entry)
            {
                AmountGroup.Select(entry.Amount, notify: false);
                _vegetableToggle.SetIsOnWithoutNotify(entry.HasVegetable);
                _vegetableToggle.interactable = entry.Amount != MealAmount.Skip;
            }

            public void SetInteractable(bool enabled)
            {
                AmountGroup.SetInteractable(enabled);
                _vegetableToggle.interactable = enabled && AmountGroup.Selected != MealAmount.Skip;
            }

            private void OnAmountChanged(MealAmount amount)
            {
                bool canHaveVegetable = amount != MealAmount.Skip;
                if (!canHaveVegetable)
                {
                    _vegetableToggle.SetIsOnWithoutNotify(false);
                }
                _vegetableToggle.interactable = canHaveVegetable;
            }
        }

        /// <summary>単一選択の選択肢チップ群（運動の強さ・食事の量・おやつ回数で共用）。
        /// 選択状態は色に加えチェックマークと太字でも示す（色のみで区別しない・requirements.md 3.3節）。</summary>
        private sealed class OptionGroup<T>
        {
            private readonly List<Option> _options = new List<Option>();
            private T _selected;

            public event Action<T> Changed;
            public T Selected => _selected;

            public void Build(Transform parent, UiTheme theme, string rowLabel, IReadOnlyList<(T value, string label)> choices, T initial)
            {
                var row = UiFactory.CreateRect(parent, $"OptionRow_{rowLabel}");
                var layout = row.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.spacing = UiPalette.SpacingXSmall;
                layout.childForceExpandWidth = true;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                UiFactory.SetLayoutSize(row.gameObject, preferredHeight: UiPalette.ChipHeight + 26f);

                var titleText = UiFactory.CreateText(row, "Title", theme, rowLabel,
                    UiPalette.FontSizeLabel, UiPalette.TextPrimary, TextAnchor.MiddleLeft);
                UiFactory.SetLayoutSize(titleText.gameObject, preferredHeight: 20f);

                var choicesRow = UiFactory.CreateRect(row, "Choices");
                var choicesLayout = choicesRow.gameObject.AddComponent<HorizontalLayoutGroup>();
                choicesLayout.spacing = UiPalette.SpacingSmall;
                choicesLayout.childForceExpandWidth = true;
                choicesLayout.childControlWidth = true;
                choicesLayout.childControlHeight = true;
                UiFactory.SetLayoutSize(choicesRow.gameObject, preferredHeight: UiPalette.ChipHeight);

                foreach (var (value, label) in choices)
                {
                    var button = UiFactory.CreateChipButton(choicesRow, $"Chip_{label}", theme, label,
                        UiPalette.FontSizeBody, out var chipText, out var chipBackground);
                    UiFactory.SetLayoutSize(button.gameObject, flexibleWidth: 1f, preferredHeight: UiPalette.ChipHeight);

                    var option = new Option { Value = value, Button = button, Background = chipBackground, Label = chipText, BaseLabel = label };
                    _options.Add(option);
                    button.onClick.AddListener(() => Select(value, notify: true));
                }

                SelectInternal(initial);
            }

            public void Select(T value, bool notify)
            {
                SelectInternal(value);
                if (notify) Changed?.Invoke(value);
            }

            public void SetInteractable(bool enabled)
            {
                foreach (var option in _options)
                {
                    option.Button.interactable = enabled;
                }
                RefreshVisuals();
            }

            private void SelectInternal(T value)
            {
                _selected = value;
                RefreshVisuals();
            }

            private void RefreshVisuals()
            {
                foreach (var option in _options)
                {
                    bool isSelected = EqualityComparer<T>.Default.Equals(option.Value, _selected);
                    ApplyVisual(option, isSelected, option.Button.interactable);
                }
            }

            private static void ApplyVisual(Option option, bool isSelected, bool interactable)
            {
                Color color = !interactable
                    ? UiPalette.AccentDisabled
                    : (isSelected ? UiPalette.SelectedColor : UiPalette.UnselectedColor);
                option.Background.color = color;
                option.Label.color = isSelected && interactable ? UiPalette.TextOnAccent : UiPalette.TextPrimary;
                option.Label.text = isSelected ? SelectedMarker + option.BaseLabel : option.BaseLabel;
                option.Label.fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;
            }

            private struct Option
            {
                public T Value;
                public Button Button;
                public Image Background;
                public Text Label;
                public string BaseLabel;
            }
        }
    }
}
