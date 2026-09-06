using System;
using SukoyakaBuddy.Model;

namespace SukoyakaBuddy.Core
{
    /// <summary>関数C：成長適用（requirements.md 4.3節）。</summary>
    public static class GrowthEngine
    {
        public static GrowthResult Apply(Character character, DailyLog log, DayScore score)
        {
            var after = character.Clone();
            var result = new GrowthResult();

            // 1. 休養日の判定。
            bool isRestDay = log.ExerciseMinutes == 0
                              && character.Fatigue >= GameConstants.RestDayFatigueThreshold
                              && score.Sleep >= GameConstants.RestDaySleepScoreThreshold;
            result.IsRestDay = isRestDay;

            // 2. ステータス変化量の算出。
            int powerDelta = StatDelta(score.Exercise, isPower: true);
            if (isRestDay) powerDelta = 0;

            int energyDelta = StatDelta(score.Sleep, isPower: false);
            if (score.Oversleep) energyDelta = Math.Min(energyDelta, GameConstants.OversleepEnergyDeltaCap);
            if (score.Overwork) energyDelta += GameConstants.OverworkEnergyPenalty;

            int bodyDelta = StatDelta(score.Meal, isPower: false);
            if (score.Overeat) bodyDelta += GameConstants.OvereatBodyPenalty;
            if (score.VegetableRich) bodyDelta += GameConstants.VegetableRichBodyBonus;

            // 3. 体調不良中の減衰：正の変化量のみ半分（切り捨て）。負の変化量はそのまま。
            if (character.IsSick)
            {
                powerDelta = HalveIfPositive(powerDelta);
                energyDelta = HalveIfPositive(energyDelta);
                bodyDelta = HalveIfPositive(bodyDelta);
            }

            // 4. ステータスの更新（0〜100にクランプ）。
            after.Power = Clamp0To100(character.Power + powerDelta);
            after.Energy = Clamp0To100(character.Energy + energyDelta);
            after.Body = Clamp0To100(character.Body + bodyDelta);

            // 5. 連続日数の更新。
            bool isBalanceDay = score.Exercise >= GameConstants.BalanceDayScoreThreshold
                                 && score.Sleep >= GameConstants.BalanceDayScoreThreshold
                                 && score.Meal >= GameConstants.BalanceDayScoreThreshold;
            after.Streak = isBalanceDay ? character.Streak + 1 : 0;

            // 6. 獲得経験値の算出。
            int gainedExp = RoundHalfUp(score.Mean());
            if (after.Streak >= GameConstants.StreakBonusThreshold)
            {
                gainedExp = RoundHalfUp(gainedExp * GameConstants.StreakBonusMultiplier);
            }
            if (character.IsSick)
            {
                gainedExp = (int)Math.Floor(gainedExp / 2f);
            }
            result.GainedExp = gainedExp;

            // 7. 経験値と段階の更新。
            var stageBefore = character.GetStage();
            after.Exp = Math.Min(GameConstants.MaxExp, character.Exp + gainedExp);
            var stageAfter = after.GetStage();
            if (stageAfter != stageBefore)
            {
                result.Events.Add(GrowthEvent.StageUp(stageAfter));
            }

            // 8. 成長タイプの確定（おとな以上に到達し、かつ未確定のときのみ）。
            if (stageAfter >= Stage.Adult && after.GrowthType == GrowthType.Undecided)
            {
                after.GrowthType = DecideGrowthType(after.Power, after.Energy, after.Body);
                result.Events.Add(GrowthEvent.GrowthTypeDecided(after.GrowthType));
            }

            // 9. つかれの更新。
            int fatigueDelta = GameConstants.FatigueBaseRecover;
            if (score.Overwork) fatigueDelta += GameConstants.FatigueOverworkPenalty;
            if (score.ShortSleep) fatigueDelta += GameConstants.FatigueShortSleepPenalty;
            if (score.SkippedMeal) fatigueDelta += GameConstants.FatigueSkippedMealPenalty;
            if (score.Sleep >= GameConstants.GreatSleepScoreThreshold) fatigueDelta += GameConstants.FatigueGreatSleepBonus;
            if (isRestDay) fatigueDelta += GameConstants.FatigueRestDayBonus;
            after.Fatigue = Clamp0To100(character.Fatigue + fatigueDelta);

            // 10. 体調の判定（開始80／回復50未満で境界に差を設ける）。
            after.IsSick = character.IsSick;
            if (!character.IsSick && after.Fatigue >= GameConstants.SickStartThreshold)
            {
                after.IsSick = true;
                result.Events.Add(GrowthEvent.SickStart());
            }
            else if (character.IsSick && after.Fatigue < GameConstants.SickRecoverThreshold)
            {
                after.IsSick = false;
                result.Events.Add(GrowthEvent.SickRecover());
            }

            // 11. 直近7日の履歴に追加。8日以上になれば最も古い日を捨てる。
            after.History.Add(new DayRecord(log.DayNo, score.Exercise, score.Sleep, score.Meal));
            while (after.History.Count > GameConstants.HistoryDays)
            {
                after.History.RemoveAt(0);
            }

            result.After = after;
            return result;
        }

        /// <summary>requirements.md 4.9節の変化量テーブル。ちからは20未満の扱いが異なる。</summary>
        private static int StatDelta(int score, bool isPower)
        {
            if (score >= 80) return GameConstants.StatDeltaHigh;
            if (score >= 60) return GameConstants.StatDeltaMidHigh;
            if (score >= 40) return GameConstants.StatDeltaMid;
            if (score >= 20) return GameConstants.StatDeltaLow;
            return isPower ? GameConstants.PowerDeltaVeryLow : GameConstants.StatDeltaVeryLow;
        }

        /// <summary>成長タイプ確定規則。SaveRepositoryの読込時再計算からも同じ規則で呼ばれる（requirements.md 4.7節 手順3）。</summary>
        public static GrowthType DecideGrowthType(int power, int energy, int body)
        {
            int max = Math.Max(power, Math.Max(energy, body));
            int secondMax = SecondMax(power, energy, body);
            if (max - secondMax <= GameConstants.BalancedTypeGapThreshold)
            {
                return GrowthType.Balanced;
            }
            if (power == max) return GrowthType.Athlete;
            if (energy == max) return GrowthType.Laidback;
            return GrowthType.Gourmet;
        }

        /// <summary>3値を昇順に並べたときの中央値＝2番目に大きい値。最大値が複数あっても差0として扱える。</summary>
        private static int SecondMax(int a, int b, int c)
        {
            int[] values = { a, b, c };
            Array.Sort(values);
            return values[1];
        }

        private static int HalveIfPositive(int delta) => delta > 0 ? (int)Math.Floor(delta / 2f) : delta;

        private static int RoundHalfUp(float value) => (int)Math.Floor(value + 0.5f);

        private static int Clamp0To100(int value) => Math.Max(0, Math.Min(100, value));
    }
}
