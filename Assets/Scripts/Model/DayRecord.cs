using System;

namespace SukoyakaBuddy.Model
{
    /// <summary>直近7日のふりかえり用の1日分の記録（requirements.md 5章）。</summary>
    [Serializable]
    public struct DayRecord
    {
        public int DayNo;
        public int Exercise;
        public int Sleep;
        public int Meal;

        public DayRecord(int dayNo, int exercise, int sleep, int meal)
        {
            DayNo = dayNo;
            Exercise = exercise;
            Sleep = sleep;
            Meal = meal;
        }
    }
}
