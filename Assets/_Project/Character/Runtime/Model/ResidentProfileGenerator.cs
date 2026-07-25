using System.Collections.Generic;
using HumanGlassWatcher.Character.Determinism;

namespace HumanGlassWatcher.Character.Model
{
    public static class ResidentProfileGenerator
    {
        private static readonly string[] LikedItemPool =
        {
            "apple", "chocolate_cake", "water_bottle", "rubber_ball", "blanket", "flashlight"
        };

        private static readonly string[] DislikedItemPool =
        {
            "dog_feces", "scissors", "rope", "baseball_bat", "hockey_stick"
        };

        private static readonly string[] LikedTagPool =
        {
            "food", "sweet", "play", "soft", "clean", "bright", "novel", "quiet"
        };

        private static readonly string[] DislikedTagPool =
        {
            "gross", "dirty", "toxic", "loud", "sharp", "threatening", "stale"
        };

        private static readonly string[] VoicePool = { "alto_clear", "mid_wry", "low_warm", "bright_quick" };
        private static readonly string[] StylePool = { "direct", "thoughtful", "playful", "guarded", "dry" };

        public static ResidentProfile Generate(int seed)
        {
            var random = new DeterministicRandom(seed);
            var traits = new PersonalityTraits
            {
                Optimism = random.NextUnit(),
                Patience = random.NextUnit(),
                Warmth = random.NextUnit(),
                AngerTendency = random.NextUnit(),
                Humor = random.NextUnit(),
                Curiosity = random.NextUnit(),
                Caution = random.NextUnit(),
                Resourcefulness = random.NextUnit(),
                Honesty = random.NextUnit(),
                Impulsiveness = random.NextUnit(),
                Trustfulness = random.NextUnit(),
                Attachment = random.NextUnit(),
                Defiance = random.NextUnit(),
                DesireForCompany = random.NextUnit(),
                FreedomValue = random.NextUnit(),
                ComfortValue = random.NextUnit(),
                CleanlinessValue = random.NextUnit(),
                NoveltyValue = random.NextUnit(),
                SafetyValue = random.NextUnit(),
                FairnessValue = random.NextUnit()
            };

            var preferences = new ResidentPreferences(
                PickDistinct(random, LikedItemPool, 3),
                PickDistinct(random, DislikedItemPool, 2),
                PickDistinct(random, LikedTagPool, 3),
                PickDistinct(random, DislikedTagPool, 3));

            return new ResidentProfile(
                seed,
                traits,
                preferences,
                VoicePool[random.Range(0, VoicePool.Length)],
                StylePool[random.Range(0, StylePool.Length)]);
        }

        private static IEnumerable<string> PickDistinct(
            DeterministicRandom random,
            IReadOnlyList<string> source,
            int count)
        {
            var indices = new List<int>();
            for (var index = 0; index < source.Count; index++)
            {
                indices.Add(index);
            }

            var result = new List<string>();
            while (result.Count < count && indices.Count > 0)
            {
                var selected = random.Range(0, indices.Count);
                result.Add(source[indices[selected]]);
                indices.RemoveAt(selected);
            }

            return result;
        }
    }
}
