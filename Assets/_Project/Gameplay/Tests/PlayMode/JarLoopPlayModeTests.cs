using System.Collections;
using System.Linq;
using HumanGlassWatcher.Character.Presentation;
using HumanGlassWatcher.Core.Interactions;
using HumanGlassWatcher.Gameplay.Input;
using HumanGlassWatcher.Gameplay.Interactions;
using HumanGlassWatcher.Gameplay.Items;
using HumanGlassWatcher.Gameplay.Scene;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace HumanGlassWatcher.Gameplay.Tests.PlayMode
{
    public sealed class JarLoopPlayModeTests
    {
        [UnitySetUp]
        public IEnumerator LoadPlayableScene()
        {
            var operation = SceneManager.LoadSceneAsync("JarLoop", LoadSceneMode.Single);
            Assert.That(operation, Is.Not.Null, "JarLoop must be enabled in build settings.");
            while (!operation.isDone)
            {
                yield return null;
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator SceneBootsWithJarResidentAndSharedGestureLoop()
        {
            var bootstrap = Object.FindFirstObjectByType<JarLoopSceneBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(bootstrap.IsReady, Is.True);
            Assert.That(GameObject.Find("Transparent Jar"), Is.Not.Null);
            Assert.That(GameObject.Find("Collision Interior"), Is.Not.Null);
            Assert.That(bootstrap.ResidentTarget, Is.Not.Null);

            var controller = bootstrap.LidController;
            var screenWidth = Mathf.Max(Screen.width, 100f);
            var gestureY = Screen.height * 0.8f;
            controller.ProcessPointer(
                new PointerSample(new Vector2(screenWidth * 0.25f, gestureY), true, true, false),
                screenWidth);
            controller.ProcessPointer(
                new PointerSample(new Vector2(screenWidth * 0.5f, gestureY), false, false, true),
                screenWidth);

            Assert.That(controller.IsSearchOpen, Is.True, "A 25% drag should reveal search.");
            controller.Cancel();
            Assert.That(controller.IsSearchOpen, Is.False);
            Assert.That(bootstrap.ItemFactory.GetComponentsInChildren<SpawnedItem>(), Is.Empty);
            LogAssert.NoUnexpectedReceived();
            yield return null;
        }

        [UnityTest]
        public IEnumerator SubmitClosesSearchAndSpawnsAFallingRigidbody()
        {
            var bootstrap = Object.FindFirstObjectByType<JarLoopSceneBootstrap>();
            bootstrap.LidController.OpenSearch();
            var submit = bootstrap.LidController.SubmitPromptAsync("rubber ball");
            while (!submit.IsCompleted)
            {
                yield return null;
            }

            Assert.That(submit.Result.CanSpawn, Is.True);
            Assert.That(bootstrap.LidController.IsSearchOpen, Is.False);
            var ball = Object.FindObjectsByType<SpawnedItem>(FindObjectsSortMode.None)
                .Single(item => item.Definition.CanonicalId == "rubber_ball");
            var body = ball.GetComponent<Rigidbody>();
            Assert.That(body, Is.Not.Null);
            var visual = ball.GetComponent<ProceduralItemVisual>();
            Assert.That(visual, Is.Not.Null);
            Assert.That(visual.StyleId, Is.EqualTo("rubber_ball"));
            Assert.That(visual.PartCount, Is.GreaterThanOrEqualTo(4));
            var startY = body.position.y;

            for (var index = 0; index < 12; index++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(body.position.y, Is.LessThan(startY - 0.1f));
            Assert.That(
                bootstrap.AffordanceTracker.Available.Any(action => action.Kind == AffordanceKind.Play),
                Is.True);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator CapabilityPairAppearsAfterTwoFactorySpawns()
        {
            var bootstrap = Object.FindFirstObjectByType<JarLoopSceneBootstrap>();
            var catalog = new LocalItemCatalog();
            catalog.TryGet("rubber_ball", out var ball);
            catalog.TryGet("hockey_stick", out var stick);

            bootstrap.ItemFactory.Spawn(ball, new Vector3(-0.5f, 7.5f, 0f));
            bootstrap.ItemFactory.Spawn(stick, new Vector3(0.5f, 7.8f, 0f));
            yield return null;

            Assert.That(
                bootstrap.AffordanceTracker.Available.Any(action => action.Kind == AffordanceKind.Strike),
                Is.True);
            Assert.That(bootstrap.ReactionPresenter.LastCombination, Is.Not.Null);
            Assert.That(bootstrap.ReactionPresenter.LastCombination.Value.Kind, Is.EqualTo(AffordanceKind.Strike));
            Assert.That(bootstrap.ReactionPresenter.CombinationBanner.gameObject.activeSelf, Is.True);
            Assert.That(bootstrap.ReactionPresenter.CombinationBanner.text, Does.Contain("STRIKE"));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator HazardDropTriggersAnObviousResidentReaction()
        {
            var bootstrap = Object.FindFirstObjectByType<JarLoopSceneBootstrap>();
            var catalog = new LocalItemCatalog();
            catalog.TryGet("dog_feces", out var hazard);

            bootstrap.ItemFactory.Spawn(hazard, new Vector3(0f, 7.5f, 0f));
            yield return null;

            Assert.That(
                bootstrap.ReactionPresenter.LastReaction,
                Is.EqualTo(GameplayReactionKind.Disgusted));
            Assert.That(bootstrap.ReactionPresenter.PresentationController, Is.Not.Null);
            Assert.That(
                bootstrap.ReactionPresenter.PresentationController.CurrentReaction,
                Is.EqualTo(ResidentReaction.Disgust));
            Assert.That(bootstrap.ReactionPresenter.ReactionBadge.text, Does.Contain("HAZARD"));
            Assert.That(bootstrap.ReactionPresenter.ReactionBadge.color.r, Is.GreaterThan(0.9f));
            LogAssert.NoUnexpectedReceived();
        }
    }
}
