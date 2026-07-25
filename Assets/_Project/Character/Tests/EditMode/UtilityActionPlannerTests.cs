using System;
using System.Collections.Generic;
using System.Linq;
using HumanGlassWatcher.Character.Appraisal;
using HumanGlassWatcher.Character.Memory;
using HumanGlassWatcher.Character.Model;
using HumanGlassWatcher.Character.Planning;
using NUnit.Framework;

namespace HumanGlassWatcher.Character.Tests
{
    public sealed class UtilityActionPlannerTests
    {
        private UtilityActionPlanner planner;

        [SetUp]
        public void SetUp()
        {
            planner = new UtilityActionPlanner();
        }

        [Test]
        public void Apple_HighHungerSelectsEat()
        {
            var resident = CreateResident();
            resident.Needs.Hunger = 0.95f;
            resident.Needs.Stimulation = 0.1f;
            var apple = Apple();
            var offers = new[]
            {
                Offer("eat_apple", ActionVerb.Eat, apple.EntityId),
                Offer("observe_apple", ActionVerb.Observe, apple.EntityId)
            };

            var selected = planner.Select(resident, offers, new[] { apple });

            Assert.That(selected.OfferId, Is.EqualTo("eat_apple"));
        }

        [Test]
        public void DogFeces_ProducesDisgustAndAvoidance()
        {
            var resident = CreateResident();
            resident.Needs.Hygiene = 0.8f;
            var feces = DogFeces();
            var appraisal = new ItemAppraisalService().Appraise(resident, feces);
            var offers = new[]
            {
                Offer("approach_mess", ActionVerb.Approach, feces.EntityId),
                Offer("avoid_mess", ActionVerb.Avoid, feces.EntityId)
            };

            var selected = planner.Select(resident, offers, new[] { feces });

            Assert.That(appraisal.Emotion, Is.EqualTo(CharacterEmotion.Disgust));
            Assert.That(selected.OfferId, Is.EqualTo("avoid_mess"));
        }

        [Test]
        public void BallAndAnySwingTool_EnableHighRankedStrikeOffer()
        {
            var resident = CreateResident();
            resident.Needs.Stimulation = 0.9f;
            var ball = Ball();
            var bat = new ItemObservation(
                "tool_entity",
                "hockey_stick",
                new[] { ItemCapability.Grabbable, ItemCapability.SwingTool },
                new[] { "sport", "tool" });
            var offers = new[]
            {
                Offer("strike_ball", ActionVerb.Strike, ball.EntityId, bat.EntityId),
                Offer("observe_ball", ActionVerb.Observe, ball.EntityId)
            };

            var selected = planner.Select(resident, offers, new[] { ball, bat });

            Assert.That(selected.OfferId, Is.EqualTo("strike_ball"));
        }

        [Test]
        public void CleaningPressure_RanksLegalCleaningPathAboveAvoidance()
        {
            var resident = CreateResident();
            resident.Needs.Hygiene = 1f;
            resident.Profile.Traits.CleanlinessValue = 1f;
            var feces = DogFeces();
            var sponge = new ItemObservation(
                "sponge_entity",
                "sponge",
                new[] { ItemCapability.Grabbable, ItemCapability.Absorbent },
                new[] { "cleaning" });
            var water = new ItemObservation(
                "water_entity",
                "water_bottle",
                new[] { ItemCapability.Drinkable, ItemCapability.CleaningAgent },
                new[] { "water" });
            var offers = new[]
            {
                Offer("clean_mess", ActionVerb.Clean, feces.EntityId, sponge.EntityId, water.EntityId),
                Offer("avoid_mess", ActionVerb.Avoid, feces.EntityId)
            };

            var selected = planner.Select(resident, offers, new[] { feces, sponge, water });

            Assert.That(selected.OfferId, Is.EqualTo("clean_mess"));
        }

        [Test]
        public void EnergyAndComfortPressure_SelectRestWithBlanket()
        {
            var resident = CreateResident();
            resident.Needs.Energy = 0.95f;
            resident.Needs.Comfort = 0.85f;
            var blanket = new ItemObservation(
                "blanket_entity",
                "blanket",
                new[] { ItemCapability.Comfort, ItemCapability.Wearable },
                new[] { "soft" },
                comfort: 1f);
            var offers = new[]
            {
                Offer("rest_blanket", ActionVerb.Rest, blanket.EntityId),
                Offer("observe_blanket", ActionVerb.Observe, blanket.EntityId)
            };

            var selected = planner.Select(resident, offers, new[] { blanket });

            Assert.That(selected.OfferId, Is.EqualTo("rest_blanket"));
        }

        [Test]
        public void NeedPressure_ChangesEatVersusPlayRanking()
        {
            var resident = CreateResident();
            var apple = Apple();
            var ball = Ball();
            var offers = new[]
            {
                Offer("eat_apple", ActionVerb.Eat, apple.EntityId),
                Offer("play_ball", ActionVerb.Play, ball.EntityId)
            };

            resident.Needs.Hunger = 0.95f;
            resident.Needs.Stimulation = 0.1f;
            Assert.That(
                planner.Select(resident, offers, new[] { apple, ball }).OfferId,
                Is.EqualTo("eat_apple"));

            resident.Needs.Hunger = 0.1f;
            resident.Needs.Stimulation = 0.95f;
            Assert.That(
                planner.Select(resident, offers, new[] { apple, ball }).OfferId,
                Is.EqualTo("play_ball"));
        }

