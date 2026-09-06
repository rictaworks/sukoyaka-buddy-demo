using System;

namespace SukoyakaBuddy.Model
{
    /// <summary>DayScorerの出力（requirements.md 4.2節）。</summary>
    [Serializable]
    public class DayScore
    {
        public int Exercise;
        public int Sleep;
        public int Meal;
        public bool Overwork;
        public bool ShortSleep;
        public bool Oversleep;
        public bool SkippedMeal;
        public bool Overeat;
        public bool VegetableRich;

        public int Min() => Math.Min(Exercise, Math.Min(Sleep, Meal));
        public float Mean() => (Exercise + Sleep + Meal) / 3f;

        /// <summary>最も低いスコアの項目。同点は運動→睡眠→食事の順で優先する（requirements.md 4.4節）。</summary>
        public ScoreCategory LowestCategory()
        {
            if (Exercise <= Sleep && Exercise <= Meal) return ScoreCategory.Exercise;
            if (Sleep <= Meal) return ScoreCategory.Sleep;
            return ScoreCategory.Meal;
        }
    }
}
