using SukoyakaBuddy.Model;

namespace SukoyakaBuddy.Core
{
    /// <summary>関数D：きぶんの判定（requirements.md 4.4節）。</summary>
    public static class MoodJudge
    {
        public static MoodResult Judge(DayScore score, Character characterAfter)
        {
            Mood mood;
            if (characterAfter.IsSick)
            {
                mood = Mood.Exhausted;
            }
            else if (score.Min() < 30)
            {
                mood = Mood.Exhausted;
            }
            else if (score.Overwork || score.ShortSleep)
            {
                mood = Mood.Tired;
            }
            else if (score.Min() >= 60)
            {
                mood = Mood.Genki;
            }
            else
            {
                mood = Mood.Normal;
            }

            var category = score.LowestCategory();
            return new MoodResult { Mood = mood, Line = MoodLines.Get(mood, category) };
        }
    }
}
