using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HumanGlassWatcher.Gameplay.Editor
{
    public static class JarLoopBuild
    {
        private const string OutputArgument = "-buildOutput";
        private const string WindowsDefault = "Builds/Windows/HumanGlassWatcher.exe";
        private const string AndroidDefault = "Builds/Android/HumanGlassWatcher.apk";

        public static void BuildWindows()
        {
            PlayerSettings.SetScriptingBackend(
                NamedBuildTarget.Standalone,
                ScriptingImplementation.Mono2x);

            BuildPlayer(
                BuildTarget.StandaloneWindows64,
                GetOutputPath(WindowsDefault));
        }

        public static void BuildAndroid()
        {
            PlayerSettings.SetScriptingBackend(
                NamedBuildTarget.Android,
                ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            EditorUserBuildSettings.buildAppBundle = false;

            BuildPlayer(
                BuildTarget.Android,
                GetOutputPath(AndroidDefault));
        }

        private static void BuildPlayer(BuildTarget target, string outputPath)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(JarLoopSceneBuilder.ScenePath) == null)
            {
                throw new BuildFailedException(
                    $"Playable scene is missing at {JarLoopSceneBuilder.ScenePath}.");
            }

            var directory = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new BuildFailedException($"Invalid build output path: {outputPath}");
            }

            Directory.CreateDirectory(directory);
            var options = new BuildPlayerOptions
            {
                scenes = new[] { JarLoopSceneBuilder.ScenePath },
                locationPathName = outputPath,
                target = target,
                options = BuildOptions.None
            };

            Debug.Log($"Building {target} to {outputPath}.");
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(
                    $"{target} build failed with {report.summary.totalErrors} error(s).");
            }

            Debug.Log(
                $"{target} build succeeded: {report.summary.totalSize} bytes at {outputPath}.");
        }

        private static string GetOutputPath(string defaultRelativePath)
        {
            var arguments = Environment.GetCommandLineArgs();
            var argumentIndex = Array.FindIndex(
                arguments,
                argument => string.Equals(argument, OutputArgument, StringComparison.OrdinalIgnoreCase));

            var requestedPath = argumentIndex >= 0 && argumentIndex + 1 < arguments.Length
                ? arguments[argumentIndex + 1]
                : defaultRelativePath;

            return Path.GetFullPath(requestedPath);
        }
    }
}
