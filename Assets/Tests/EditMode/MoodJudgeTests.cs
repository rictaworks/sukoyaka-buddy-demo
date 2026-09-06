using NUnit.Framework;
using SukoyakaBuddy.Core;
using SukoyakaBuddy.Model;

namespace SukoyakaBuddy.Tests.EditMode
{
    public class MoodJudgeTests
    {
        private static Character Character(bool isSick = false) => new Character { IsSick = isSick };

        private static DayScore Score(int exercise, int sleep, int meal, bool overwork = false, bool shortSleep = false)
        {
            return new DayScore { Exercise = exercise, Sleep = sleep, Meal = meal, Overwork = overwork, ShortSleep = shortSleep };
        }

        [Test]
        public void Sick_AlwaysExhausted_RegardlessOfScore()
        {
            var result = MoodJudge.Judge(Score(90, 90, 90), Character(isSick: true));
            Assert.AreEqual(Mood.Exhausted, result.Mood);
        }

        [Test]
        public void MinScoreBelowThirty_IsExhausted_EvenIfNotSick()
        {
            var result = MoodJudge.Judge(Score(29, 90, 90), Character());
            Assert.AreEqual(Mood.Exhausted, result.Mood);
        }

        [Test]
        public void OverworkFlag_TakesPriorityOverHighScores_WhenNotExhausted()
        {
            var result = MoodJudge.Judge(Score(90, 90, 90, overwork: true), Character());
            Assert.AreEqual(Mood.Tired, result.Mood);
        }

        [Test]
        public void AllScoresSixtyOrAbove_IsGenki()
        {
            var result = MoodJudge.Judge(Score(60, 70, 80), Character());
            Assert.AreEqual(Mood.Genki, result.Mood);
        }

        [Test]
        public void Otherwise_IsNormal()
        {
            var result = MoodJudge.Judge(Score(40, 50, 55), Character());
            Assert.AreEqual(Mood.Normal, result.Mood);
        }

        [Test]
        public void Line_IsNotEmpty_ForEveryMood()
        {
            Assert.IsNotEmpty(MoodJudge.Judge(Score(90, 90, 90), Character()).Line);
            Assert.IsNotEmpty(MoodJudge.Judge(Score(10, 90, 90), Character()).Line);
        }
    }
}
