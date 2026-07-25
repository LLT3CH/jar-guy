using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HumanGlassWatcher.Gameplay.Editor
{
    [InitializeOnLoad]
    public static class JarLoopStartupScene
    {
        private const string StartupCheckKey = "HumanGlassWatcher.JarLoopStartupScene.Checked";

        static JarLoopStartupScene()
        {
            EditorApplication.delayCall += OpenPlayableSceneWhenEditorStartsBlank;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem("Human Glass Watcher/Open Playable Jar Scene", priority = 0)]
        public static void OpenPlayableScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Stop Play Mode before opening the playable jar scene.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            OpenPlayableSceneWithoutPrompt();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.delayCall += OpenPlayableSceneWhenEditorStartsBlank;
            }
        }

        private static void OpenPlayableSceneWhenEditorStartsBlank()
        {
            if (Application.isBatchMode ||
                EditorApplication.isPlayingOrWillChangePlaymode ||
                SessionState.GetBool(StartupCheckKey, false))
            {
                return;
            }

            var activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                return;
            }

            SessionState.SetBool(StartupCheckKey, true);
            if (!string.IsNullOrEmpty(activeScene.path) || activeScene.isDirty)
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(JarLoopSceneBuilder.ScenePath) == null)
            {
                Debug.LogWarning($"Playable jar scene was not found at {JarLoopSceneBuilder.ScenePath}.");
                return;
            }

            OpenPlayableSceneWithoutPrompt();
            Debug.Log("Opened the playable jar scene because Unity started on a blank Untitled scene.");
        }

        private static void OpenPlayableSceneWithoutPrompt()
        {
            EditorSceneManager.OpenScene(JarLoopSceneBuilder.ScenePath, OpenSceneMode.Single);
        }
    }
}
