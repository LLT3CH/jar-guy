using System;
using HumanGlassWatcher.Core.Items;
using UnityEngine;
using UnityEngine.Rendering;

namespace HumanGlassWatcher.Gameplay.Items
{
    public sealed class RuntimeItemFactory : MonoBehaviour
    {
        [SerializeField] private Transform itemRoot;

        public event Action<SpawnedItem> ItemSpawned;

        public void Configure(Transform root)
        {
            itemRoot = root;
        }

        public SpawnedItem Spawn(ItemDefinition definition, Vector3 position)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            var primitive = PrimitiveFor(definition.Archetype);
            var itemObject = GameObject.CreatePrimitive(primitive);
            itemObject.transform.SetParent(itemRoot != null ? itemRoot : transform, false);
            itemObject.transform.position = position;
            itemObject.transform.localScale = definition.Scale;
            itemObject.name = $"Item_{definition.CanonicalId}";

            var renderer = itemObject.GetComponent<Renderer>();
            renderer.sharedMaterial = PlaceholderMaterials.CreateOpaque(definition.Color);

            var collider = itemObject.GetComponent<Collider>();
            var physicsMaterial = new PhysicsMaterial($"{definition.CanonicalId}_Physics")
            {
                bounciness = definition.Bounciness,
                dynamicFriction = definition.Has(ItemCapability.Bouncy) ? 0.2f : 0.65f,
                staticFriction = definition.Has(ItemCapability.Bouncy) ? 0.2f : 0.7f,
                bounceCombine = PhysicsMaterialCombine.Maximum
            };
            collider.material = physicsMaterial;

            var rigidbody = itemObject.AddComponent<Rigidbody>();
            rigidbody.mass = definition.MassKg;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rigidbody.maxAngularVelocity = 18f;

            if (definition.Has(ItemCapability.LightSource))
            {
                AddPlaceholderLight(itemObject);
            }

            var spawnedItem = itemObject.AddComponent<SpawnedItem>();
            spawnedItem.Initialize(definition);
            ItemSpawned?.Invoke(spawnedItem);
            return spawnedItem;
        }

        private static PrimitiveType PrimitiveFor(VisualArchetype archetype)
        {
            switch (archetype)
            {
                case VisualArchetype.Sphere:
                case VisualArchetype.Food:
                case VisualArchetype.Organic:
                    return PrimitiveType.Sphere;
                case VisualArchetype.Cylinder:
                case VisualArchetype.Bottle:
                case VisualArchetype.Tool:
                    return PrimitiveType.Cylinder;
                default:
                    return PrimitiveType.Cube;
            }
        }

        private static void AddPlaceholderLight(GameObject parent)
        {
            var lightObject = new GameObject("Placeholder_Beam");
            lightObject.transform.SetParent(parent.transform, false);
            lightObject.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            lightObject.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.range = 7f;
            light.spotAngle = 42f;
            light.intensity = 6f;
            light.color = new Color(1f, 0.92f, 0.68f);
        }
    }

    public static class PlaceholderMaterials
    {
        private const string UrpLitShader = "Universal Render Pipeline/Lit";
        private const string BuiltInLitShader = "Standard";

        public static Material CreateOpaque(Color color)
        {
            var material = new Material(FindLitShader())
            {
                color = color,
                name = $"Placeholder_{ColorUtility.ToHtmlStringRGB(color)}"
            };
            material.SetColor("_BaseColor", color);
            return material;
        }

        public static Material CreateTransparent(Color color)
        {
            var material = CreateOpaque(color);
            material.name = "Placeholder_Glass";
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)RenderQueue.Transparent;
            return material;
        }

        private static Shader FindLitShader()
        {
            var shader = Shader.Find(UrpLitShader);
            if (shader == null)
            {
                shader = Shader.Find(BuiltInLitShader);
            }

            if (shader == null)
            {
                throw new InvalidOperationException("No supported lit shader is available.");
            }

            return shader;
        }
    }
}
