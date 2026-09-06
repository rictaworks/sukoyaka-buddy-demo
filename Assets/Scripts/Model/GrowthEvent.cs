using System;
using System.Collections.Generic;

namespace SukoyakaBuddy.Model
{
    /// <summary>成長適用で発生したイベント（requirements.md 4.3節）。</summary>
    [Serializable]
    public struct GrowthEvent
    {
        public EventType Type;
        public Stage Stage;
        public GrowthType GrowthType;

        public static GrowthEvent StageUp(Stage stage) =>
            new GrowthEvent { Type = EventType.StageUp, Stage = stage };

        public static GrowthEvent GrowthTypeDecided(GrowthType growthType) =>
            new GrowthEvent { Type = EventType.GrowthTypeDecided, GrowthType = growthType };

        public static GrowthEvent SickStart() => new GrowthEvent { Type = EventType.SickStart };

        public static GrowthEvent SickRecover() => new GrowthEvent { Type = EventType.SickRecover };
    }

    /// <summary>GrowthEngine.Applyの出力（requirements.md 9章 クラス図）。</summary>
    [Serializable]
    public class GrowthResult
    {
        public Character After;
        public int GainedExp;
        public List<GrowthEvent> Events = new List<GrowthEvent>();
        public bool IsRestDay;
    }

    /// <summary>MoodJudge.Judgeの出力（requirements.md 4.4節）。</summary>
    [Serializable]
    public struct MoodResult
    {
        public Mood Mood;
        public string Line;
    }
}
