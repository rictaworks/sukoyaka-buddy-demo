using System;

namespace SukoyakaBuddy.Model
{
    /// <summary>記録フォームの生入力値。UI側で範囲・刻みを制限した上で渡すが、InputNormalizerが最終検証する（requirements.md 12.2節）。</summary>
    [Serializable]
    public class DailyInput
    {
        public int ExerciseMinutes;
        public Intensity Intensity;
        public float SleepHours;
        public MealEntry Breakfast;
        public MealEntry Lunch;
        public MealEntry Dinner;
        public int Snacks;
        public int DayNo;

        public static DailyInput Default(int dayNo)
        {
            return new DailyInput
            {
                ExerciseMinutes = 0,
                Intensity = Intensity.Normal,
                SleepHours = 7f,
                Breakfast = new MealEntry(MealAmount.Light, false),
                Lunch = new MealEntry(MealAmount.Light, false),
                Dinner = new MealEntry(MealAmount.Light, false),
                Snacks = 0,
                DayNo = dayNo
            };
        }
    }
}
