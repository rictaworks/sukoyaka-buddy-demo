using SukoyakaBuddy.Core;
using SukoyakaBuddy.Model;

namespace SukoyakaBuddy.View
{
    /// <summary>
    /// Model/Coreの列挙値に対応する日本語表示名。Core/MoodLines.csと同様、表示文言を1箇所に集約する。
    /// StatusPanel・BuddyView・HistoryChart・GameControllerが共通で参照する。
    /// </summary>
    public static class DisplayNames
    {
        public static string StageLabel(Stage stage) => stage switch
        {
            Stage.Egg => "たまご",
            Stage.Child => "こども",
            Stage.Adult => "おとな",
            Stage.Master => "たつじん",
            _ => stage.ToString()
        };

        public static string GrowthTypeLabel(GrowthType growthType) => growthType switch
        {
            GrowthType.Undecided => "未確定",
            GrowthType.Athlete => "アスリート型",
            GrowthType.Laidback => "のんびり型",
            GrowthType.Gourmet => "グルメ型",
            GrowthType.Balanced => "バランス型",
            _ => growthType.ToString()
        };

        public static string ScoreCategoryLabel(ScoreCategory category) => category switch
        {
            ScoreCategory.Exercise => "運動",
            ScoreCategory.Sleep => "睡眠",
            ScoreCategory.Meal => "食事",
            _ => category.ToString()
        };

        public static string IntensityLabel(Intensity intensity) => intensity switch
        {
            Intensity.Light => "軽い",
            Intensity.Normal => "ふつう",
            Intensity.Intense => "激しい",
            _ => intensity.ToString()
        };

        public static string MealAmountLabel(MealAmount amount) => amount switch
        {
            MealAmount.Skip => "抜き",
            MealAmount.Light => "軽め",
            MealAmount.Full => "しっかり",
            MealAmount.Overeat => "食べすぎ",
            _ => amount.ToString()
        };

        public static string PresetLabel(Preset preset) => PresetCatalog.DisplayName(preset);
    }
}
