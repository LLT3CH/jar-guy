using System;
using System.IO;
using HumanGlassWatcher.Gameplay.Scene;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace HumanGlassWatcher.Gameplay.Editor
{
    public static class JarLoopSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Gameplay/Scenes/JarLoop.unity";
        private const string SettingsDirectory = "Assets/_Project/Gameplay/Settings";
        private const string RendererPath = SettingsDirectory + "/JarLoop_UniversalRenderer.asset";
        private const string PipelinePath = SettingsDirectory + "/JarLoop_URP.asset";

        [MenuItem("Human Glass Watcher/Build Playable Jar Scene")]
        public static void BuildPlayableScene()
        {
            EnsureDirectory(Path.GetDirectoryName(ScenePath));
            EnsureUniversalRenderPipeline();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Jar Loop Scene");
            root.AddComponent<JarLoopSceneBootstrap>();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"Could not save playable scene to {ScenePath}.");
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Playable jar scene generated at {ScenePath}.");
        }

        [MenuItem("Human Glass Watcher/Validate Playable Jar Scene")]
        public static void ValidatePlayableScene()
        {
            BuildPlayableScene();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new InvalidOperationException("Playable scene did not open.");
            }

            var bootstrap = UnityEngine.Object.FindFirstObjectByType<JarLoopSceneBootstrap>();
            if (bootstrap == null)
            {
                throw new InvalidOperationException("Playable scene is missing JarLoopSceneBootstrap.");
            }

            if (EditorUtility.scriptCompilationFailed)
            {
                throw new InvalidOperationException("Unity reports script compilation errors.");
            }

            Debug.Log($"Validated open scene '{scene.path}' with zero reported compilation errors.");
        }

        private static void EnsureUniversalRenderPipeline()
        {
            EnsureDirectory(SettingsDirectory);

            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                rendererData.name = "Jar Loop Universal Renderer";
                AssetDatabase.CreateAsset(rendererData, RendererPath);
            }

            var pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipelineAsset == null)
            {
                pipelineAsset = UniversalRenderPipelineAsset.Create(rendererData);
                pipelineAsset.name = "Jar Loop URP";
                pipelineAsset.supportsHDR = false;
                pipelineAsset.msaaSampleCount = 4;
                pipelineAsset.renderScale = 1f;
                AssetDatabase.CreateAsset(pipelineAsset, PipelinePath);
            }

            GraphicsSettings.defaultRenderPipeline = pipelineAsset;
            QualitySettings.renderPipeline = pipelineAsset;
            EditorUtility.SetDirty(pipelineAsset);
        }

        private static void EnsureDirectory(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath) || AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            var normalized = assetPath.Replace('\\', '/');
            var parent = Path.GetDirectoryName(normalized)?.Replace('\\', '/');
            var leaf = Path.GetFileName(normalized);
            EnsureDirectory(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
