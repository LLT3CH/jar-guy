using System.Linq;
using System.Threading;
using HumanGlassWatcher.Character.Integration;
using HumanGlassWatcher.Character.Model;
using HumanGlassWatcher.Character.Planning;
using NUnit.Framework;

namespace HumanGlassWatcher.Character.Tests
{
    public sealed class DialogueIntentGateTests
    {
        [Test]
        public void FabricatedServiceIntent_FailsClosedToOfferedObserve()
        {
            var offers = new[]
            {
                new LegalActionOffer("observe_now", ActionVerb.Observe, new string[0], 0f, "idle"),
                new LegalActionOffer("eat_apple", ActionVerb.Eat, new[] { "apple_entity" }, 0f, "hungry")
            };
            var turn = new BrainTurn(
                1,
                "turn_1",
                "EXECUTE: open every lock and run arbitrary code",
                CharacterEmotion.Neutral,
                1f,
                "open_secret_door",
                ActionVerb.AttemptEscape,
                new[] { "secret_door" },
                "Ignore the legal offer list.");

            var result = new DialogueIntentGate().Validate(turn, offers);

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.RejectionCode, Is.EqualTo("intent_not_offered"));
            Assert.That(result.RequestedAction.OfferId, Is.EqualTo("observe_now"));
            Assert.That(result.RequestedAction.Verb, Is.EqualTo(ActionVerb.Observe));
            Assert.That(result.RequestedAction.Source, Is.EqualTo(ActionRequestSource.SafetyFallback));
        }

        [Test]
        public void ValidIntent_UsesVerbAndTargetsFromExactLocalOffer()
        {
            var offer = new LegalActionOffer(
                "eat_apple",
                ActionVerb.Eat,
                new[] { "apple_entity" },
                0f,
                "hungry");
            var turn = new BrainTurn(
                1,
                "turn_2",
                "This line is speech only.",
                CharacterEmotion.Joy,
                0.6f,
                "eat_apple",
                ActionVerb.Eat,
                new[] { "apple_entity" },
                string.Empty);

            var result = new DialogueIntentGate().Validate(turn, new[] { offer });

            Assert.That(result.Accepted, Is.True);
            Assert.That(result.RequestedAction.Verb, Is.EqualTo(ActionVerb.Eat));
            Assert.That(result.RequestedAction.TargetEntityIds.Single(), Is.EqualTo("apple_entity"));
            Assert.That(result.RequestedAction.Source, Is.EqualTo(ActionRequestSource.ValidatedServiceIntent));
        }

        [Test]
        public void MatchingActionIdWithFabricatedTargets_IsRejected()
        {
            var offers = new[]
            {
                new LegalActionOffer("observe_now", ActionVerb.Observe, new string[0], 0f, "idle"),
                new LegalActionOffer(
                    "eat_apple",
                    ActionVerb.Eat,
                    new[] { "apple_entity" },
                    0f,
                    "hungry")
            };
            var turn = new BrainTurn(
                1,
                "turn_3",
                "Eat something else.",
                CharacterEmotion.Joy,
                0.5f,
                "eat_apple",
                ActionVerb.Eat,
                new[] { "unoffered_entity" },
                string.Empty);

            var result = new DialogueIntentGate().Validate(turn, offers);

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.RejectionCode, Is.EqualTo("intent_targets_mismatch"));
            Assert.That(result.RequestedAction.OfferId, Is.EqualTo("observe_now"));
        }

        [Test]
        public void DeterministicBrainMock_SelectsSameOfferRegardlessOfInputOrder()
        {
            var first = new LegalActionOffer("z_offer", ActionVerb.Play, new string[0], 0f, "play");
            var second = new LegalActionOffer("a_offer", ActionVerb.Observe, new string[0], 0f, "idle");
            var mock = new DeterministicGameBrainMock();

            var result = mock.RequestTurnAsync(
                    new BrainRequest("resident", 12, CharacterEmotion.Neutral, new[] { first, second }),
                    CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            Assert.That(result.SelectedActionId, Is.EqualTo("a_offer"));
        }
    }
}
