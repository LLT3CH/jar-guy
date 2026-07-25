using System;
using HumanGlassWatcher.Character.Memory;
using HumanGlassWatcher.Character.Model;
using HumanGlassWatcher.Character.Persistence;
using NUnit.Framework;

namespace HumanGlassWatcher.Character.Tests
{
    public sealed class ResidentPersistenceTests
    {
        [Test]
        public void SaveLoad_RoundTripsAllCharacterStateWithVersion()
        {
            var original = ResidentState.Create(99173, "resident_alpha");
            original.Needs.Hunger = 0.91f;
            original.Needs.Freedom = 0.82f;
            original.Relationship.Trust = 0.67f;
            original.Relationship.Resentment = 0.21f;
            original.SetPlan("pry_lid_plan");
            original.Advance(0.5f);
            original.Record(new ResidentEvent(
                "gift_apple_1",
                original.SimulationTick,
                ResidentEventType.GiftReceived,
                "apple_entity",
                "player",
                0.8f,
                0.9f,
                "The player gave a welcome apple."));
            var mapper = new ResidentSaveMapper();

            var dto = mapper.Capture(original);
            var restored = mapper.Restore(dto);

            Assert.That(dto.SchemaVersion, Is.EqualTo(ResidentSaveMapper.CurrentSchemaVersion));
            Assert.That(restored.ResidentId, Is.EqualTo(original.ResidentId));
            Assert.That(restored.Profile.Seed, Is.EqualTo(original.Profile.Seed));
            Assert.That(restored.SimulationTick, Is.EqualTo(original.SimulationTick));
            Assert.That(restored.CurrentPlanId, Is.EqualTo(original.CurrentPlanId));
            Assert.That(restored.Profile.VoiceIdentity, Is.EqualTo(original.Profile.VoiceIdentity));
            Assert.That(restored.Profile.ConversationStyle, Is.EqualTo(original.Profile.ConversationStyle));
            Assert.That(restored.Profile.Traits.Resourcefulness,
                Is.EqualTo(original.Profile.Traits.Resourcefulness));
            Assert.That(restored.Needs.Hunger, Is.EqualTo(original.Needs.Hunger));
            Assert.That(restored.Needs.Freedom, Is.EqualTo(original.Needs.Freedom));
            Assert.That(restored.Mood.Valence, Is.EqualTo(original.Mood.Valence));
            Assert.That(restored.Mood.Emotion, Is.EqualTo(original.Mood.Emotion));
            Assert.That(restored.Relationship.Trust, Is.EqualTo(original.Relationship.Trust));
            Assert.That(restored.Relationship.Resentment, Is.EqualTo(original.Relationship.Resentment));
            Assert.That(restored.Memory.Events.Count, Is.EqualTo(1));
            Assert.That(restored.Memory.Events[0].EventId, Is.EqualTo("gift_apple_1"));
            Assert.That(restored.Memory.Events[0].Summary,
                Is.EqualTo("The player gave a welcome apple."));
            CollectionAssert.AreEquivalent(
                original.Profile.Preferences.LikedItemIds,
                restored.Profile.Preferences.LikedItemIds);
            CollectionAssert.AreEquivalent(
                original.Profile.Preferences.DislikedTags,
                restored.Profile.Preferences.DislikedTags);
        }

        [Test]
        public void ImportantEventsPersistAndLowImportanceNoiseIsIgnored()
        {
            var resident = ResidentState.Create(8);

            Assert.That(resident.Record(new ResidentEvent(
                "background_noise",
                1,
                ResidentEventType.Conversation,
                "player",
                "player",
                0f,
                0.1f,
                "Unimportant noise.")), Is.False);
            Assert.That(resident.Record(new ResidentEvent(
                "broken_promise",
                2,
                ResidentEventType.PromiseBroken,
                "player",
                "player",
                -0.8f,
                1f,
                "The player broke a promise.")), Is.True);

            var restored = new ResidentSaveMapper().Restore(
                new ResidentSaveMapper().Capture(resident));

            Assert.That(restored.Memory.Events.Count, Is.EqualTo(1));
            Assert.That(restored.Memory.Events[0].EventId, Is.EqualTo("broken_promise"));
            Assert.That(restored.Relationship.PerceivedReliability,
                Is.EqualTo(resident.Relationship.PerceivedReliability));
        }

        [Test]
        public void UnsupportedSaveVersion_IsRejected()
        {
            var mapper = new ResidentSaveMapper();
            var dto = mapper.Capture(ResidentState.Create(2));
            dto.SchemaVersion = 99;

            Assert.Throws<NotSupportedException>(() => mapper.Restore(dto));
        }
    }
}
