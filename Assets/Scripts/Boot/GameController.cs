using System;
using System.Collections;
using System.Collections.Generic;
using SukoyakaBuddy.Core;
using SukoyakaBuddy.Model;
using SukoyakaBuddy.Persistence;
using SukoyakaBuddy.View;
using UnityEngine;
using UnityEngine.UI;
using EventType = SukoyakaBuddy.Model.EventType;

namespace SukoyakaBuddy.Boot
{
    /// <summary>
    /// ゲーム本体の状態機械（requirements.md 10.3節）。
    /// LOADING → INPUT → PROCESSING → REACTING →（EVOLVING）→ INPUT のサイクルを回し、
    /// はじめからはINPUT → CONFIRM_RESET → LOADINGを辿る。
    /// PROCESSING・REACTING・EVOLVING・CONFIRM_RESETの間は「1日を終える」「はじめから」を受け付けない（二重防御の一環）。
    /// </summary>
    public class GameController : MonoBehaviour
    {
        private const string MessageSaveFailed = "保存できませんでした";
        private const string GreetingNewGame = "はじめまして！いっしょにそだてよう！";
        private const string GreetingContinue = "おかえり！つづきをはじめよう！";
        private const string PresetRowTitle = "プリセット";
        private const string EndDayButtonLabel = "1日を終える";
        private const string ResetButtonLabel = "はじめから";

        private const float ColumnGap = UiPalette.Spacing;
        private const float LeftColumnWidth = 440f;
        private const float BuddyAreaHeight = 380f;
        private const float PresetRowHeight = 52f;
        private const float ActionRowHeight = 60f;

        private enum GameState { Loading, Input, Processing, Reacting, Evolving, ConfirmReset }

        private readonly SaveRepository _saveRepository = new SaveRepository();
        private readonly ResetPolicy _resetPolicy = new ResetPolicy();
        private readonly List<Button> _presetButtons = new List<Button>();

        private UiTheme _theme;
        private Character _character;
        private GameState _state;
        private string _playDate;

        private InputForm _inputForm;
        private BuddyView _buddyView;
        private StatusPanel _statusPanel;
        private HistoryChart _historyChart;
        private ConfirmResetDialog _confirmDialog;
        private Button _endDayButton;
        private Button _resetButton;

        public void Initialize(RectTransform root, UiTheme theme)
        {
            _theme = theme;
            _state = GameState.Loading;

            BuildLayout(root);
            bool isNewGame = LoadGame();

            _buddyView.SetMood(Mood.Normal, isNewGame ? GreetingNewGame : GreetingContinue);
            _state = GameState.Input;
        }

        private void BuildLayout(RectTransform root)
        {
            var mainRow = UiFactory.CreateRect(root, "MainRow");
            UiFactory.StretchFull(mainRow);
            var mainLayout = mainRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            mainLayout.padding = new RectOffset((int)UiPalette.Spacing, (int)UiPalette.Spacing,
                (int)UiPalette.Spacing, (int)UiPalette.Spacing);
            mainLayout.spacing = ColumnGap;
            mainLayout.childForceExpandWidth = false;
            mainLayout.childForceExpandHeight = true;
            mainLayout.childControlWidth = true;
            mainLayout.childControlHeight = true;

            var leftColumn = UiFactory.CreateRect(mainRow, "LeftColumn");
            var leftLayout = leftColumn.gameObject.AddComponent<VerticalLayoutGroup>();
            leftLayout.spacing = UiPalette.Spacing;
            leftLayout.childForceExpandWidth = true;
            leftLayout.childForceExpandHeight = false;
            leftLayout.childControlWidth = true;
            leftLayout.childControlHeight = true;
            UiFactory.SetLayoutSize(leftColumn.gameObject, preferredWidth: LeftColumnWidth);

            var buddyContainer = UiFactory.CreatePanel(leftColumn, "BuddyContainer", _theme.RoundedRectSprite, UiPalette.Panel);
            UiFactory.SetLayoutSize(buddyContainer.gameObject, preferredHeight: BuddyAreaHeight);
            _buddyView = buddyContainer.gameObject.AddComponent<BuddyView>();
            _buddyView.Initialize(buddyContainer.rectTransform, _theme);

            _historyChart = new HistoryChart();
            _historyChart.Build(leftColumn, _theme);

            var rightColumn = UiFactory.CreateRect(mainRow, "RightColumn");
            var rightLayout = rightColumn.gameObject.AddComponent<VerticalLayoutGroup>();
            rightLayout.spacing = UiPalette.Spacing;
            rightLayout.childForceExpandWidth = true;
            rightLayout.childForceExpandHeight = false;
            rightLayout.childControlWidth = true;
            rightLayout.childControlHeight = true;
            UiFactory.SetLayoutSize(rightColumn.gameObject, flexibleWidth: 1f);

            _statusPanel = new StatusPanel();
            _statusPanel.Build(rightColumn, _theme);

            _inputForm = new InputForm();
            _inputForm.Build(rightColumn, _theme);

            BuildPresetRow(rightColumn);
            BuildActionRow(rightColumn);

            _confirmDialog = new ConfirmResetDialog();
            _confirmDialog.Build(root, _theme);
        }

