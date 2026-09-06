using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SukoyakaBuddy.EditorTools
{
    /// <summary>
    /// requirements.md 12.6節：
    /// `Unity.exe -batchmode -nographics -executeMethod SukoyakaBuddy.EditorTools.Build.WebGL -quit` でCLIビルドする。
    /// シーンはコードで生成するため、空シーンを用意するだけでよい（プレハブ・シーンファイルへの依存を作らない）。
    /// </summary>
    public static class Build
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";
        private const string OutputDir = "Build/WebGL";

        public static void WebGL()
        {
            EnsureScene();
            EnsureWebGLTarget();

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = OutputDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            });

            var summary = report.summary;
            Debug.Log($"[Build] Build result: {summary.result}, totalErrors={summary.totalErrors}, totalWarnings={summary.totalWarnings}, size={summary.totalSize} bytes");

            if (summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                EditorApplication.Exit(1);
            }
        }

        private static void EnsureScene()
        {
            if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");

            if (!File.Exists(ScenePath))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, ScenePath);
            }

            var existing = EditorBuildSettings.scenes;
            bool alreadyIncluded = false;
            foreach (var s in existing) if (s.path == ScenePath) { alreadyIncluded = true; break; }

            if (!alreadyIncluded)
            {
                var scenes = new EditorBuildSettingsScene[existing.Length + 1];
                existing.CopyTo(scenes, 0);
                scenes[existing.Length] = new EditorBuildSettingsScene(ScenePath, true);
                EditorBuildSettings.scenes = scenes;
            }
        }

        private static void EnsureWebGLTarget()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            }
        }
    }
}