        [Test]
        public void NegativeTargetMemory_ChangesActionRanking()
        {
            var resident = CreateResident();
            resident.Needs.Hunger = 0.5f;
            resident.Needs.Stimulation = 0.55f;
            var apple = Apple();
            var ball = Ball();
            var offers = new[]
            {
                Offer("eat_apple", ActionVerb.Eat, apple.EntityId),
                Offer("play_ball", ActionVerb.Play, ball.EntityId)
            };

            Assert.That(
                planner.Select(resident, offers, new[] { apple, ball }).OfferId,
                Is.EqualTo("eat_apple"));

            resident.Record(new ResidentEvent(
                "bad_apple",
                1,
                ResidentEventType.Harmed,
                apple.EntityId,
                "player",
                -1f,
                1f,
                "The apple caused pain."));

            Assert.That(
                planner.Select(resident, offers, new[] { apple, ball }).OfferId,
                Is.EqualTo("play_ball"));
        }

        [Test]
        public void ResourcefulnessAndFreedomPressure_ChangeEscapeRanking()
        {
            var tool = new ItemObservation(
                "lever_entity",
                "baseball_bat",
                new[] { ItemCapability.SwingTool, ItemCapability.Lever, ItemCapability.Grabbable },
                new[] { "tool" },
                safetyRisk: 0.2f);
            var offers = new[]
            {
                Offer("attempt_lid", ActionVerb.AttemptEscape, tool.EntityId),
                Offer("observe_lid", ActionVerb.Observe, tool.EntityId)
            };
            var resourceful = CreateResident();
            resourceful.Needs.Freedom = 1f;
            resourceful.Profile.Traits.Resourcefulness = 1f;
            resourceful.Profile.Traits.FreedomValue = 1f;
            resourceful.Profile.Traits.Defiance = 0.9f;
            resourceful.Profile.Traits.ComfortValue = 0f;

            var comfortDriven = CreateResident();
            comfortDriven.Needs.Freedom = 0.1f;
            comfortDriven.Needs.Comfort = 0.05f;
            comfortDriven.Profile.Traits.Resourcefulness = 0f;
            comfortDriven.Profile.Traits.FreedomValue = 0f;
            comfortDriven.Profile.Traits.Defiance = 0f;
            comfortDriven.Profile.Traits.ComfortValue = 1f;

            Assert.That(
                planner.Select(resourceful, offers, new[] { tool }).OfferId,
                Is.EqualTo("attempt_lid"));
            Assert.That(
                planner.Select(comfortDriven, offers, new[] { tool }).OfferId,
                Is.EqualTo("observe_lid"));
            Assert.That(
                Score(resourceful, offers[0], tool),
                Is.GreaterThan(Score(comfortDriven, offers[0], tool) + 100f));
        }

        [Test]
        public void StrikeWithoutRequiredCapabilities_IsDefensivelyRejectedByRanking()
        {
            var resident = CreateResident();
            resident.Needs.Stimulation = 1f;
            var apple = Apple();
            var offers = new[]
            {
                Offer("impossible_strike", ActionVerb.Strike, apple.EntityId),
                Offer("observe", ActionVerb.Observe, apple.EntityId)
            };

            Assert.That(
                planner.Select(resident, offers, new[] { apple }).OfferId,
                Is.EqualTo("observe"));
        }

        private float Score(ResidentState resident, LegalActionOffer offer, params ItemObservation[] items)
        {
            return planner.Rank(resident, new[] { offer }, items).Single().Utility;
        }

        private static ResidentState CreateResident()
        {
            return new ResidentState(
                "resident",
                new ResidentProfile(
                    1,
                    PersonalityTraits.Neutral(),
                    new ResidentPreferences(
                        Array.Empty<string>(),
                        Array.Empty<string>(),
                        Array.Empty<string>(),
                        Array.Empty<string>()),
                    "test_voice",
                    "direct"),
                ResidentNeeds.Neutral(),
                new MoodState(0f, 0.25f, 0f, CharacterEmotion.Neutral),
                new RelationshipState(),
                new EpisodicMemory());
        }

        private static LegalActionOffer Offer(string id, ActionVerb verb, params string[] targets)
        {
            return new LegalActionOffer(id, verb, targets, 0f, "test_offer");
        }

        private static ItemObservation Apple()
        {
            return new ItemObservation(
                "apple_entity",
                "apple",
                new[] { ItemCapability.Grabbable, ItemCapability.Edible },
                new[] { "food", "fruit" },
                taste: 0.8f,
                novelty: 0.2f);
        }

        private static ItemObservation Ball()
        {
            return new ItemObservation(
                "ball_entity",
                "rubber_ball",
                new[]
                {
                    ItemCapability.Grabbable,
                    ItemCapability.Throwable,
                    ItemCapability.Bouncy,
                    ItemCapability.Entertainment
                },
                new[] { "play", "sport" },
                novelty: 0.4f);
        }

        private static ItemObservation DogFeces()
        {
            return new ItemObservation(
                "feces_entity",
                "dog_feces",
                new[] { ItemCapability.Dirty, ItemCapability.Toxic },
                new[] { "gross", "dirty" },
                safetyRisk: 0.35f,
                dirtiness: 1f,
                novelty: 0.6f);
        }
    }
}
