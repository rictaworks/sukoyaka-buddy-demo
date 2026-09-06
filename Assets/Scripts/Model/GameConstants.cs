namespace SukoyakaBuddy.Model
{
    /// <summary>デモ版の既定値一覧（requirements.md 4.9節）。文言・定数のハードコード回避のため、この1ファイルに集約する。</summary>
    public static class GameConstants
    {
        // --- 運動（関数A・B） ---
        public const int ExerciseMinMinutes = 0;
        public const int ExerciseMaxMinutes = 180;
        public const int ExerciseStepMinutes = 5;
        public const float IntensityLightFactor = 0.6f;
        public const float IntensityNormalFactor = 1.0f;
        public const float IntensityIntenseFactor = 1.4f;
        public const float OverworkThreshold = 120f;
        public static readonly float[] ExerciseAnchorX = { 0f, 30f, 60f, 120f, 240f };
        public static readonly float[] ExerciseAnchorY = { 0f, 70f, 100f, 100f, 50f };

        // --- 睡眠（関数A・B） ---
        public const float SleepMinHours = 0f;
        public const float SleepMaxHours = 14f;
        public const float SleepStepHours = 0.5f;
        public const float DefaultSleepHours = 7f;
        public const float ShortSleepThreshold = 6f;
        public const float OversleepThreshold = 10f;
        public static readonly float[] SleepAnchorX = { 0f, 4f, 6f, 7f, 7.5f, 9f, 10f, 11f, 12f };
        public static readonly float[] SleepAnchorY = { 0f, 20f, 60f, 90f, 100f, 100f, 80f, 60f, 40f };

        // --- 食事（関数A・B） ---
        public const int MealSkipPoint = 0;
        public const int MealLightPoint = 20;
        public const int MealFullPoint = 30;
        public const int MealOvereatPoint = 15;
        public const int VegetableBonus = 5;
        public const int SnackPenaltyPerExtra = 5;
        public const int SnackMax = 3;

        // --- 成長適用（関数C） ---
        public const int StatDeltaHigh = 6;    // score >= 80
        public const int StatDeltaMidHigh = 4; // 60-79
        public const int StatDeltaMid = 2;     // 40-59
        public const int StatDeltaLow = 0;     // 20-39
        public const int StatDeltaVeryLow = -3; // <20
        public const int PowerDeltaVeryLow = -2; // ちからの20未満は-2
        public const int OversleepEnergyDeltaCap = 2;
        public const int OverworkEnergyPenalty = -2;
        public const int OvereatBodyPenalty = -2;
        public const int VegetableRichBodyBonus = 1;
        public const int BalanceDayScoreThreshold = 60;
        public const int StreakBonusThreshold = 3;
        public const float StreakBonusMultiplier = 1.5f;
        public const int BalancedTypeGapThreshold = 5;
        public const int RestDayFatigueThreshold = 40;
        public const int RestDaySleepScoreThreshold = 60;
        public const int FatigueBaseRecover = -5;
        public const int FatigueOverworkPenalty = 15;
        public const int FatigueShortSleepPenalty = 10;
        public const int FatigueSkippedMealPenalty = 5;
        public const int FatigueGreatSleepBonus = -15;
        public const int FatigueRestDayBonus = -15;
        public const int GreatSleepScoreThreshold = 90;
        public const int SickStartThreshold = 80;
        public const int SickRecoverThreshold = 50;

        // --- 段階・経験値 ---
        public const int ChildExpThreshold = 100;
        public const int AdultExpThreshold = 300;
        public const int MasterExpThreshold = 600;
        public const int MaxExp = 9999;

        // --- 初期値・履歴 ---
        public const int InitialStat = 10;
        public const int HistoryDays = 7;

        // --- 日付境界（関数H） ---
        public const int JstOffsetHours = 9;
        public const int BusinessDayOffsetHours = 3;

        // --- 保存（関数G） ---
        public const int SchemaVersion = 1;

        // --- 画面基準解像度（3章） ---
        public const float ReferenceWidth = 1280f;
        public const float ReferenceHeight = 720f;
    }
}
