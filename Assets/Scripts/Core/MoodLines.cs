using SukoyakaBuddy.Model;

namespace SukoyakaBuddy.Core
{
    /// <summary>きぶん×最も低いスコアの項目ごとの吹き出し文言（requirements.md 3.3節・4.4節）。
    /// バディの台詞はキャラクター性のためくだけた口調のまま実装する（一般コンテンツのですます調は適用しない）。</summary>
    public static class MoodLines
    {
        public static string Get(Mood mood, ScoreCategory category)
        {
            return mood switch
            {
                Mood.Genki => category switch
                {
                    ScoreCategory.Exercise => "いっぱい動いて気持ちいい！",
                    ScoreCategory.Sleep => "ぐっすり眠れて絶好調！",
                    _ => "きょうもいいかんじ！"
                },
                Mood.Normal => category switch
                {
                    ScoreCategory.Exercise => "今日はほどほどに動いたよ",
                    ScoreCategory.Sleep => "まあまあ眠れたかな",
                    _ => "まあまあかな"
                },
                Mood.Tired => category switch
                {
                    ScoreCategory.Exercise => "ちょっと動きすぎたかも…",
                    ScoreCategory.Sleep => "寝不足でちょっとつらい…",
                    _ => "ちょっとやりすぎたかも…"
                },
                _ => category switch
                {
                    ScoreCategory.Exercise => "もう動けない…",
                    ScoreCategory.Sleep => "ねむすぎる…ちゃんと休ませて…",
                    _ => "おなかの調子がよくないよ…"
                }
            };
        }
    }
}
