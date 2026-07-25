using System;
using System.Collections.Generic;
using UnityEngine;

namespace HumanGlassWatcher.Core.Items
{
    [Serializable]
    public sealed class ItemDefinition
    {
        [SerializeField] private string canonicalId;
        [SerializeField] private string displayName;
        [SerializeField] private VisualArchetype archetype;
        [SerializeField] private Color color;
        [SerializeField] private Vector3 scale;
        [SerializeField] private float massKg;
        [SerializeField] private float bounciness;
        [SerializeField] private ItemCapability[] capabilities;

        public ItemDefinition(
            string canonicalId,
            string displayName,
            VisualArchetype archetype,
            Color color,
            Vector3 scale,
            float massKg,
            float bounciness,
            params ItemCapability[] capabilities)
        {
            this.canonicalId = canonicalId ?? throw new ArgumentNullException(nameof(canonicalId));
            this.displayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            this.archetype = archetype;
            this.color = color;
            this.scale = scale;
            this.massKg = Mathf.Clamp(massKg, 0.001f, 50f);
            this.bounciness = Mathf.Clamp01(bounciness);
            this.capabilities = capabilities ?? Array.Empty<ItemCapability>();
        }

        public string CanonicalId => canonicalId;
        public string DisplayName => displayName;
        public VisualArchetype Archetype => archetype;
        public Color Color => color;
        public Vector3 Scale => scale;
        public float MassKg => massKg;
        public float Bounciness => bounciness;
        public IReadOnlyList<ItemCapability> Capabilities => capabilities;

        public bool Has(ItemCapability capability)
        {
            return Array.IndexOf(capabilities, capability) >= 0;
        }
    }
}
