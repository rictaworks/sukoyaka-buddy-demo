using System;
using System.Collections.Generic;
using System.Globalization;
using SukoyakaBuddy.Core;
using SukoyakaBuddy.Model;
using UnityEngine;

namespace SukoyakaBuddy.Persistence
{
    /// <summary>関数G：保存と読込（requirements.md 4.7節）。PlayerPrefsのみを対象とする。</summary>
    public class SaveRepository
    {
        private const string KeySchemaVersion = "schema_version";
        private const string KeyPlayDate = "play_date";
        private const string KeySaveSeq = "save_seq";
        private const string KeyDayNo = "day_no";
        private const string KeyExp = "exp";
        private const string KeyPower = "power";
        private const string KeyEnergy = "energy";
        private const string KeyBody = "body";
        private const string KeyFatigue = "fatigue";
        private const string KeyIsSick = "is_sick";
        private const string KeyStreak = "streak";
        private const string KeyGrowthType = "growth_type";
        private const string KeyHistory = "history";

        public int SchemaVersion => GameConstants.SchemaVersion;

        /// <summary>すべてのキーを書いてからSaveを呼ぶ。失敗時は呼び出し元がキャラクターを保存前の状態に戻す（requirements.md 4.5節 手順4）。</summary>
        public bool Save(Character character, string playDate)
        {
            try
            {
                PlayerPrefs.SetInt(KeySchemaVersion, GameConstants.SchemaVersion);
                PlayerPrefs.SetString(KeyPlayDate, playDate);
                PlayerPrefs.SetInt(KeyDayNo, character.DayNo);
                PlayerPrefs.SetInt(KeyExp, character.Exp);
                PlayerPrefs.SetInt(KeyPower, character.Power);
                PlayerPrefs.SetInt(KeyEnergy, character.Energy);
                PlayerPrefs.SetInt(KeyBody, character.Body);
                PlayerPrefs.SetInt(KeyFatigue, character.Fatigue);
                PlayerPrefs.SetInt(KeyIsSick, character.IsSick ? 1 : 0);
                PlayerPrefs.SetInt(KeyStreak, character.Streak);
                PlayerPrefs.SetString(KeyGrowthType, character.GrowthType.ToString());
                PlayerPrefs.SetString(KeyHistory, SerializeHistory(character.History));

                int seq = PlayerPrefs.GetInt(KeySaveSeq, 0) + 1;
                PlayerPrefs.SetInt(KeySaveSeq, seq);
                PlayerPrefs.Save();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>版数・型・範囲を検証し、1項目でも不正なら新規開始とする（requirements.md 4.7節・12.3節）。</summary>
        public Character Load(string playDate, out bool isNewGame)
        {
            if (!PlayerPrefs.HasKey(KeySchemaVersion) || PlayerPrefs.GetInt(KeySchemaVersion, -1) != GameConstants.SchemaVersion)
            {
                isNewGame = true;
                return CreateAndSaveNewGame(playDate);
            }

            if (!TryLoadRaw(out var character))
            {
                isNewGame = true;
                return CreateAndSaveNewGame(playDate);
            }

            // 成長タイプ未確定・段階おとな以上なら、読込時点のステータスで確定する。
            if (character.GrowthType == GrowthType.Undecided && character.GetStage() >= Stage.Adult)
            {
                character.GrowthType = GrowthEngine.DecideGrowthType(character.Power, character.Energy, character.Body);
            }

            // 体調不良の有無とつかれの整合を確認する。
            if (!character.IsSick && character.Fatigue >= GameConstants.SickStartThreshold)
            {
                character.IsSick = true;
            }
            else if (character.IsSick && character.Fatigue < GameConstants.SickRecoverThreshold)
            {
                character.IsSick = false;
            }

            // 履歴は7日を超える分を古い側から捨てる。
            while (character.History.Count > GameConstants.HistoryDays)
            {
                character.History.RemoveAt(0);
            }

            isNewGame = false;
            return character;
        }

        private Character CreateAndSaveNewGame(string playDate)
        {
            var character = Character.NewEgg();
            Save(character, playDate);
            return character;
        }

        private bool TryLoadRaw(out Character character)
        {
            character = null;
            try
            {
                int dayNo = PlayerPrefs.GetInt(KeyDayNo, -1);
                int exp = PlayerPrefs.GetInt(KeyExp, -1);
                int power = PlayerPrefs.GetInt(KeyPower, -1);
                int energy = PlayerPrefs.GetInt(KeyEnergy, -1);
                int body = PlayerPrefs.GetInt(KeyBody, -1);
                int fatigue = PlayerPrefs.GetInt(KeyFatigue, -1);
                int isSickRaw = PlayerPrefs.GetInt(KeyIsSick, -1);
                int streak = PlayerPrefs.GetInt(KeyStreak, -1);
                string growthTypeRaw = PlayerPrefs.GetString(KeyGrowthType, null);
                string historyRaw = PlayerPrefs.GetString(KeyHistory, null);

                if (dayNo < 1) return false;
                if (exp < 0 || exp > GameConstants.MaxExp) return false;
                if (!InRange(power) || !InRange(energy) || !InRange(body) || !InRange(fatigue)) return false;
                if (isSickRaw != 0 && isSickRaw != 1) return false;
                if (streak < 0) return false;
                if (growthTypeRaw == null || !Enum.TryParse(growthTypeRaw, out GrowthType growthType)) return false;
                if (historyRaw == null || !TryDeserializeHistory(historyRaw, out var history)) return false;

                character = new Character
                {
                    DayNo = dayNo,
                    Exp = exp,
                    Power = power,
                    Energy = energy,
                    Body = body,
                    Fatigue = fatigue,
                    IsSick = isSickRaw == 1,
                    Streak = streak,
                    GrowthType = growthType,
                    History = history
                };
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool InRange(int value) => value >= 0 && value <= 100;

        private static string SerializeHistory(List<DayRecord> history)
        {
            var parts = new List<string>(history.Count);
            foreach (var record in history)
            {
                parts.Add(string.Join(",",
                    record.DayNo.ToString(CultureInfo.InvariantCulture),
                    record.Exercise.ToString(CultureInfo.InvariantCulture),
                    record.Sleep.ToString(CultureInfo.InvariantCulture),
                    record.Meal.ToString(CultureInfo.InvariantCulture)));
            }
            return string.Join(";", parts);
        }

        private static bool TryDeserializeHistory(string raw, out List<DayRecord> history)
        {
            history = new List<DayRecord>();
            if (string.IsNullOrEmpty(raw)) return true;

            foreach (var entry in raw.Split(';'))
            {
                if (string.IsNullOrEmpty(entry)) continue;
                var fields = entry.Split(',');
                if (fields.Length != 4) return false;
                if (!int.TryParse(fields[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int dayNo)) return false;
                if (!int.TryParse(fields[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int exercise)) return false;
                if (!int.TryParse(fields[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int sleep)) return false;
                if (!int.TryParse(fields[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out int meal)) return false;
                history.Add(new DayRecord(dayNo, exercise, sleep, meal));
            }
            return true;
        }
    }
}
