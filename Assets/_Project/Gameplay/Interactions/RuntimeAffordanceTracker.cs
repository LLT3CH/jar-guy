using System;
using System.Collections.Generic;
using System.Linq;
using HumanGlassWatcher.Core.Interactions;
using HumanGlassWatcher.Core.Items;
using HumanGlassWatcher.Gameplay.Items;
using UnityEngine;

namespace HumanGlassWatcher.Gameplay.Interactions
{
    public sealed class RuntimeAffordanceTracker : MonoBehaviour
    {
        private static readonly EnvironmentCapability[] JarCapabilities =
        {
            EnvironmentCapability.LidSeam,
            EnvironmentCapability.BreakableBoundary,
            EnvironmentCapability.DirtySurface
        };

        private readonly List<ItemDefinition> items = new List<ItemDefinition>();
        private readonly List<Affordance> available = new List<Affordance>();

        public event Action<IReadOnlyList<Affordance>> Changed;

        public IReadOnlyList<Affordance> Available => available;

        public void Observe(SpawnedItem item)
        {
            if (item == null || item.Definition == null)
            {
                return;
            }

            foreach (var existing in items)
            {
                available.AddRange(CapabilityAffordanceResolver.ResolvePair(existing, item.Definition));
            }

            items.Add(item.Definition);
            available.AddRange(CapabilityAffordanceResolver.ResolveSingle(item.Definition, JarCapabilities));
            RemoveDuplicates();
            Changed?.Invoke(available);
        }

        private void RemoveDuplicates()
        {
            var unique = available.Distinct().ToArray();
            available.Clear();
            available.AddRange(unique);
        }
    }
}
