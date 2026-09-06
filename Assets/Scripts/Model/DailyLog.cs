using System;

namespace SukoyakaBuddy.Model
{
    /// <summary>InputNormalizerが正規化した後の日次記録（requirements.md 4.1節）。</summary>
    [Serializable]
    public class DailyLog
    {
        public int ExerciseMinutes;
        public Intensity Intensity;
        public float SleepHours;
        public MealEntry Breakfast;
        public MealEntry Lunch;
        public MealEntry Dinner;
        public int Snacks;
        public int DayNo;
    }

    /// <summary>InputNormalizer.Normalize の結果。受付不可の場合はRejectReasonに理由が入る。</summary>
    public readonly struct NormalizationResult
    {
        public readonly bool Accepted;
        public readonly DailyLog Log;
        public readonly string RejectReason;
        public readonly DailyInput CorrectedInput;

        private NormalizationResult(bool accepted, DailyLog log, string rejectReason, DailyInput correctedInput)
        {
            Accepted = accepted;
            Log = log;
            RejectReason = rejectReason;
            CorrectedInput = correctedInput;
        }

        public static NormalizationResult Ok(DailyLog log, DailyInput correctedInput) =>
            new NormalizationResult(true, log, null, correctedInput);

        public static NormalizationResult Reject(string reason, DailyInput correctedInput) =>
            new NormalizationResult(false, null, reason, correctedInput);
    }
}
