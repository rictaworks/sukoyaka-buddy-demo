using System;
using SukoyakaBuddy.Model;

namespace SukoyakaBuddy.Core
{
    /// <summary>関数B：日次スコア算出（requirements.md 4.2節）。</summary>
    public static class DayScorer
    {
        public static DayScore Score(DailyLog log)
        {
            var score = new DayScore();

            float intensityFactor = log.Intensity switch
            {
                Intensity.Light => GameConstants.IntensityLightFactor,
                Intensity.Intense => GameConstants.IntensityIntenseFactor,
                _ => GameConstants.IntensityNormalFactor
            };
            float exerciseAmount = log.ExerciseMinutes * intensityFactor;
            score.Exercise = RoundHalfUp(Piecewise(exerciseAmount, GameConstants.ExerciseAnchorX, GameConstants.ExerciseAnchorY));
            score.Overwork = exerciseAmount > GameConstants.OverworkThreshold;

            score.Sleep = RoundHalfUp(Piecewise(log.SleepHours, GameConstants.SleepAnchorX, GameConstants.SleepAnchorY));
            score.ShortSleep = log.SleepHours < GameConstants.ShortSleepThreshold;
            score.Oversleep = log.SleepHours > GameConstants.OversleepThreshold;

            int mealPoints = MealPoint(log.Breakfast) + MealPoint(log.Lunch) + MealPoint(log.Dinner);
            int snackPenalty = log.Snacks > 1 ? (log.Snacks - 1) * GameConstants.SnackPenaltyPerExtra : 0;
            score.Meal = Clamp0To100(mealPoints - snackPenalty);

            score.SkippedMeal = IsSkip(log.Breakfast) || IsSkip(log.Lunch) || IsSkip(log.Dinner);
            score.Overeat = IsOvereat(log.Breakfast) || IsOvereat(log.Lunch) || IsOvereat(log.Dinner)
                             || log.Snacks == GameConstants.SnackMax;
            score.VegetableRich = !IsSkip(log.Breakfast) && !IsSkip(log.Lunch) && !IsSkip(log.Dinner)
                                   && log.Breakfast.HasVegetable && log.Lunch.HasVegetable && log.Dinner.HasVegetable;

            return score;
        }

        private static int MealPoint(MealEntry meal)
        {
            int basePoint = meal.Amount switch
            {
                MealAmount.Skip => GameConstants.MealSkipPoint,
                MealAmount.Light => GameConstants.MealLightPoint,
                MealAmount.Full => GameConstants.MealFullPoint,
                MealAmount.Overeat => GameConstants.MealOvereatPoint,
                _ => GameConstants.MealSkipPoint
            };
            bool vegetableBonus = meal.Amount != MealAmount.Skip && meal.HasVegetable;
            return basePoint + (vegetableBonus ? GameConstants.VegetableBonus : 0);
        }

        private static bool IsSkip(MealEntry meal) => meal.Amount == MealAmount.Skip;
        private static bool IsOvereat(MealEntry meal) => meal.Amount == MealAmount.Overeat;

        /// <summary>区間ごとの直線補間。範囲外は端の値でクランプする（requirements.md 4.2節）。</summary>
        private static float Piecewise(float x, float[] anchorX, float[] anchorY)
        {
            if (x <= anchorX[0]) return anchorY[0];
            int last = anchorX.Length - 1;
            if (x >= anchorX[last]) return anchorY[last];

            for (int i = 0; i < last; i++)
            {
                if (x >= anchorX[i] && x <= anchorX[i + 1])
                {
                    float t = (x - anchorX[i]) / (anchorX[i + 1] - anchorX[i]);
                    return anchorY[i] + t * (anchorY[i + 1] - anchorY[i]);
                }
            }
            return anchorY[last];
        }

        private static int RoundHalfUp(float value) => (int)Math.Floor(value + 0.5f);

        private static int Clamp0To100(int value) => Math.Max(0, Math.Min(100, value));
    }
}
