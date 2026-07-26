using System;
using System.IO;
using System.Linq;
using HumanGlassWatcher.Gameplay.Input;
using HumanGlassWatcher.Gameplay.Scene;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HumanGlassWatcher.Gameplay.Editor
{
    [InitializeOnLoad]
    public static class JarLoopLiveDemoLauncher
    {
        private const string RequestPath = "Temp/JarLoopLiveDemo.request";

        private static int stage;
        private static double stageStartedAt;
        private static JarLoopSceneBootstrap bootstrap;
        private static bool runSequenceWhenPlayStarts;

        static JarLoopLiveDemoLauncher()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.delayCall += TryLaunchRequestedDemo;
        }

        [MenuItem("Human Glass Watcher/Start Live Jar Demo", priority = 1)]
        public static void StartLiveDemo()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Stop Play Mode before starting the live jar demo.");
                return;
            }

            if (!CanReplaceActiveSceneWithoutPrompt() &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(JarLoopSceneBuilder.ScenePath, OpenSceneMode.Single);
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            runSequenceWhenPlayStarts = true;
            EditorApplication.delayCall += () => EditorApplication.isPlaying = true;
        }

        private static void TryLaunchRequestedDemo()
        {
            var absoluteRequestPath = Path.GetFullPath(RequestPath);
            if (!File.Exists(absoluteRequestPath))
            {
                return;
            }

            File.Delete(absoluteRequestPath);
            StartLiveDemo();
        }

        private static bool CanReplaceActiveSceneWithoutPrompt()
        {
            var activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || !string.IsNullOrEmpty(activeScene.path))
            {
                return false;
            }

            return activeScene.GetRootGameObjects().All(root =>
                root.name == "Main Camera" || root.name == "Directional Light");
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode)
            {
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    EditorApplication.update -= AdvanceDemo;
                }

                return;
            }

            if (!runSequenceWhenPlayStarts)
            {
                return;
            }

            runSequenceWhenPlayStarts = false;
            bootstrap = null;
            stage = 0;
            stageStartedAt = EditorApplication.timeSinceStartup;
            EditorApplication.update -= AdvanceDemo;
            EditorApplication.update += AdvanceDemo;
        }

        private static void AdvanceDemo()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.update -= AdvanceDemo;
                return;
            }

            bootstrap ??= UnityEngine.Object.FindAnyObjectByType<JarLoopSceneBootstrap>();
            if (bootstrap == null || !bootstrap.IsReady)
            {
                return;
            }

            var elapsed = EditorApplication.timeSinceStartup - stageStartedAt;
            var screenWidth = Mathf.Max(Screen.width, 100f);
            var gestureY = Screen.height * 0.8f;

            switch (stage)
            {
                case 0 when elapsed >= 1d:
                    bootstrap.LidController.ProcessPointer(
                        new PointerSample(new Vector2(screenWidth * 0.25f, gestureY), true, true, false),
                        screenWidth);
                    NextStage();
                    break;

                case 1:
                    var fraction = Mathf.Clamp01((float)(elapsed / 1.25d));
                    bootstrap.LidController.ProcessPointer(
                        new PointerSample(
                            new Vector2(Mathf.Lerp(screenWidth * 0.25f, screenWidth * 0.52f, fraction), gestureY),
                            fraction < 1f,
                            false,
                            fraction >= 1f),
                        screenWidth);
                    if (fraction >= 1f)
                    {
                        NextStage();
                    }

                    break;

                case 2 when elapsed >= 0.5d:
                    bootstrap.LidController.ItemInput.text = "rubber ball";
                    NextStage();
                    break;

                case 3 when elapsed >= 0.9d:
                    _ = bootstrap.LidController.SubmitPromptAsync("rubber ball");
                    NextStage();
                    break;

                case 4 when elapsed >= 2d:
                    bootstrap.LidController.OpenSearch();
                    bootstrap.LidController.ItemInput.text = "hockey stick";
                    NextStage();
                    break;

                case 5 when elapsed >= 1d:
                    _ = bootstrap.LidController.SubmitPromptAsync("hockey stick");
                    NextStage();
                    break;

                case 6 when elapsed >= 4d:
                    EditorApplication.update -= AdvanceDemo;
                    Debug.Log("Live jar demo complete: rubber ball and hockey stick spawned through the lid loop.");
                    break;
            }
        }

        private static void NextStage()
        {
            stage++;
            stageStartedAt = EditorApplication.timeSinceStartup;
        }
    }
}