        private void BuildPresetRow(Transform parent)
        {
            var container = UiFactory.CreateRect(parent, "PresetRow");
            var layout = container.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = UiPalette.SpacingXSmall;
            layout.childForceExpandWidth = true;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            UiFactory.SetLayoutSize(container.gameObject, preferredHeight: PresetRowHeight + 22f);

            var title = UiFactory.CreateText(container, "Title", _theme, PresetRowTitle,
                UiPalette.FontSizeLabel, UiPalette.TextSecondary, TextAnchor.MiddleLeft);
            UiFactory.SetLayoutSize(title.gameObject, preferredHeight: 20f);

            var buttonsRow = UiFactory.CreateRect(container, "Buttons");
            var buttonsLayout = buttonsRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            buttonsLayout.spacing = UiPalette.SpacingSmall;
            buttonsLayout.childForceExpandWidth = true;
            buttonsLayout.childControlWidth = true;
            buttonsLayout.childControlHeight = true;
            UiFactory.SetLayoutSize(buttonsRow.gameObject, preferredHeight: PresetRowHeight);

            foreach (Preset preset in (Preset[])Enum.GetValues(typeof(Preset)))
            {
                var button = UiFactory.CreateButton(buttonsRow, $"Preset_{preset}", _theme, DisplayNames.PresetLabel(preset),
                    UiPalette.PanelAlt, UiPalette.TextPrimary, UiPalette.FontSizeBody, out _);
                UiFactory.SetLayoutSize(button.gameObject, flexibleWidth: 1f, preferredHeight: PresetRowHeight);
                _presetButtons.Add(button);
                button.onClick.AddListener(() => OnPresetSelected(preset));
            }
        }

        private void BuildActionRow(Transform parent)
        {
            var row = UiFactory.CreateRect(parent, "ActionRow");
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = UiPalette.Spacing;
            layout.childForceExpandWidth = true;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            UiFactory.SetLayoutSize(row.gameObject, preferredHeight: ActionRowHeight);

            _resetButton = UiFactory.CreateButton(row, "ResetButton", _theme, ResetButtonLabel,
                UiPalette.AccentDanger, UiPalette.TextOnAccent, UiPalette.FontSizeButton, out _);
            UiFactory.SetLayoutSize(_resetButton.gameObject, preferredWidth: 160f, preferredHeight: ActionRowHeight);
            _resetButton.onClick.AddListener(OnResetPressed);

            _endDayButton = UiFactory.CreateButton(row, "EndDayButton", _theme, EndDayButtonLabel,
                UiPalette.AccentPrimary, UiPalette.TextOnAccent, UiPalette.FontSizeButton, out _);
            UiFactory.SetLayoutSize(_endDayButton.gameObject, flexibleWidth: 1f, preferredHeight: ActionRowHeight);
            _endDayButton.onClick.AddListener(OnEndDayPressed);
        }

        /// <summary>関数H・関数G（requirements.md 4.8節・8.1節）。ページロード時のみ呼ぶ。戻り値は新規開始か否か。</summary>
        private bool LoadGame()
        {
            var utcNow = DateTime.UtcNow;
            _resetPolicy.CheckAndReset(utcNow);
            _playDate = ResetPolicy.BusinessDate(utcNow);
            _character = _saveRepository.Load(_playDate, out bool isNewGame);
            RefreshAllViews();
            return isNewGame;
        }

