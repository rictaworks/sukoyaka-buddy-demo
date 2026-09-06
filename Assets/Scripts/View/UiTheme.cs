using System;
using UnityEngine;

namespace SukoyakaBuddy.View
{
    /// <summary>
    /// 実行時に一度だけ生成するフォント・スプライトを保持するインスタンス。
    /// GameBootstrapが1つだけ生成し、以降のView構築にすべて引数として渡す（グローバルな可変状態＝static変数を避けるため）。
    /// </summary>
    public class UiTheme
    {
        private const string FontResourcePath = "Fonts/NotoSansJP-VF";
        private const int SpriteTextureSize = 64;
        private const float RoundedCornerRadius = 16f;
        private const float RingThicknessRatio = 0.22f;

        public Font JapaneseFont { get; }
        public Sprite CircleSprite { get; }
        public Sprite RoundedRectSprite { get; }
        public Sprite RingSprite { get; }

        public UiTheme()
        {
            JapaneseFont = Resources.Load<Font>(FontResourcePath);
            if (JapaneseFont == null)
            {
                // unity-ugui-runtime-uiスキル 不変条件7：CJK対応フォントが無い場合は組み込みフォントで代替せず停止する。
                throw new InvalidOperationException(
                    $"日本語フォントが見つかりません。Resources/{FontResourcePath}.ttf を配置してください。");
            }

            CircleSprite = CreateCircleSprite();
            RoundedRectSprite = CreateRoundedRectSprite();
            RingSprite = CreateRingSprite();
        }

        private static Sprite CreateCircleSprite()
        {
            int size = SpriteTextureSize;
            float radius = size / 2f;
            var center = new Vector2(radius, radius);
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    float alpha = Mathf.Clamp01(radius - dist);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            return BuildSprite(pixels, size, "GeneratedCircle", Vector4.zero);
        }

        private static Sprite CreateRingSprite()
        {
            int size = SpriteTextureSize;
            float radius = size / 2f;
            float innerRadius = radius * (1f - RingThicknessRatio);
            var center = new Vector2(radius, radius);
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    float outerAlpha = Mathf.Clamp01(radius - dist);
                    float innerAlpha = Mathf.Clamp01(dist - innerRadius);
                    float alpha = Mathf.Min(outerAlpha, innerAlpha);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            return BuildSprite(pixels, size, "GeneratedRing", Vector4.zero);
        }

        private static Sprite CreateRoundedRectSprite()
        {
            int size = SpriteTextureSize;
            float r = RoundedCornerRadius;
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;
                    float cx = Mathf.Clamp(px, r, size - r);
                    float cy = Mathf.Clamp(py, r, size - r);
                    float dist = Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy));
                    float alpha = Mathf.Clamp01(r - dist + 1f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            var border = new Vector4(r, r, r, r);
            return BuildSprite(pixels, size, "GeneratedRoundedRect", border);
        }

        private static Sprite BuildSprite(Color32[] pixels, int size, string textureName, Vector4 border)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = textureName,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.SetPixels32(pixels);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size,
                0,
                SpriteMeshType.FullRect,
                border);
        }
    }
}
