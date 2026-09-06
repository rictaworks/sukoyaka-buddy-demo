using NUnit.Framework;
using SukoyakaBuddy.Core;
using SukoyakaBuddy.Model;

namespace SukoyakaBuddy.Tests.EditMode
{
    public class InputNormalizerTests
    {
        private static DailyInput Base(int dayNo = 1) => DailyInput.Default(dayNo);

        [Test]
        public void ExerciseMinutes_RoundsToNearestStep_TieRoundsUp()
        {
            var input = Base();
            input.ExerciseMinutes = 22; // 20と25の中間ではないが、22は20との差2、25との差3なので20に丸まる
            var result = InputNormalizer.Normalize(input, input.DayNo);
            Assert.AreEqual(20, result.Log.ExerciseMinutes);

            input.ExerciseMinutes = 23; // 20との差3、25との差2 -> 25
            result = InputNormalizer.Normalize(input, input.DayNo);
            Assert.AreEqual(25, result.Log.ExerciseMinutes);
        }

        [Test]
        public void ExerciseMinutes_OutOfRange_DefaultsToZero()
        {
            var input = Base();
            input.ExerciseMinutes = 200;
            var result = InputNormalizer.Normalize(input, input.DayNo);
            Assert.AreEqual(0, result.Log.ExerciseMinutes);
        }

        [Test]
        public void SleepHours_RoundsToNearestHalfStep()
        {
            var input = Base();
            input.SleepHours = 6.2f; // 6.0との差0.2、6.5との差0.3 -> 6.0
            var result = InputNormalizer.Normalize(input, input.DayNo);
            Assert.AreEqual(6.0f, result.Log.SleepHours, 0.001f);

            input.SleepHours = 6.3f; // 6.0との差0.3、6.5との差0.2 -> 6.5
            result = InputNormalizer.Normalize(input, input.DayNo);
            Assert.AreEqual(6.5f, result.Log.SleepHours, 0.001f);
        }

        [Test]
        public void SleepHours_OutOfRange_DefaultsToSeven()
        {
            var input = Base();
            input.SleepHours = 20f;
            var result = InputNormalizer.Normalize(input, input.DayNo);
            Assert.AreEqual(GameConstants.DefaultSleepHours, result.Log.SleepHours, 0.001f);
        }

        [Test]
        public void Meal_SkipForcesVegetableFalse()
        {
            var input = Base();
            input.Breakfast = new MealEntry(MealAmount.Skip, true);
            var result = InputNormalizer.Normalize(input, input.DayNo);
            Assert.IsFalse(result.Log.Breakfast.HasVegetable);
        }

        [Test]
        public void Snacks_OutOfRange_DefaultsToZero()
        {
            var input = Base();
            input.Snacks = 5;
            var result = InputNormalizer.Normalize(input, input.DayNo);
            Assert.AreEqual(0, result.Log.Snacks);
        }

        [Test]
        public void DayNoMismatch_IsRejected()
        {
            var input = Base(dayNo: 3);
            var result = InputNormalizer.Normalize(input, currentDayNo: 4);
            Assert.IsFalse(result.Accepted);
            Assert.AreEqual("この日はすでに終わっています", result.RejectReason);
        }

        [Test]
        public void ValidInput_IsAccepted()
        {
            var input = Base(dayNo: 1);
            var result = InputNormalizer.Normalize(input, currentDayNo: 1);
            Assert.IsTrue(result.Accepted);
            Assert.IsNotNull(result.Log);
        }
    }
}
