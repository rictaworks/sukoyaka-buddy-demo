namespace SukoyakaBuddy.Model
{
    /// <summary>キャラクターの段階（経験値から求める派生値）。requirements.md 4.9節: こども100/おとな300/たつじん600。</summary>
    public enum Stage
    {
        Egg,
        Child,
        Adult,
        Master
    }

    /// <summary>成長タイプ。おとなに達した日のステータスで一度だけ確定する（requirements.md 4.3節 手順8）。</summary>
    public enum GrowthType
    {
        Undecided,
        Athlete,
        Laidback,
        Gourmet,
        Balanced
    }

    /// <summary>その日のきぶん（requirements.md 4.4節）。</summary>
    public enum Mood
    {
        Genki,
        Normal,
        Tired,
        Exhausted
    }

    /// <summary>運動の強さ（requirements.md 4.1節）。</summary>
    public enum Intensity
    {
        Light,
        Normal,
        Intense
    }

    /// <summary>食事の量（requirements.md 4.1節）。</summary>
    public enum MealAmount
    {
        Skip,
        Light,
        Full,
        Overeat
    }

    /// <summary>スコア算出対象の項目。MoodJudgeの吹き出し選定・reviewでの内訳表示に使う。</summary>
    public enum ScoreCategory
    {
        Exercise,
        Sleep,
        Meal
    }

    /// <summary>成長適用で発生しうるイベント（requirements.md 4.3節）。</summary>
    public enum EventType
    {
        StageUp,
        GrowthTypeDecided,
        SickStart,
        SickRecover
    }
}