        private void RefreshAllViews()
        {
            _statusPanel.Render(_character);
            _historyChart.Render(_character.History);
            _buddyView.Render(_character);
        }

        private void OnPresetSelected(Preset preset)
        {
            if (_state != GameState.Input) return;
            _inputForm.Write(PresetCatalog.Get(preset, _character.DayNo));
        }

        private void OnEndDayPressed()
        {
            if (_state != GameState.Input) return;
            StartCoroutine(EndDayRoutine());
        }

        /// <summary>関数E：1日を終える（requirements.md 4.5節・8.2節）。</summary>
        private IEnumerator EndDayRoutine()
        {
            _state = GameState.Processing;
            SetInteractable(false);

            var raw = _inputForm.Read();
            raw.DayNo = _character.DayNo;
            var normalized = InputNormalizer.Normalize(raw, _character.DayNo);
            _inputForm.Write(normalized.CorrectedInput);

            if (!normalized.Accepted)
            {
                _buddyView.ShowMessage(normalized.RejectReason);
                _state = GameState.Input;
                SetInteractable(true);
                yield break;
            }

            var score = DayScorer.Score(normalized.Log);
            var growth = GrowthEngine.Apply(_character, normalized.Log, score);
            var mood = MoodJudge.Judge(score, growth.After);

            var updated = growth.After;
            updated.DayNo = _character.DayNo + 1;

            bool saved = _saveRepository.Save(updated, _playDate);
            if (!saved)
            {
                // 保存失敗時はキャラクターを保存前の状態に戻す（_characterへは未反映のため何もしないだけでよい）。
                _buddyView.ShowMessage(MessageSaveFailed);
                _state = GameState.Input;
                SetInteractable(true);
                yield break;
            }

            _character = updated;

            _state = GameState.Reacting;
            _buddyView.SetMood(mood.Mood, mood.Line);
            yield return new WaitForSeconds(BuddyView.ReactionAnimationSeconds);

            if (growth.Events.Count > 0)
            {
                _state = GameState.Evolving;
                foreach (var evt in growth.Events)
                {
                    yield return StartCoroutine(PlayEventAnimation(evt));
                }
            }

            RefreshAllViews();
            _inputForm.ResetToDefault();
            _state = GameState.Input;
            SetInteractable(true);
        }

        private IEnumerator PlayEventAnimation(GrowthEvent evt)
        {
            switch (evt.Type)
            {
                case EventType.StageUp:
                    _buddyView.PlayEvolution(evt.Stage, _character.GrowthType);
                    yield return new WaitForSeconds(BuddyView.EvolutionAnimationSeconds);
                    break;
                case EventType.GrowthTypeDecided:
                    _buddyView.PlayEvolution(_character.GetStage(), evt.GrowthType);
                    yield return new WaitForSeconds(BuddyView.EvolutionAnimationSeconds);
                    break;
                case EventType.SickStart:
                    _buddyView.PlaySick();
                    yield return new WaitForSeconds(BuddyView.SickAnimationSeconds);
                    break;
                case EventType.SickRecover:
                    _buddyView.PlayRecover();
                    yield return new WaitForSeconds(BuddyView.RecoverAnimationSeconds);
                    break;
            }
        }

        private void OnResetPressed()
        {
            if (_state != GameState.Input) return;
            _state = GameState.ConfirmReset;
            SetInteractable(false);
            _confirmDialog.Show(ConfirmReset, CancelReset);
        }

        private void CancelReset()
        {
            _state = GameState.Input;
            SetInteractable(true);
        }

        /// <summary>関数F12：はじめから。ResetPolicyが日付リセット時に行うPlayerPrefs全削除と同一の操作を、
        /// 来場者の明示的な操作で行う（端末ローカルのゲームセーブデータのみを対象とする）。</summary>
        private void ConfirmReset()
        {
            PlayerPrefs.DeleteAll();
            _state = GameState.Loading;
            LoadGame();
            _inputForm.ResetToDefault();
            _buddyView.SetMood(Mood.Normal, GreetingNewGame);
            _state = GameState.Input;
            SetInteractable(true);
        }

        private void SetInteractable(bool enabled)
        {
            _inputForm.SetEnabled(enabled);
            _endDayButton.interactable = enabled;
            _resetButton.interactable = enabled;
            foreach (var button in _presetButtons)
            {
                button.interactable = enabled;
            }
        }
    }
}
