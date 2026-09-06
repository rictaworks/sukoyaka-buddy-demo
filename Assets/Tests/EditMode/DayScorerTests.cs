using NUnit.Framework;
using SukoyakaBuddy.Core;
using SukoyakaBuddy.Model;

namespace SukoyakaBuddy.Tests.EditMode
{
    public class DayScorerTests
    {
        private static DailyLog LogWith(int exerciseMinutes = 0, Intensity intensity = Intensity.Normal,
            float sleepHours = 7f, MealAmount b = MealAmount.Light, MealAmount l = MealAmount.Light,
            MealAmount d = MealAmount.Light, bool bv = false, bool lv = false, bool dv = false, int snacks = 0)
        {
            return new DailyLog
            {
                ExerciseMinutes = exerciseMinutes,
                Intensity = intensity,
                SleepHours = sleepHours,
                Breakfast = new MealEntry(b, bv),
                Lunch = new MealEntry(l, lv),
                Dinner = new MealEntry(d, dv),
                Snacks = snacks,
                DayNo = 1
            };
        }

        [TestCase(0, 0)]
        [TestCase(30, 70)]
        [TestCase(60, 100)]
        [TestCase(120, 100)]
        public void ExerciseScore_MatchesAnchors_NoOverwork(int minutes, int expectedScore)
        {
            var score = DayScorer.Score(LogWith(exerciseMinutes: minutes, intensity: Intensity.Normal));
            Assert.AreEqual(expectedScore, score.Exercise);
            Assert.IsFalse(score.Overwork, "運動量120までは過剰運動フラグが立たない");
        }

        [Test]
        public void ExerciseAmount_Over120_SetsOverworkFlag()
        {
            // 運動量 = 121（強さふつう=1.0）
            var score = DayScorer.Score(LogWith(exerciseMinutes: 121, intensity: Intensity.Normal));
            Assert.IsTrue(score.Overwork);
        }

        [TestCase(6f, false, false)]
        [TestCase(5.999f, true, false)]
        [TestCase(10f, false, false)]
        [TestCase(10.001f, false, true)]
        public void SleepFlags_BoundaryValues(float hours, bool expectedShortSleep, bool expectedOversleep)
        {
            var score = DayScorer.Score(LogWith(sleepHours: hours));
            Assert.AreEqual(expectedShortSleep, score.ShortSleep);
            Assert.AreEqual(expectedOversleep, score.Oversleep);
        }

        [Test]
        public void MealScore_VegetableBonusAndSnackPenalty()
        {
            // 3食しっかり(30)+野菜あり(+5)=35×3=105、おやつ2回で(2-1)*5=5減点 -> 100（クランプ）
            var score = DayScorer.Score(LogWith(b: MealAmount.Full, l: MealAmount.Full, d: MealAmount.Full,
                bv: true, lv: true, dv: true, snacks: 2));
            Assert.AreEqual(100, score.Meal);
            Assert.IsTrue(score.VegetableRich);
        }

        [Test]
        public void MealScore_SkipAndOvereatFlags()
        {
            var score = DayScorer.Score(LogWith(b: MealAmount.Skip, l: MealAmount.Overeat, d: MealAmount.Light));
            Assert.IsTrue(score.SkippedMeal);
            Assert.IsTrue(score.Overeat);
            Assert.IsFalse(score.VegetableRich);
        }

        [Test]
        public void Snacks_ThreeTimes_SetsOvereatFlag()
        {
            var score = DayScorer.Score(LogWith(snacks: 3));
            Assert.IsTrue(score.Overeat);
        }

        [Test]
        public void Snacks_OneTime_NoPenalty()
        {
            var withoutSnack = DayScorer.Score(LogWith(b: MealAmount.Full, l: MealAmount.Full, d: MealAmount.Full, snacks: 0));
            var withOneSnack = DayScorer.Score(LogWith(b: MealAmount.Full, l: MealAmount.Full, d: MealAmount.Full, snacks: 1));
            Assert.AreEqual(withoutSnack.Meal, withOneSnack.Meal, "おやつ1回目は減点しない");
        }
    }
}
