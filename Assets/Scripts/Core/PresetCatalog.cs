using System;
using SukoyakaBuddy.Model;

namespace SukoyakaBuddy.Core
{
    /// <summary>展示用のプリセット4種（requirements.md 4.6節）。</summary>
    public enum Preset
    {
        Genki,
        Darake,
        Ganbarisugi,
        ChottoFusoku
    }

    public static class PresetCatalog
    {
        public static DailyInput Get(Preset preset, int dayNo)
        {
            switch (preset)
            {
                case Preset.Genki:
                    return new DailyInput
                    {
                        ExerciseMinutes = 45,
                        Intensity = Intensity.Normal,
                        SleepHours = 8f,
                        Breakfast = new MealEntry(MealAmount.Full, true),
                        Lunch = new MealEntry(MealAmount.Full, true),
                        Dinner = new MealEntry(MealAmount.Full, true),
                        Snacks = 0,
                        DayNo = dayNo
                    };
                case Preset.Darake:
                    return new DailyInput
                    {
                        ExerciseMinutes = 0,
                        Intensity = Intensity.Normal,
                        SleepHours = 11f,
                        Breakfast = new MealEntry(MealAmount.Skip, false),
                        Lunch = new MealEntry(MealAmount.Overeat, false),
                        Dinner = new MealEntry(MealAmount.Overeat, false),
                        Snacks = 3,
                        DayNo = dayNo
                    };
                case Preset.Ganbarisugi:
                    return new DailyInput
                    {
                        ExerciseMinutes = 120,
                        Intensity = Intensity.Intense,
                        SleepHours = 5f,
                        Breakfast = new MealEntry(MealAmount.Light, false),
                        Lunch = new MealEntry(MealAmount.Light, false),
                        Dinner = new MealEntry(MealAmount.Light, false),
                        Snacks = 0,
                        DayNo = dayNo
                    };
                case Preset.ChottoFusoku:
                    return new DailyInput
                    {
                        ExerciseMinutes = 15,
                        Intensity = Intensity.Light,
                        SleepHours = 6f,
                        Breakfast = new MealEntry(MealAmount.Light, false),
                        Lunch = new MealEntry(MealAmount.Full, true),
                        Dinner = new MealEntry(MealAmount.Light, false),
                        Snacks = 1,
                        DayNo = dayNo
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(preset), preset, null);
            }
        }

        public static string DisplayName(Preset preset) => preset switch
        {
            Preset.Genki => "元気な一日",
            Preset.Darake => "だらけた一日",
            Preset.Ganbarisugi => "がんばりすぎ",
            Preset.ChottoFusoku => "ちょっと不足",
            _ => preset.ToString()
        };
    }
}
