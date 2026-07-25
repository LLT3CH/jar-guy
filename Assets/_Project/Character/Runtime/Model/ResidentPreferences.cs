using System;
using System.Collections.Generic;

namespace HumanGlassWatcher.Character.Model
{
    [Serializable]
    public sealed class ResidentPreferences
    {
        private readonly HashSet<string> likedItemIds;
        private readonly HashSet<string> dislikedItemIds;
        private readonly HashSet<string> likedTags;
        private readonly HashSet<string> dislikedTags;

        public ResidentPreferences(
            IEnumerable<string> likedItemIds,
            IEnumerable<string> dislikedItemIds,
            IEnumerable<string> likedTags,
            IEnumerable<string> dislikedTags)
        {
            this.likedItemIds = CopySet(likedItemIds);
            this.dislikedItemIds = CopySet(dislikedItemIds);
            this.likedTags = CopySet(likedTags);
            this.dislikedTags = CopySet(dislikedTags);
        }

        public IReadOnlyCollection<string> LikedItemIds => likedItemIds;
        public IReadOnlyCollection<string> DislikedItemIds => dislikedItemIds;
        public IReadOnlyCollection<string> LikedTags => likedTags;
        public IReadOnlyCollection<string> DislikedTags => dislikedTags;

        public float Score(string canonicalItemId, IEnumerable<string> tags)
        {
            var score = 0f;
            if (likedItemIds.Contains(canonicalItemId))
            {
                score += 15f;
            }

            if (dislikedItemIds.Contains(canonicalItemId))
            {
                score -= 20f;
            }

            if (tags == null)
            {
                return score;
            }

            foreach (var tag in tags)
            {
                if (likedTags.Contains(tag))
                {
                    score += 6f;
                }

                if (dislikedTags.Contains(tag))
                {
                    score -= 8f;
                }
            }

            return CharacterMath.Clamp(score, -32f, 27f);
        }

        private static HashSet<string> CopySet(IEnumerable<string> values)
        {
            return new HashSet<string>(
                CharacterMath.CopyStrings(values),
                StringComparer.Ordinal);
        }
    }
}
