using System.IO;
using HumanGlassWatcher.Character.Model;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HumanGlassWatcher.Character.Presentation.Editor
{
    public static class ResidentPresentationPreviewCapture
    {
        public static string DefaultOutputPath =>
            Path.Combine(Path.GetTempPath(), "HumanGlassWatcher.ResidentPresentation.png");

        [MenuItem("Human Glass Watcher/Character/Capture Resident Presentation")]
        public static void Capture()
        {
            Capture(DefaultOutputPath);
        }

        public static void Capture(string outputPath)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.42f, 0.48f, 0.60f);
            RenderSettings.ambientEquatorColor = new Color(0.20f, 0.24f, 0.32f);
            RenderSettings.ambientGroundColor = new Color(0.075f, 0.085f, 0.12f);

            var anchor = new GameObject("Resident Preview");
            var rig = ResidentPresentationFactory.Build(anchor.transform);
            var controller = rig.gameObject.AddComponent<ResidentPresentationController>();
            controller.Initialize(rig);
            controller.SetReaction(ResidentReaction.Celebrate, 0.76f, 30f);
            controller.SnapToPose(1.2f);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            floor.name = "Preview Plinth";
            floor.transform.position = new Vector3(0f, -1.11f, 0f);
            floor.transform.localScale = new Vector3(1.18f, 0.055f, 1.18f);
            floor.GetComponent<Renderer>().sharedMaterial = PreviewMaterial(
                new Color(0.08f, 0.105f, 0.16f));

            var key = new GameObject("Preview Key").AddComponent<Light>();
            key.type = LightType.Directional;
            key.transform.rotation = Quaternion.Euler(38f, -32f, 0f);
            key.color = new Color(1f, 0.86f, 0.72f);
            key.intensity = 1.55f;
            key.shadows = LightShadows.Soft;

            var fill = new GameObject("Preview Fill").AddComponent<Light>();
            fill.type = LightType.Point;
            fill.transform.position = new Vector3(-2.2f, 1.4f, -2.2f);
            fill.color = new Color(0.30f, 0.62f, 1f);
            fill.intensity = 4.2f;
            fill.range = 8f;

            var camera = new GameObject("Preview Camera").AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 0.02f, -4.25f);
            camera.transform.LookAt(new Vector3(0f, -0.02f, 0f));
            camera.fieldOfView = 34f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.052f, 0.09f);

            const int size = 768;
            var target = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 4
            };
            var image = new Texture2D(size, size, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            image.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            File.WriteAllBytes(outputPath, image.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = previous;
            Object.DestroyImmediate(image);
            Object.DestroyImmediate(target);
            Debug.Log($"Resident presentation preview captured at {outputPath}");
        }

        private static Material PreviewMaterial(Color color)
        {
            var shader =
                Shader.Find("Universal Render Pipeline/Lit") ??
                Shader.Find("Standard") ??
                Shader.Find("Sprites/Default");
            var material = new Material(shader)
            {
                color = color
            };
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            return material;
        }
    }
}
