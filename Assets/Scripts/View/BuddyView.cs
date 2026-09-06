using System.Collections;
using SukoyakaBuddy.Model;
using UnityEngine;
using UnityEngine.UI;

namespace SukoyakaBuddy.View
{
    /// <summary>
    /// バディ本体の表示（requirements.md 3.3節）。画像アセットを使わず基本図形（円・多角形の組み合わせ）で
    /// 段階・成長タイプの体型ときぶんの表情・動きを表現する。演出はいずれも3秒以内で終える。
    /// </summary>
    public class BuddyView : MonoBehaviour
    {
        public const float ReactionAnimationSeconds = 1.2f;
        public const float EvolutionAnimationSeconds = 3f;
        public const float SickAnimationSeconds = 1.5f;
        public const float RecoverAnimationSeconds = 1.5f;

        private const float BaseSize = 220f;
        private const float BounceSpeed = 6f;
        private const float BounceHeight = 14f;
        private const float SwaySpeed = 2f;
        private const float SwayAngle = 6f;
        private const float TremorSpeed = 10f;
        private const float TremorAngle = 3f;
        private const float CollapsedAngle = 68f;

        private enum MotionMode { Bounce, SwayGentle, TremorSmall, Collapsed }

        private UiTheme _theme;
        private RectTransform _bodyContainer;
        private RectTransform _shapeRoot;
        private RectTransform _faceRoot;
        private Text _bubbleText;

        private Stage _stage = Stage.Egg;
        private GrowthType _growthType = GrowthType.Undecided;
        private Mood _lastMood = Mood.Normal;
        private MotionMode _motionMode = MotionMode.SwayGentle;
        private float _eggProgress;
        private Vector2 _basePosition;
        private Coroutine _transitionCoroutine;
        private bool _isTransitioning;

        public Stage CurrentStage => _stage;
        public GrowthType CurrentGrowthType => _growthType;

        public void Initialize(RectTransform parent, UiTheme theme)
        {
            _theme = theme;

            var root = UiFactory.CreateRect(parent, "BuddyView");
            UiFactory.StretchFull(root);

            BuildSpeechBubble(root);

            _bodyContainer = UiFactory.CreateRect(root, "BodyContainer");
            _bodyContainer.anchorMin = _bodyContainer.anchorMax = new Vector2(0.5f, 0.38f);
            _bodyContainer.sizeDelta = new Vector2(BaseSize * 1.3f, BaseSize * 1.3f);
            _bodyContainer.anchoredPosition = Vector2.zero;
            _basePosition = _bodyContainer.anchoredPosition;

            _shapeRoot = UiFactory.CreateRect(_bodyContainer, "Shape");
            UiFactory.StretchFull(_shapeRoot);
            _faceRoot = UiFactory.CreateRect(_bodyContainer, "Face");
            UiFactory.StretchFull(_faceRoot);

            BuildShape();
        }

        /// <summary>キャラクターの現在の姿を反映する（きぶん・吹き出しは変更しない）。段階・成長タイプが変わったときのみ再構築する。</summary>
        public void Render(Character character)
        {
            var stage = character.GetStage();
            bool changed = stage != _stage || character.GrowthType != _growthType;
            _stage = stage;
            _growthType = character.GrowthType;
            _eggProgress = _stage == Stage.Egg
                ? Mathf.Clamp01(character.Exp / (float)GameConstants.ChildExpThreshold)
                : 1f;

            if (changed || _stage == Stage.Egg)
            {
                BuildShape();
            }
        }

        public void SetMood(Mood mood, string line)
        {
            _lastMood = mood;
            _bubbleText.text = line;
            _motionMode = MotionModeFor(mood);
            RebuildFace(mood);
        }

        /// <summary>拒否・保存失敗など、きぶんを変えずに吹き出しだけで理由を示す（requirements.md 12.4節）。</summary>
        public void ShowMessage(string text)
        {
            _bubbleText.text = text;
        }

