using SukoyakaBuddy.Model;
using SukoyakaBuddy.View;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SukoyakaBuddy.Boot
{
    /// <summary>
    /// シーン起動時のエントリポイント（requirements.md 12.6節：シーン・UIはすべてコードで動的生成する）。
    /// Canvas・EventSystemを生成し、GameControllerを作って初期化する。
    /// [RuntimeInitializeOnLoadMethod]で自動起動するため、シーンファイルは完全に空でよい
    /// （org内の既存Unityプロジェクトrobo-farm-rules-demoと同じ規約。GameObjectの事前配置に依存しない）。
    /// </summary>
    public static class GameBootstrap
    {
        private const string RootName = "SukoyakaBuddyRoot";
        private const string CanvasName = "MainCanvas";
        private const string EventSystemName = "EventSystem";
        private const string GameControllerName = "GameController";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var root = new GameObject(RootName);
            Object.DontDestroyOnLoad(root);

            EnsureEventSystem();
            var canvasRoot = BuildCanvas(root.transform);

            var theme = new UiTheme();
            var controllerGameObject = new GameObject(GameControllerName);
            controllerGameObject.transform.SetParent(root.transform, false);
            var controller = controllerGameObject.AddComponent<GameController>();
            controller.Initialize(canvasRoot, theme);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;

            var eventSystemObject = new GameObject(EventSystemName);
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private static RectTransform BuildCanvas(Transform parent)
        {
            var canvasObject = new GameObject(CanvasName, typeof(RectTransform));
            canvasObject.transform.SetParent(parent, false);

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(GameConstants.ReferenceWidth, GameConstants.ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            var background = canvasObject.AddComponent<Image>();
            background.color = UiPalette.Background;

            return (RectTransform)canvasObject.transform;
        }
    }
}
