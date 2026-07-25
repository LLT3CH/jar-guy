using System.Linq;
using System.Threading;
using HumanGlassWatcher.Core.Interactions;
using HumanGlassWatcher.Core.Services;
using HumanGlassWatcher.Gameplay.Items;
using NUnit.Framework;

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
}
