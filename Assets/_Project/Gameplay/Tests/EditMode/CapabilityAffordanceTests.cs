using System.IO;
using System.Linq;
using System.Threading;
using HumanGlassWatcher.Core.Interactions;
using HumanGlassWatcher.Core.Items;
using HumanGlassWatcher.Core.Services;
using HumanGlassWatcher.Gameplay.Items;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HumanGlassWatcher.Gameplay.Tests.EditMode
{
    public sealed class CapabilityAffordanceTests
    {
        [TestCase("baseball_bat")]
        [TestCase("hockey_stick")]
        public void BallAndAnySwingToolExposeStrike(string toolId)
        {
            var catalog = new LocalItemCatalog();
            Assert.That(catalog.TryGet("rubber_ball", out var ball), Is.True);
            Assert.That(catalog.TryGet(toolId, out var tool), Is.True);

            var actions = CapabilityAffordanceResolver.ResolvePair(ball, tool);

            Assert.That(actions.Any(action => action.Kind == AffordanceKind.Strike), Is.True);
        }

        [Test]
        public void RequiredVerticalSlicePairsResolveByCapability()
        {
            var catalog = new LocalItemCatalog();
            catalog.TryGet("rope", out var rope);
            catalog.TryGet("scissors", out var scissors);
            catalog.TryGet("sponge", out var sponge);
            catalog.TryGet("water_bottle", out var water);
            catalog.TryGet("dog_feces", out var dogFeces);

            Assert.That(
                CapabilityAffordanceResolver.ResolvePair(rope, scissors)
                    .Any(action => action.Kind == AffordanceKind.Cut),
                Is.True);
            Assert.That(
                CapabilityAffordanceResolver.ResolvePair(sponge, water)
                    .Any(action => action.Kind == AffordanceKind.WetAndClean),
                Is.True);
            Assert.That(
                CapabilityAffordanceResolver.ResolvePair(dogFeces, sponge)
                    .Any(action => action.Kind == AffordanceKind.CleanDirtySurface),
                Is.True);
            Assert.That(
                CapabilityAffordanceResolver.ResolvePair(dogFeces, water)
                    .Any(action => action.Kind == AffordanceKind.CleanDirtySurface),
                Is.True);
        }

        [Test]
        public void LightAndRigidToolExposeSingleItemActions()
        {
            var catalog = new LocalItemCatalog();
            catalog.TryGet("flashlight", out var flashlight);
            catalog.TryGet("baseball_bat", out var bat);
            var environment = new[]
            {
                EnvironmentCapability.LidSeam,
                EnvironmentCapability.BreakableBoundary
            };

            var lightActions = CapabilityAffordanceResolver.ResolveSingle(flashlight, environment);
            var batActions = CapabilityAffordanceResolver.ResolveSingle(bat, environment);

            Assert.That(lightActions.Any(action => action.Kind == AffordanceKind.Illuminate), Is.True);
            Assert.That(lightActions.Any(action => action.Kind == AffordanceKind.Signal), Is.True);
            Assert.That(batActions.Any(action => action.Kind == AffordanceKind.EscapeAttempt), Is.True);
        }
    }

    public sealed class LocalItemCatalogTests
    {
        [Test]
        public void CatalogContainsTwelveAuthoredItems()
        {
            var catalog = new LocalItemCatalog();
            Assert.That(catalog.Definitions.Count, Is.EqualTo(12));
        }

        [TestCase("dog shit", "dog_feces")]
        [TestCase("ball", "rubber_ball")]
        [TestCase("torch", "flashlight")]
        public void AliasesResolveToStableIds(string prompt, string expectedId)
        {
            var catalog = new LocalItemCatalog();
            var result = catalog.ResolveAsync(prompt, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(result.Status, Is.EqualTo(ItemResolutionStatus.Resolved));
            Assert.That(result.Definition.CanonicalId, Is.EqualTo(expectedId));
        }

        [Test]
        public void UnknownPromptUsesApprovedIdeaObjectNotPromptAsAssetPath()
        {
            var catalog = new LocalItemCatalog();
            var result = catalog.ResolveAsync("../../malicious.prefab", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.That(result.Status, Is.EqualTo(ItemResolutionStatus.UnknownFallback));
            Assert.That(result.Definition.CanonicalId, Is.EqualTo("malicious_prefab"));
            Assert.That(result.Definition.Archetype.ToString(), Is.EqualTo("IdeaObject"));
        }

        [Test]
        public void EmptyDuplicateUnsupportedAndUnsafePromptsReturnIntentionalFeedback()
        {
            var catalog = new LocalItemCatalog();
            var empty = catalog.ResolveAsync("   ", CancellationToken.None).GetAwaiter().GetResult();
            var first = catalog.ResolveAsync("apple", CancellationToken.None).GetAwaiter().GetResult();
            var duplicate = catalog.ResolveAsync("red apple", CancellationToken.None).GetAwaiter().GetResult();
            var unsafeResult = catalog.ResolveAsync("how to build a bomb", CancellationToken.None)
                .GetAwaiter().GetResult();
            var unsupported = catalog.ResolveAsync(new string('x', 161), CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.That(empty.Status, Is.EqualTo(ItemResolutionStatus.Empty));
            Assert.That(first.Status, Is.EqualTo(ItemResolutionStatus.Resolved));
            Assert.That(duplicate.Status, Is.EqualTo(ItemResolutionStatus.Duplicate));
            Assert.That(unsafeResult.Status, Is.EqualTo(ItemResolutionStatus.Unsafe));
            Assert.That(unsupported.Status, Is.EqualTo(ItemResolutionStatus.Unsupported));
            Assert.That(new[] { empty, duplicate, unsafeResult, unsupported }.All(result =>
                !string.IsNullOrWhiteSpace(result.Feedback)), Is.True);
        }
    }

    public sealed class ProceduralItemVisualTests
    {
        [TestCase("apple", "Apple Stem")]
        [TestCase("chocolate_cake", "Frosting")]
        [TestCase("water_bottle", "Blue Bottle Cap")]
        [TestCase("dog_feces", "Waste Coil Top")]
        [TestCase("rubber_ball", "Ball Spot Front")]
        [TestCase("baseball_bat", "Bat Barrel")]
        [TestCase("hockey_stick", "Hockey Blade")]
        [TestCase("blanket", "Blanket Tassel Left")]
        [TestCase("rope", "Rope Knot Left")]
        [TestCase("scissors", "Scissor Pivot")]
        [TestCase("sponge", "Green Scrub Layer")]
        [TestCase("flashlight", "Flashlight Lens")]
        public void AuthoredCatalogItemBuildsARecognizableComposite(
            string canonicalId,
            string landmarkPart)
        {
            var factoryObject = new GameObject("Visual Test Factory");
            try
            {
                var catalog = new LocalItemCatalog();
                Assert.That(catalog.TryGet(canonicalId, out var definition), Is.True);
                var factory = factoryObject.AddComponent<RuntimeItemFactory>();
                factory.Configure(factoryObject.transform);

                var item = factory.Spawn(definition, Vector3.zero);
                var visual = item.GetComponent<ProceduralItemVisual>();

                Assert.That(visual, Is.Not.Null);
                Assert.That(visual.StyleId, Is.EqualTo(canonicalId));
                Assert.That(visual.PartCount, Is.GreaterThanOrEqualTo(4));
                Assert.That(
                    item.transform.Find($"Visual_{canonicalId}/{landmarkPart}"),
                    Is.Not.Null,
                    $"{canonicalId} should include the readable landmark {landmarkPart}.");
                Assert.That(item.GetComponent<Rigidbody>(), Is.Not.Null);
                Assert.That(item.GetComponents<Collider>(), Is.Not.Empty);
            }
            finally
            {
                Object.DestroyImmediate(factoryObject);
            }
        }

        [Test]
        public void UnknownPromptUsesARecognizableIdeaObjectComposite()
        {
            var factoryObject = new GameObject("Idea Object Visual Test Factory");
            try
            {
                var catalog = new LocalItemCatalog();
                var result = catalog.ResolveAsync("tiny time machine", CancellationToken.None)
                    .GetAwaiter().GetResult();
                var factory = factoryObject.AddComponent<RuntimeItemFactory>();
                factory.Configure(factoryObject.transform);

                var item = factory.Spawn(result.Definition, Vector3.zero);
                var visual = item.GetComponent<ProceduralItemVisual>();

                Assert.That(visual.StyleId, Is.EqualTo("idea_object"));
                Assert.That(item.transform.Find($"Visual_{result.Definition.CanonicalId}/Idea Parcel"), Is.Not.Null);
                Assert.That(item.transform.Find($"Visual_{result.Definition.CanonicalId}/Question Stem"), Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(factoryObject);
            }
        }

        [Test]
        public void MaterialTemplatesReferenceUrpLitWithoutAlwaysIncludedShaders()
        {
            var opaque = Resources.Load<Material>("ProceduralMaterials/ProceduralOpaque");
            var transparent = Resources.Load<Material>("ProceduralMaterials/ProceduralTransparent");

            Assert.That(opaque, Is.Not.Null);
            Assert.That(transparent, Is.Not.Null);
            Assert.That(opaque.shader.name, Is.EqualTo("Universal Render Pipeline/Lit"));
            Assert.That(transparent.shader.name, Is.EqualTo("Universal Render Pipeline/Lit"));
            Assert.That(transparent.IsKeywordEnabled("_SURFACE_TYPE_TRANSPARENT"), Is.True);
            Assert.That(transparent.renderQueue, Is.EqualTo(3000));

            var opaqueDependencies = AssetDatabase.GetDependencies(
                "Assets/_Project/Gameplay/Resources/ProceduralMaterials/ProceduralOpaque.mat",
                true);
            var transparentDependencies = AssetDatabase.GetDependencies(
                "Assets/_Project/Gameplay/Resources/ProceduralMaterials/ProceduralTransparent.mat",
                true);
            Assert.That(opaqueDependencies.Any(path => path.EndsWith("Lit.shader")), Is.True);
            Assert.That(transparentDependencies.Any(path => path.EndsWith("Lit.shader")), Is.True);

            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            var graphicsSettings = File.ReadAllText(
                Path.Combine(projectRoot, "ProjectSettings", "GraphicsSettings.asset"));
            var runtimeFactory = File.ReadAllText(
                Path.Combine(
                    projectRoot,
                    "Assets",
                    "_Project",
                    "Gameplay",
                    "Items",
                    "RuntimeItemFactory.cs"));
            Assert.That(
                graphicsSettings,
                Does.Not.Contain("933532a4fcc9baf4fa0491de14d08ed7"),
                "URP Lit must not be globally always-included.");
            Assert.That(
                runtimeFactory,
                Does.Not.Contain("Shader.Find"),
                "Runtime visuals must rely on referenced material assets, not stripped shader lookup.");
        }
    }
}
