using UnityEngine;

namespace SukoyakaBuddy.View
{
    /// <summary>
    /// UI全体で使う色・寸法の定数一覧。文言・数値をハードコードせず1箇所に集約する（CLAUDE.md コーディング規約）。
    /// Colorはconstにできないためstatic readonlyで宣言する（GameConstantsと同じ不変定数のパターン）。
    /// 前景・背景の組み合わせはいずれもコントラスト比4.5以上を満たすよう選定済み（unity-ugui-runtime-uiスキル 不変条件10）。
    /// </summary>
    public static class UiPalette
    {
        // --- 背景・パネル ---
        public static readonly Color Background = new Color(0.937f, 0.918f, 0.878f);
        public static readonly Color Panel = new Color(1f, 1f, 1f);
        public static readonly Color PanelAlt = new Color(0.969f, 0.957f, 0.933f);
        public static readonly Color TrackColor = new Color(0.894f, 0.875f, 0.827f);

        // --- 文字 ---
        public static readonly Color TextPrimary = new Color(0.180f, 0.169f, 0.149f);
        public static readonly Color TextSecondary = new Color(0.420f, 0.396f, 0.361f);
        public static readonly Color TextOnAccent = Color.white;

        // --- アクセント（ボタン等。白文字コントラスト比4.5以上） ---
        public static readonly Color AccentPrimary = new Color(0.122f, 0.478f, 0.431f); // 「1日を終える」・げんきゲージ
        public static readonly Color AccentDanger = new Color(0.698f, 0.227f, 0.180f);  // 「はじめから」・つかれゲージ
        public static readonly Color AccentDisabled = new Color(0.788f, 0.769f, 0.729f);

        // --- 選択状態（記録フォームの選択肢） ---
        public static readonly Color SelectedColor = AccentPrimary;
        public static readonly Color UnselectedColor = PanelAlt;

        // --- ステータスゲージ ---
        public static readonly Color PowerColor = new Color(0.851f, 0.482f, 0.161f);  // ちから
        public static readonly Color EnergyColor = AccentPrimary;                      // げんき
        public static readonly Color BodyColor = new Color(0.545f, 0.373f, 0.749f);    // からだ
        public static readonly Color FatigueColor = AccentDanger;                      // つかれ

        // --- バディ表現 ---
        public static readonly Color EggColor = new Color(0.941f, 0.875f, 0.627f);
        public static readonly Color BuddyBodyColor = new Color(0.624f, 0.847f, 0.776f);
        public static readonly Color AthleteColor = new Color(0.867f, 0.475f, 0.322f);
        public static readonly Color LaidbackColor = new Color(0.702f, 0.792f, 0.898f);
        public static readonly Color GourmetColor = new Color(0.937f, 0.706f, 0.463f);
        public static readonly Color BalancedColor = new Color(0.663f, 0.816f, 0.667f);
        public static readonly Color FeatureColor = TextPrimary; // 目・口
        public static readonly Color SweatColor = new Color(0.549f, 0.788f, 0.918f);
        public static readonly Color HaloColor = new Color(0.957f, 0.773f, 0.259f);
        public static readonly Color LeafColor = new Color(0.298f, 0.604f, 0.298f);
        public static readonly Color CrackColor = new Color(0.470f, 0.400f, 0.220f);

        // --- グラフ ---
        public static readonly Color ExerciseColor = PowerColor;
        public static readonly Color SleepColor = EnergyColor;
        public static readonly Color MealColor = BodyColor;

        // --- 通知 ---
        public static readonly Color MessageColor = AccentDanger;

        // --- 寸法（参照解像度1280x720基準） ---
        public const float SpacingXSmall = 6f;
        public const float SpacingSmall = 10f;
        public const float Spacing = 16f;
        public const float SpacingLarge = 24f;
        public const float PanelPadding = 16f;

        public const float MinTapSize = 44f;   // スキル不変条件10：タップ領域44px以上
        public const float ButtonHeight = 48f;
        public const float ChipHeight = 44f;
        public const float SliderHeight = 44f;
        public const float HandleSize = 26f;
        public const float ToggleSize = 32f;
        public const float GaugeBarHeight = 16f;

        public const int FontSizeSmall = 14;   // スキル不変条件10：本文最小14px
        public const int FontSizeLabel = 15;
        public const int FontSizeBody = 16;
        public const int FontSizeHeading = 19;
        public const int FontSizeTitle = 23;
        public const int FontSizeBubble = 18;
        public const int FontSizeButton = 17;
    }
}
