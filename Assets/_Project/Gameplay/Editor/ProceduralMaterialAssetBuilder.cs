using System;
using System.IO;
using HumanGlassWatcher.Gameplay.Items;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace HumanGlassWatcher.Gameplay.Editor
{
    [InitializeOnLoad]
    public static class ProceduralMaterialAssetBuilder
    {
        private const string MaterialDirectory =
            "Assets/_Project/Gameplay/Resources/ProceduralMaterials";
        private const string OpaquePath = MaterialDirectory + "/ProceduralOpaque.mat";
        private const string TransparentPath = MaterialDirectory + "/ProceduralTransparent.mat";
        private const string UrpLitShader = "Universal Render Pipeline/Lit";

        static ProceduralMaterialAssetBuilder()
        {
            EditorApplication.delayCall += EnsureMaterialAssets;
        }

        [MenuItem("Human Glass Watcher/Rebuild Procedural Material Assets")]
        public static void RebuildMaterialAssets()
        {
            AssetDatabase.DeleteAsset(OpaquePath);
            AssetDatabase.DeleteAsset(TransparentPath);
            EnsureMaterialAssets();
        }

        public static void EnsureMaterialAssets()
        {
            if (AssetDatabase.LoadAssetAtPath<Material>(OpaquePath) != null &&
                AssetDatabase.LoadAssetAtPath<Material>(TransparentPath) != null)
            {
                return;
            }

            var shader = Shader.Find(UrpLitShader);
            if (shader == null)
            {
                throw new InvalidOperationException(
                    $"Editor could not find {UrpLitShader}; procedural material assets cannot be generated.");
            }

            EnsureDirectory(MaterialDirectory);
            if (AssetDatabase.LoadAssetAtPath<Material>(OpaquePath) == null)
            {
                var opaque = new Material(shader)
                {
                    name = "ProceduralOpaque",
                    color = Color.white
                };
                opaque.SetColor("_BaseColor", Color.white);
                AssetDatabase.CreateAsset(opaque, OpaquePath);
            }

            if (AssetDatabase.LoadAssetAtPath<Material>(TransparentPath) == null)
            {
                var transparent = new Material(shader)
                {
                    name = "ProceduralTransparent",
                    color = new Color(1f, 1f, 1f, 0.5f),
                    renderQueue = (int)RenderQueue.Transparent
                };
                transparent.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0.5f));
                transparent.SetFloat("_Surface", 1f);
                transparent.SetFloat("_Blend", 0f);
                transparent.SetFloat("_SrcBlend", (float)BlendMode.One);
                transparent.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                transparent.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
                transparent.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
                transparent.SetFloat("_ZWrite", 0f);
                transparent.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                transparent.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                transparent.SetOverrideTag("RenderType", "Transparent");
                transparent.SetShaderPassEnabled("DepthOnly", false);
                transparent.SetShaderPassEnabled("ShadowCaster", false);
                transparent.SetShaderPassEnabled("MOTIONVECTORS", false);
                AssetDatabase.CreateAsset(transparent, TransparentPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            PlaceholderMaterials.ClearRuntimeCache();
            Debug.Log("Verified build-referenced procedural opaque and transparent URP material assets.");
        }

        private static void EnsureDirectory(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            var parent = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            var leaf = Path.GetFileName(assetPath);
            EnsureDirectory(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
