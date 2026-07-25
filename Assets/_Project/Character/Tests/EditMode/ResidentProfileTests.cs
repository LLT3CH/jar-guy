using System.Linq;
using HumanGlassWatcher.Character.Model;
using NUnit.Framework;

namespace HumanGlassWatcher.Character.Tests
{
    public sealed class ResidentProfileTests
    {
        [Test]
        public void IdenticalSeeds_ReproduceEveryProfileField()
        {
            var first = ResidentProfileGenerator.Generate(74291);
            var second = ResidentProfileGenerator.Generate(74291);

            Assert.That(second.Seed, Is.EqualTo(first.Seed));
            Assert.That(second.VoiceIdentity, Is.EqualTo(first.VoiceIdentity));
            Assert.That(second.ConversationStyle, Is.EqualTo(first.ConversationStyle));
            AssertTraitsEqual(first.Traits, second.Traits);
            CollectionAssert.AreEquivalent(first.Preferences.LikedItemIds, second.Preferences.LikedItemIds);
            CollectionAssert.AreEquivalent(first.Preferences.DislikedItemIds, second.Preferences.DislikedItemIds);
            CollectionAssert.AreEquivalent(first.Preferences.LikedTags, second.Preferences.LikedTags);
            CollectionAssert.AreEquivalent(first.Preferences.DislikedTags, second.Preferences.DislikedTags);
        }

        [Test]
        public void DifferentSeeds_AreObservablyDistinct()
        {
            var first = ResidentProfileGenerator.Generate(1001);
            var second = ResidentProfileGenerator.Generate(1002);
            var traitDifferences = new[]
            {
                first.Traits.Optimism != second.Traits.Optimism,
                first.Traits.Curiosity != second.Traits.Curiosity,
                first.Traits.Resourcefulness != second.Traits.Resourcefulness,
                first.Traits.FreedomValue != second.Traits.FreedomValue,
                first.Traits.CleanlinessValue != second.Traits.CleanlinessValue
            }.Count(different => different);

            Assert.That(traitDifferences, Is.GreaterThanOrEqualTo(4));
            Assert.That(
                first.Preferences.LikedItemIds.SequenceEqual(second.Preferences.LikedItemIds) &&
                first.Preferences.DislikedTags.SequenceEqual(second.Preferences.DislikedTags),
                Is.False);
        }

        private static void AssertTraitsEqual(PersonalityTraits expected, PersonalityTraits actual)
        {
            Assert.That(actual.Optimism, Is.EqualTo(expected.Optimism));
            Assert.That(actual.Patience, Is.EqualTo(expected.Patience));
            Assert.That(actual.Warmth, Is.EqualTo(expected.Warmth));
            Assert.That(actual.AngerTendency, Is.EqualTo(expected.AngerTendency));
            Assert.That(actual.Humor, Is.EqualTo(expected.Humor));
            Assert.That(actual.Curiosity, Is.EqualTo(expected.Curiosity));
            Assert.That(actual.Caution, Is.EqualTo(expected.Caution));
            Assert.That(actual.Resourcefulness, Is.EqualTo(expected.Resourcefulness));
            Assert.That(actual.Honesty, Is.EqualTo(expected.Honesty));
            Assert.That(actual.Impulsiveness, Is.EqualTo(expected.Impulsiveness));
            Assert.That(actual.Trustfulness, Is.EqualTo(expected.Trustfulness));
            Assert.That(actual.Attachment, Is.EqualTo(expected.Attachment));
            Assert.That(actual.Defiance, Is.EqualTo(expected.Defiance));
            Assert.That(actual.DesireForCompany, Is.EqualTo(expected.DesireForCompany));
            Assert.That(actual.FreedomValue, Is.EqualTo(expected.FreedomValue));
            Assert.That(actual.ComfortValue, Is.EqualTo(expected.ComfortValue));
            Assert.That(actual.CleanlinessValue, Is.EqualTo(expected.CleanlinessValue));
            Assert.That(actual.NoveltyValue, Is.EqualTo(expected.NoveltyValue));
            Assert.That(actual.SafetyValue, Is.EqualTo(expected.SafetyValue));
            Assert.That(actual.FairnessValue, Is.EqualTo(expected.FairnessValue));
        }
    }
}