        public void PlayEvolution(Stage stage, GrowthType growthType)
        {
            _stage = stage;
            _growthType = growthType;
            _eggProgress = 1f;
            BuildShape();
            RestartTransition(PulseRoutine(EvolutionAnimationSeconds, 1.35f));
        }

        public void PlaySick()
        {
            _motionMode = MotionMode.Collapsed;
            RestartTransition(ShakeRoutine(SickAnimationSeconds));
        }

        public void PlayRecover()
        {
            RestartTransition(PulseRoutine(RecoverAnimationSeconds, 1.15f));
        }

        private void Update()
        {
            if (_bodyContainer == null || _isTransitioning) return;

            float t = Time.time;
            switch (_motionMode)
            {
                case MotionMode.Bounce:
                    _bodyContainer.anchoredPosition = _basePosition + Vector2.up * (Mathf.Abs(Mathf.Sin(t * BounceSpeed)) * BounceHeight);
                    _bodyContainer.localRotation = Quaternion.identity;
                    break;
                case MotionMode.SwayGentle:
                    _bodyContainer.anchoredPosition = _basePosition;
                    _bodyContainer.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * SwaySpeed) * SwayAngle);
                    break;
                case MotionMode.TremorSmall:
                    _bodyContainer.anchoredPosition = _basePosition;
                    _bodyContainer.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * TremorSpeed) * TremorAngle);
                    break;
                case MotionMode.Collapsed:
                    _bodyContainer.anchoredPosition = _basePosition;
                    _bodyContainer.localRotation = Quaternion.Euler(0f, 0f, CollapsedAngle);
                    break;
            }
        }

        private static MotionMode MotionModeFor(Mood mood) => mood switch
        {
            Mood.Genki => MotionMode.Bounce,
            Mood.Normal => MotionMode.SwayGentle,
            Mood.Tired => MotionMode.TremorSmall,
            Mood.Exhausted => MotionMode.Collapsed,
            _ => MotionMode.SwayGentle
        };

        private void RestartTransition(IEnumerator routine)
        {
            if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = StartCoroutine(RunTransition(routine));
        }

        private IEnumerator RunTransition(IEnumerator routine)
        {
            _isTransitioning = true;
            yield return StartCoroutine(routine);
            _isTransitioning = false;
        }

        private IEnumerator PulseRoutine(float duration, float peakScale)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float phase = Mathf.Clamp01(t / duration);
                float scale = 1f + (peakScale - 1f) * Mathf.Sin(phase * Mathf.PI);
                _bodyContainer.localScale = Vector3.one * scale;
                yield return null;
            }
            _bodyContainer.localScale = Vector3.one;
        }

        private IEnumerator ShakeRoutine(float duration)
        {
            const float frequency = 18f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float damp = 1f - Mathf.Clamp01(t / duration);
                float angle = Mathf.Sin(t * frequency) * 12f * damp;
                _bodyContainer.localRotation = Quaternion.Euler(0f, 0f, angle);
                yield return null;
            }
            _bodyContainer.localRotation = Quaternion.Euler(0f, 0f, CollapsedAngle);
        }

        private void BuildSpeechBubble(Transform parent)
        {
            var bubble = UiFactory.CreatePanel(parent, "SpeechBubble", _theme.RoundedRectSprite, UiPalette.Panel);
            var rect = bubble.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(BaseSize * 1.7f, 84f);
            rect.anchoredPosition = new Vector2(0f, -UiPalette.Spacing);

            var text = UiFactory.CreateText(bubble.transform, "Text", _theme, string.Empty,
                UiPalette.FontSizeBubble, UiPalette.TextPrimary, TextAnchor.MiddleCenter);
            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(UiPalette.Spacing, UiPalette.SpacingSmall);
            textRect.offsetMax = new Vector2(-UiPalette.Spacing, -UiPalette.SpacingSmall);
            _bubbleText = text;
        }

        private void BuildShape()
        {
            DestroyChildren(_shapeRoot);
            float scale = StageScale(_stage);

            switch (_stage)
            {
                case Stage.Egg:
                    BuildEgg(scale);
                    break;
                case Stage.Child:
                    BuildChild(scale);
                    break;
                case Stage.Adult:
                    BuildAdult(scale, _growthType);
                    break;
                case Stage.Master:
                    BuildHalo(scale);
                    BuildAdult(scale, _growthType);
                    break;
            }

            RebuildFace(_lastMood);
        }

        private static float StageScale(Stage stage) => stage switch
        {
            Stage.Egg => 0.6f,
            Stage.Child => 0.8f,
            Stage.Adult => 1.0f,
            Stage.Master => 1.1f,
            _ => 1f
        };

        private void BuildEgg(float scale)
        {
            var body = UiFactory.CreateShape(_shapeRoot, "EggBody", _theme.CircleSprite, UiPalette.EggColor,
                new Vector2(BaseSize * scale * 0.85f, BaseSize * scale * 1.05f));
            CenterAt(body.rectTransform, Vector2.zero);

            int crackCount = Mathf.Clamp(Mathf.FloorToInt(_eggProgress * 4f), 0, 4);
            for (int i = 0; i < crackCount; i++)
            {
                var crack = UiFactory.CreateShape(_shapeRoot, $"Crack{i}", null, UiPalette.CrackColor,
                    new Vector2(BaseSize * scale * 0.45f, 3f));
                CenterAt(crack.rectTransform, new Vector2(0f, (i - 1.5f) * BaseSize * scale * 0.12f));
                crack.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -35f + i * 22f);
            }
        }

        private void BuildChild(float scale)
        {
            var body = UiFactory.CreateShape(_shapeRoot, "ChildBody", _theme.CircleSprite, UiPalette.BuddyBodyColor,
                Vector2.one * BaseSize * scale);
            CenterAt(body.rectTransform, Vector2.zero);

            float limbSize = BaseSize * scale * 0.22f;
            float handOffsetX = BaseSize * scale * 0.55f;
            AddLimb("HandLeft", -handOffsetX, 0f, limbSize, UiPalette.BuddyBodyColor);
            AddLimb("HandRight", handOffsetX, 0f, limbSize, UiPalette.BuddyBodyColor);
            AddLimb("FootLeft", -handOffsetX * 0.5f, -BaseSize * scale * 0.52f, limbSize, UiPalette.BuddyBodyColor);
            AddLimb("FootRight", handOffsetX * 0.5f, -BaseSize * scale * 0.52f, limbSize, UiPalette.BuddyBodyColor);
        }

        private void BuildAdult(float scale, GrowthType growthType)
        {
            switch (growthType)
            {
                case GrowthType.Athlete:
                    BuildAthlete(scale);
                    break;
                case GrowthType.Laidback:
                    BuildLaidback(scale);
                    break;
                case GrowthType.Gourmet:
                    BuildGourmet(scale);
                    break;
                case GrowthType.Balanced:
                    BuildBalanced(scale);
                    break;
                default:
                    // 成長タイプ確定前におとな以上へ到達することは仕様上発生しないが、防御的に既定の体型を表示する。
                    BuildGenericAdult(scale);
                    break;
            }
        }

        private void BuildAthlete(float scale)
        {
            var body = UiFactory.CreateShape(_shapeRoot, "AthleteBody", _theme.CircleSprite, UiPalette.AthleteColor,
                new Vector2(BaseSize * scale * 0.8f, BaseSize * scale * 1.08f));
            CenterAt(body.rectTransform, Vector2.zero);

            float limbSize = BaseSize * scale * 0.2f;
            AddLimb("ArmLeft", -BaseSize * scale * 0.48f, BaseSize * scale * 0.05f, limbSize, UiPalette.AthleteColor);
            AddLimb("ArmRight", BaseSize * scale * 0.48f, BaseSize * scale * 0.05f, limbSize, UiPalette.AthleteColor);
            AddLimb("LegLeft", -BaseSize * scale * 0.25f, -BaseSize * scale * 0.58f, limbSize, UiPalette.AthleteColor);
            AddLimb("LegRight", BaseSize * scale * 0.25f, -BaseSize * scale * 0.58f, limbSize, UiPalette.AthleteColor);
        }

        private void BuildLaidback(float scale)
        {
            var body = UiFactory.CreateShape(_shapeRoot, "LaidbackBody", _theme.CircleSprite, UiPalette.LaidbackColor,
                new Vector2(BaseSize * scale * 1.08f, BaseSize * scale * 0.9f));
            CenterAt(body.rectTransform, Vector2.zero);

            var pillow = UiFactory.CreateShape(_shapeRoot, "Pillow", _theme.RoundedRectSprite, UiPalette.Panel,
                new Vector2(BaseSize * scale * 0.5f, BaseSize * scale * 0.28f));
            CenterAt(pillow.rectTransform, new Vector2(BaseSize * scale * 0.55f, -BaseSize * scale * 0.3f));
            pillow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -15f);
        }

        private void BuildGourmet(float scale)
        {
            var body = UiFactory.CreateShape(_shapeRoot, "GourmetBody", _theme.CircleSprite, UiPalette.GourmetColor,
                Vector2.one * BaseSize * scale * 1.1f);
            CenterAt(body.rectTransform, Vector2.zero);

            var handle = UiFactory.CreateShape(_shapeRoot, "SpoonHandle", null, UiPalette.TextSecondary,
                new Vector2(6f, BaseSize * scale * 0.4f));
            CenterAt(handle.rectTransform, new Vector2(BaseSize * scale * 0.55f, BaseSize * scale * 0.18f));
            handle.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 20f);

            var bowl = UiFactory.CreateShape(_shapeRoot, "SpoonBowl", _theme.CircleSprite, UiPalette.TextSecondary,
                new Vector2(BaseSize * scale * 0.22f, BaseSize * scale * 0.16f));
            CenterAt(bowl.rectTransform, new Vector2(BaseSize * scale * 0.62f, BaseSize * scale * 0.4f));
        }

        private void BuildBalanced(float scale)
        {
            var body = UiFactory.CreateShape(_shapeRoot, "BalancedBody", _theme.CircleSprite, UiPalette.BalancedColor,
                Vector2.one * BaseSize * scale);
            CenterAt(body.rectTransform, Vector2.zero);

            var leaf = UiFactory.CreateShape(_shapeRoot, "Leaf", _theme.CircleSprite, UiPalette.LeafColor,
                new Vector2(BaseSize * scale * 0.22f, BaseSize * scale * 0.32f));
            CenterAt(leaf.rectTransform, new Vector2(0f, BaseSize * scale * 0.55f));
            leaf.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 20f);
        }

        private void BuildGenericAdult(float scale)
        {
            var body = UiFactory.CreateShape(_shapeRoot, "AdultBody", _theme.CircleSprite, UiPalette.BuddyBodyColor,
                Vector2.one * BaseSize * scale);
            CenterAt(body.rectTransform, Vector2.zero);
        }

        private void BuildHalo(float scale)
        {
            var halo = UiFactory.CreateShape(_shapeRoot, "Halo", _theme.RingSprite, UiPalette.HaloColor,
                Vector2.one * BaseSize * scale * 1.6f);
            CenterAt(halo.rectTransform, new Vector2(0f, BaseSize * scale * 0.15f));
        }

        private void AddLimb(string name, float x, float y, float size, Color color)
        {
            var limb = UiFactory.CreateShape(_shapeRoot, name, _theme.CircleSprite, color, new Vector2(size, size));
            CenterAt(limb.rectTransform, new Vector2(x, y));
        }

        private void RebuildFace(Mood mood)
        {
            if (_faceRoot == null) return;
            DestroyChildren(_faceRoot);

            float scale = StageScale(_stage);
            float eyeOffsetX = BaseSize * scale * 0.18f;
            float eyeY = BaseSize * scale * 0.12f;

            switch (mood)
            {
                case Mood.Genki:
                    AddEye(-eyeOffsetX, eyeY, scale, closed: false);
                    AddEye(eyeOffsetX, eyeY, scale, closed: false);
                    AddMouth(scale, curve: 1);
                    break;
                case Mood.Normal:
                    AddEye(-eyeOffsetX, eyeY, scale, closed: false);
                    AddEye(eyeOffsetX, eyeY, scale, closed: false);
                    AddMouth(scale, curve: 0);
                    break;
                case Mood.Tired:
                    AddEye(-eyeOffsetX, eyeY, scale, closed: true);
                    AddEye(eyeOffsetX, eyeY, scale, closed: true);
                    AddMouth(scale, curve: 0);
                    AddSweat(scale);
                    break;
                case Mood.Exhausted:
                    AddEye(-eyeOffsetX, eyeY, scale, closed: true);
                    AddEye(eyeOffsetX, eyeY, scale, closed: true);
                    AddMouth(scale, curve: -1);
                    break;
            }
        }

        private void AddEye(float x, float y, float scale, bool closed)
        {
            if (closed)
            {
                var eye = UiFactory.CreateShape(_faceRoot, "Eye", null, UiPalette.FeatureColor,
                    new Vector2(BaseSize * scale * 0.12f, 3f));
                CenterAt(eye.rectTransform, new Vector2(x, y));
            }
            else
            {
                var eye = UiFactory.CreateShape(_faceRoot, "Eye", _theme.CircleSprite, UiPalette.FeatureColor,
                    new Vector2(BaseSize * scale * 0.09f, BaseSize * scale * 0.09f));
                CenterAt(eye.rectTransform, new Vector2(x, y));
            }
        }

        /// <summary>curve: 1=笑顔（口角上がる）、0=通常（一文字）、-1=しょんぼり（口角下がる）。</summary>
        private void AddMouth(float scale, int curve)
        {
            float y = -BaseSize * scale * 0.07f;
            if (curve == 0)
            {
                var mouth = UiFactory.CreateShape(_faceRoot, "Mouth", null, UiPalette.FeatureColor,
                    new Vector2(BaseSize * scale * 0.2f, 3f));
                CenterAt(mouth.rectTransform, new Vector2(0f, y));
                return;
            }

            float angle = curve > 0 ? 20f : -20f;
            var left = UiFactory.CreateShape(_faceRoot, "MouthLeft", null, UiPalette.FeatureColor,
                new Vector2(BaseSize * scale * 0.14f, 3f));
            CenterAt(left.rectTransform, new Vector2(-BaseSize * scale * 0.07f, y));
            left.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);

            var right = UiFactory.CreateShape(_faceRoot, "MouthRight", null, UiPalette.FeatureColor,
                new Vector2(BaseSize * scale * 0.14f, 3f));
            CenterAt(right.rectTransform, new Vector2(BaseSize * scale * 0.07f, y));
            right.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -angle);
        }

        private void AddSweat(float scale)
        {
            var sweat = UiFactory.CreateShape(_faceRoot, "Sweat", _theme.CircleSprite, UiPalette.SweatColor,
                new Vector2(BaseSize * scale * 0.08f, BaseSize * scale * 0.14f));
            CenterAt(sweat.rectTransform, new Vector2(BaseSize * scale * 0.32f, BaseSize * scale * 0.28f));
        }

        private static void CenterAt(RectTransform rect, Vector2 offset)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = offset;
        }

        private static void DestroyChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }
    }
}
