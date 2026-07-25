using System;
using System.Collections.Generic;
using HumanGlassWatcher.Character.Model;

namespace HumanGlassWatcher.Character.Appraisal
{
    /// <summary>
    /// Read-only character-side projection of a gameplay-owned entity.
    /// </summary>
    public sealed class ItemObservation
    {
        private readonly HashSet<ItemCapability> capabilities;
        private readonly string[] tags;

        public ItemObservation(
            string entityId,
            string canonicalId,
            IEnumerable<ItemCapability> capabilities,
            IEnumerable<string> tags = null,
            float safetyRisk = 0f,
            float dirtiness = 0f,
            float taste = 0f,
            float comfort = 0f,
            float novelty = 0.5f)
        {
            if (!CharacterMath.IsStableId(entityId))
            {
                throw new ArgumentException("Entity ID must be a stable contract-safe ID.", nameof(entityId));
            }

            EntityId = entityId;
            CanonicalId = canonicalId ?? string.Empty;
            this.capabilities = new HashSet<ItemCapability>(
                capabilities ?? Array.Empty<ItemCapability>());
            this.tags = CharacterMath.CopyStrings(tags);
            SafetyRisk = CharacterMath.Clamp01(safetyRisk);
            Dirtiness = CharacterMath.Clamp01(dirtiness);
            Taste = CharacterMath.Clamp(taste, -1f, 1f);
            Comfort = CharacterMath.Clamp01(comfort);
            Novelty = CharacterMath.Clamp01(novelty);
        }

        public string EntityId { get; }
        public string CanonicalId { get; }
        public IReadOnlyCollection<ItemCapability> Capabilities => capabilities;
        public IReadOnlyList<string> Tags => tags;
        public float SafetyRisk { get; }
        public float Dirtiness { get; }
        public float Taste { get; }
        public float Comfort { get; }
        public float Novelty { get; }

        public bool Has(ItemCapability capability)
        {
            return capabilities.Contains(capability);
        }
    }
}
