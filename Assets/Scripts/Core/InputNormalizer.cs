using SukoyakaBuddy.Model;

namespace SukoyakaBuddy.Core
{
    /// <summary>関数A：入力の正規化（requirements.md 4.1節）。</summary>
    public static class InputNormalizer
    {
        public static NormalizationResult Normalize(DailyInput raw, int currentDayNo)
        {
            var corrected = new DailyInput
            {
                DayNo = raw.DayNo
            };

            // 1. 運動時間：0〜180の整数、5の倍数に丸める（同距離は切り上げ）。範囲外・非数は既定値0。
            corrected.ExerciseMinutes = NormalizeExerciseMinutes(raw.ExerciseMinutes);

            // 2. 運動の強さ：軽い/ふつう/激しい以外は「ふつう」。運動時間0のときは判定に用いないが値は保持する。
            corrected.Intensity = IsValidIntensity(raw.Intensity) ? raw.Intensity : Intensity.Normal;

            // 3. 睡眠時間：0〜14の0.5刻み。範囲外・非数は既定値7。
            corrected.SleepHours = NormalizeSleepHours(raw.SleepHours);

            // 4. 朝・昼・夕の量：4種以外は「軽め」。野菜の有無は「抜き」なら偽に強制（MealEntryコンストラクタで担保）。
            corrected.Breakfast = NormalizeMeal(raw.Breakfast);
            corrected.Lunch = NormalizeMeal(raw.Lunch);
            corrected.Dinner = NormalizeMeal(raw.Dinner);

            // 5. おやつ回数：0〜3の整数。範囲外・非数は0。
            corrected.Snacks = (raw.Snacks < 0 || raw.Snacks > GameConstants.SnackMax) ? 0 : raw.Snacks;

            // 6. 仮想日番号の一致確認（二重適用の防止）。
            if (raw.DayNo != currentDayNo)
            {
                return NormalizationResult.Reject("この日はすでに終わっています", corrected);
            }

            var log = new DailyLog
            {
                ExerciseMinutes = corrected.ExerciseMinutes,
                Intensity = corrected.Intensity,
                SleepHours = corrected.SleepHours,
                Breakfast = corrected.Breakfast,
                Lunch = corrected.Lunch,
                Dinner = corrected.Dinner,
                Snacks = corrected.Snacks,
                DayNo = corrected.DayNo
            };

            return NormalizationResult.Ok(log, corrected);
        }

        private static int NormalizeExerciseMinutes(int minutes)
        {
            if (minutes < GameConstants.ExerciseMinMinutes || minutes > GameConstants.ExerciseMaxMinutes)
            {
                return 0;
            }
            return RoundToStep(minutes, GameConstants.ExerciseStepMinutes);
        }

        private static float NormalizeSleepHours(float hours)
        {
            if (float.IsNaN(hours) || hours < GameConstants.SleepMinHours || hours > GameConstants.SleepMaxHours)
            {
                return GameConstants.DefaultSleepHours;
            }
            return RoundToStep(hours, GameConstants.SleepStepHours);
        }

        private static MealEntry NormalizeMeal(MealEntry entry)
        {
            var amount = IsValidMealAmount(entry.Amount) ? entry.Amount : MealAmount.Light;
            return new MealEntry(amount, entry.HasVegetable);
        }

        private static bool IsValidIntensity(Intensity intensity) =>
            intensity == Intensity.Light || intensity == Intensity.Normal || intensity == Intensity.Intense;

        private static bool IsValidMealAmount(MealAmount amount) =>
            amount == MealAmount.Skip || amount == MealAmount.Light || amount == MealAmount.Full || amount == MealAmount.Overeat;

        /// <summary>最も近い刻みへ丸める。同距離は切り上げ（requirements.md 4.1節 手順1・3）。</summary>
        private static int RoundToStep(int value, int step)
        {
            int remainder = value % step;
            if (remainder == 0) return value;
            int lower = value - remainder;
            int upper = lower + step;
            // 同距離（remainder == step/2）は切り上げ。
            return (remainder * 2 >= step) ? upper : lower;
        }

        private static float RoundToStep(float value, float step)
        {
            float steps = value / step;
            float lower = (float)System.Math.Floor(steps);
            float remainder = steps - lower;
            float rounded = (remainder >= 0.5f) ? lower + 1f : lower;
            return rounded * step;
        }
    }
}
