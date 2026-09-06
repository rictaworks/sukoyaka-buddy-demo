using System.Linq;
using NUnit.Framework;
using SukoyakaBuddy.Core;
using SukoyakaBuddy.Model;

namespace SukoyakaBuddy.Tests.EditMode
{
    public class GrowthEngineTests
    {
        private static Character NewCharacter(int power = 50, int energy = 50, int body = 50,
            int fatigue = 0, bool isSick = false, int streak = 0, int exp = 0,
            GrowthType growthType = GrowthType.Undecided)
        {
            return new Character
            {
                DayNo = 1,
                Exp = exp,
                Power = power,
                Energy = energy,
                Body = body,
                Fatigue = fatigue,
                IsSick = isSick,
                Streak = streak,
                GrowthType = growthType
            };
        }

        private static DailyLog LogWithMinutes(int minutes) => new DailyLog { DayNo = 1, ExerciseMinutes = minutes, SleepHours = 7f };

        private static DayScore Score(int exercise, int sleep, int meal, bool overwork = false,
            bool shortSleep = false, bool oversleep = false, bool skippedMeal = false,
            bool overeat = false, bool vegetableRich = false)
        {
            return new DayScore
            {
                Exercise = exercise,
                Sleep = sleep,
                Meal = meal,
                Overwork = overwork,
                ShortSleep = shortSleep,
                Oversleep = oversleep,
                SkippedMeal = skippedMeal,
                Overeat = overeat,
                VegetableRich = vegetableRich
            };
        }

        [Test]
        public void HighScores_IncreaseStatsAndStreak_NoBonusBelowThreeDays()
        {
            var character = NewCharacter();
            var result = GrowthEngine.Apply(character, LogWithMinutes(30), Score(85, 85, 85));

            Assert.AreEqual(56, result.After.Power);
            Assert.AreEqual(56, result.After.Energy);
            Assert.AreEqual(56, result.After.Body);
            Assert.AreEqual(1, result.After.Streak);
            Assert.AreEqual(85, result.GainedExp, "連続3日未満はボーナスなし");
        }

        [Test]
        public void VeryLowExerciseScore_UsesMinusTwoForPowerOnly()
        {
            var character = NewCharacter();
            var result = GrowthEngine.Apply(character, LogWithMinutes(30), Score(10, 10, 10));

            Assert.AreEqual(48, result.After.Power, "ちからの20未満は-2");
            Assert.AreEqual(47, result.After.Energy, "げんきの20未満は-3");
            Assert.AreEqual(47, result.After.Body, "からだの20未満は-3");
        }

        [Test]
        public void RestDay_KeepsPowerUnchanged_AndReducesFatigueMore()
        {
            var character = NewCharacter(fatigue: 40);
            var result = GrowthEngine.Apply(character, LogWithMinutes(0), Score(0, 70, 70));

            Assert.IsTrue(result.IsRestDay);
            Assert.AreEqual(character.Power, result.After.Power, "休養日はちから変化なし");
            Assert.AreEqual(20, result.After.Fatigue, "休養日は基礎-5に加えて-15");
        }

        [Test]
        public void Sick_HalvesPositiveDeltaAndGainedExp_ButNotNegative()
        {
            var character = NewCharacter(isSick: true, fatigue: 90);
            var result = GrowthEngine.Apply(character, LogWithMinutes(30), Score(85, 85, 85));

            Assert.AreEqual(53, result.After.Power);
            Assert.AreEqual(53, result.After.Energy);
            Assert.AreEqual(53, result.After.Body);
            Assert.AreEqual(42, result.GainedExp, "体調不良中は獲得経験値も半分（切り捨て）");
        }

        [Test]
        public void StageUp_EmitsOneEvent_WhenCrossingThreshold()
        {
            var character = NewCharacter(exp: 95);
            var result = GrowthEngine.Apply(character, LogWithMinutes(30), Score(60, 60, 60));

            Assert.AreEqual(155, result.After.Exp);
            Assert.IsTrue(result.Events.Any(e => e.Type == EventType.StageUp && e.Stage == Stage.Child));
        }

        [Test]
        public void GrowthType_DecidedOnceReachingAdult_UsesDominantStat()
        {
            var character = NewCharacter(power: 70, energy: 60, body: 55, exp: 295);
            var result = GrowthEngine.Apply(character, LogWithMinutes(30), Score(20, 20, 20));

            Assert.AreEqual(Stage.Adult, result.After.GetStage());
            Assert.AreEqual(GrowthType.Athlete, result.After.GrowthType);
            Assert.IsTrue(result.Events.Any(e => e.Type == EventType.GrowthTypeDecided && e.GrowthType == GrowthType.Athlete));
        }

        [Test]
        public void GrowthType_Balanced_WhenStatsAreClose()
        {
            var character = NewCharacter(power: 62, energy: 60, body: 58, exp: 295);
            var result = GrowthEngine.Apply(character, LogWithMinutes(30), Score(20, 20, 20));

            Assert.AreEqual(GrowthType.Balanced, result.After.GrowthType);
        }

        [Test]
        public void Fatigue_ReachesEighty_TriggersSickStart()
        {
            var character = NewCharacter(fatigue: 70);
            var result = GrowthEngine.Apply(character, LogWithMinutes(120), Score(50, 50, 50, overwork: true));

            Assert.AreEqual(80, result.After.Fatigue);
            Assert.IsTrue(result.After.IsSick);
            Assert.IsTrue(result.Events.Any(e => e.Type == EventType.SickStart));
        }

        [Test]
        public void Fatigue_BelowFifty_TriggersSickRecover()
        {
            var character = NewCharacter(isSick: true, fatigue: 55);
            var result = GrowthEngine.Apply(character, LogWithMinutes(0), Score(50, 70, 50));

            Assert.Less(result.After.Fatigue, 50);
            Assert.IsFalse(result.After.IsSick);
            Assert.IsTrue(result.Events.Any(e => e.Type == EventType.SickRecover));
        }

        [Test]
        public void History_KeepsOnlyLastSevenDays()
        {
            var character = NewCharacter();
            for (int day = 1; day <= 8; day++)
            {
                character.DayNo = day;
                var log = new DailyLog { DayNo = day, ExerciseMinutes = 30, SleepHours = 7f };
                var result = GrowthEngine.Apply(character, log, Score(50, 50, 50));
                character = result.After;
            }

            Assert.AreEqual(7, character.History.Count);
            Assert.AreEqual(2, character.History[0].DayNo, "最も古い1日目は捨てられている");
            Assert.AreEqual(8, character.History[^1].DayNo);
        }
    }
}
