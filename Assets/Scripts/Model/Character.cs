using System;
using System.Collections.Generic;

namespace SukoyakaBuddy.Model
{
    /// <summary>キャラクター本体。段階・きぶんは派生値であり保存しない（requirements.md 5章）。</summary>
    [Serializable]
    public class Character
    {
        public int DayNo;
        public int Exp;
        public int Power;
        public int Energy;
        public int Body;
        public int Fatigue;
        public bool IsSick;
        public int Streak;
        public GrowthType GrowthType;
        public List<DayRecord> History = new List<DayRecord>();

        public static Character NewEgg()
        {
            return new Character
            {
                DayNo = 1,
                Exp = 0,
                Power = GameConstants.InitialStat,
                Energy = GameConstants.InitialStat,
                Body = GameConstants.InitialStat,
                Fatigue = 0,
                IsSick = false,
                Streak = 0,
                GrowthType = GrowthType.Undecided,
                History = new List<DayRecord>()
            };
        }

        /// <summary>段階は経験値のみから求める派生値（requirements.md 4.3節 手順7）。</summary>
        public Stage GetStage() => StageFromExp(Exp);

        public static Stage StageFromExp(int exp)
        {
            if (exp >= GameConstants.MasterExpThreshold) return Stage.Master;
            if (exp >= GameConstants.AdultExpThreshold) return Stage.Adult;
            if (exp >= GameConstants.ChildExpThreshold) return Stage.Child;
            return Stage.Egg;
        }

        /// <summary>次の段階までの残り経験値。たつじんは上限のため0を返す。</summary>
        public int ExpToNextStage()
        {
            switch (GetStage())
            {
                case Stage.Egg: return GameConstants.ChildExpThreshold - Exp;
                case Stage.Child: return GameConstants.AdultExpThreshold - Exp;
                case Stage.Adult: return GameConstants.MasterExpThreshold - Exp;
                default: return 0;
            }
        }

        public Character Clone()
        {
            return new Character
            {
                DayNo = DayNo,
                Exp = Exp,
                Power = Power,
                Energy = Energy,
                Body = Body,
                Fatigue = Fatigue,
                IsSick = IsSick,
                Streak = Streak,
                GrowthType = GrowthType,
                History = new List<DayRecord>(History)
            };
        }
    }
}
