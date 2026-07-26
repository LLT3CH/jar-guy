using System;
using System.Collections.Generic;
using HumanGlassWatcher.Core.Items;
using UnityEngine;

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

            var itemObject = new GameObject($"Item_{definition.CanonicalId}");
            itemObject.transform.SetParent(itemRoot != null ? itemRoot : transform, false);
            itemObject.transform.position = position;
            var physicsMaterial = new PhysicsMaterial($"{definition.CanonicalId}_Physics")
            {
                bounciness = definition.Bounciness,
                dynamicFriction = definition.Has(ItemCapability.Bouncy) ? 0.2f : 0.65f,
                staticFriction = definition.Has(ItemCapability.Bouncy) ? 0.2f : 0.7f,
                bounceCombine = PhysicsMaterialCombine.Maximum
            };
            AddPhysicsColliders(itemObject, definition, physicsMaterial);

            var rigidbody = itemObject.AddComponent<Rigidbody>();
            rigidbody.mass = definition.MassKg;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rigidbody.maxAngularVelocity = 18f;

            var spawnedItem = itemObject.AddComponent<SpawnedItem>();
            spawnedItem.Initialize(definition);
            ProceduralItemVisualBuilder.Build(itemObject, definition);
            ItemSpawned?.Invoke(spawnedItem);
            return spawnedItem;
        }

        private static void AddPhysicsColliders(
            GameObject itemObject,
            ItemDefinition definition,
            PhysicsMaterial physicsMaterial)
        {
            if (definition.CanonicalId == "hockey_stick")
            {
                var shaft = itemObject.AddComponent<BoxCollider>();
                shaft.size = new Vector3(
                    definition.Scale.x * 0.64f,
                    definition.Scale.y * 1.75f,
                    definition.Scale.z * 0.62f);
                shaft.center = new Vector3(0f, definition.Scale.y * 0.08f, 0f);
                shaft.material = physicsMaterial;

                var blade = itemObject.AddComponent<BoxCollider>();
                blade.size = new Vector3(
                    definition.Scale.x * 2.65f,
                    definition.Scale.y * 0.30f,
                    definition.Scale.z * 0.90f);
                blade.center = new Vector3(
                    definition.Scale.x * 1.02f,
                    definition.Scale.y * -0.79f,
                    0f);
                blade.material = physicsMaterial;
                return;
            }

            if (definition.CanonicalId == "scissors")
            {
                var scissors = itemObject.AddComponent<BoxCollider>();
                scissors.size = new Vector3(
                    definition.Scale.x * 1.15f,
                    definition.Scale.z * 1.85f,
                    definition.Scale.y * 1.35f);
                scissors.center = new Vector3(0f, definition.Scale.z * 0.04f, 0f);
                scissors.material = physicsMaterial;
                return;
            }

            Collider collider;
            switch (definition.Archetype)
            {
                case VisualArchetype.Sphere:
                case VisualArchetype.Food:
                case VisualArchetype.Organic:
                    var sphere = itemObject.AddComponent<SphereCollider>();
                    sphere.radius = Mathf.Max(definition.Scale.x, definition.Scale.y, definition.Scale.z) * 0.5f;
                    collider = sphere;
                    break;

                case VisualArchetype.Cylinder:
                case VisualArchetype.Bottle:
                case VisualArchetype.Tool:
                    var capsule = itemObject.AddComponent<CapsuleCollider>();
                    capsule.direction = 1;
                    capsule.radius = Mathf.Max(definition.Scale.x, definition.Scale.z) * 0.46f;
                    capsule.height = Mathf.Max(definition.Scale.y * 2f, capsule.radius * 2f);
                    collider = capsule;
                    break;

                default:
                    var box = itemObject.AddComponent<BoxCollider>();
                    box.size = definition.Scale;
                    collider = box;
                    break;
            }

            collider.material = physicsMaterial;
        }
    }

    public static class PlaceholderMaterials
    {
        private const string OpaqueResourcePath = "ProceduralMaterials/ProceduralOpaque";
        private const string TransparentResourcePath = "ProceduralMaterials/ProceduralTransparent";

        private static readonly Dictionary<uint, Material> OpaqueCache = new();
        private static readonly Dictionary<uint, Material> TransparentCache = new();

        public static Material CreateOpaque(Color color)
        {
            return CreateFromTemplate(color, false);
        }

        public static Material CreateTransparent(Color color)
        {
            return CreateFromTemplate(color, true);
        }

        public static void ClearRuntimeCache()
        {
            DestroyCachedMaterials(OpaqueCache);
            DestroyCachedMaterials(TransparentCache);
        }

        private static Material CreateFromTemplate(Color color, bool transparent)
        {
            var cache = transparent ? TransparentCache : OpaqueCache;
            var key = ColorKey(color);
            if (cache.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            var resourcePath = transparent ? TransparentResourcePath : OpaqueResourcePath;
            var template = Resources.Load<Material>(resourcePath);
            if (template == null)
            {
                throw new InvalidOperationException(
                    $"Missing build-referenced gameplay material at Resources/{resourcePath}.mat.");
            }

            var material = new Material(template)
            {
                color = color,
                name = transparent
                    ? $"Procedural_Transparent_{ColorUtility.ToHtmlStringRGBA(color)}"
                    : $"Procedural_Opaque_{ColorUtility.ToHtmlStringRGB(color)}",
                hideFlags = HideFlags.DontSave
            };
            material.SetColor("_BaseColor", color);
            cache[key] = material;
            return material;
        }

        private static uint ColorKey(Color color)
        {
            Color32 packed = color;
            return ((uint)packed.r << 24) |
                   ((uint)packed.g << 16) |
                   ((uint)packed.b << 8) |
                   packed.a;
        }

        private static void DestroyCachedMaterials(Dictionary<uint, Material> cache)
        {
            foreach (var material in cache.Values)
            {
                if (material == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(material);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(material);
                }
            }

            cache.Clear();
        }
    }
}
