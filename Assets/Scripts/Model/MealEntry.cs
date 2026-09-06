using System;

namespace SukoyakaBuddy.Model
{
    /// <summary>1食分の記録。requirements.md 4.1節 手順4：量が「抜き」のとき野菜の有無は偽に強制する。</summary>
    [Serializable]
    public struct MealEntry
    {
        public MealAmount Amount;
        public bool HasVegetable;

        public MealEntry(MealAmount amount, bool hasVegetable)
        {
            Amount = amount;
            // 食べていない食事に野菜は付かない（requirements.md 4.1節 手順4）。
            HasVegetable = amount == MealAmount.Skip ? false : hasVegetable;
        }
    }
}
