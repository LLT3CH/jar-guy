using System;
using System.Collections.Generic;
using HumanGlassWatcher.Core.Items;

namespace HumanGlassWatcher.Core.Interactions
{
    public enum AffordanceKind
    {
        Eat,
        Drink,
        Strike,
        Cut,
        WetAndClean,
        CleanDirtySurface,
        Illuminate,
        Signal,
        Comfort,
        EscapeAttempt,
        Play
    }

    public enum EnvironmentCapability
    {
        LidSeam,
        BreakableBoundary,
        DirtySurface
    }

    public readonly struct Affordance : IEquatable<Affordance>
    {
        public Affordance(AffordanceKind kind, string primaryId, string secondaryId, string description)
        {
            Kind = kind;
            PrimaryId = primaryId;
            SecondaryId = secondaryId;
            Description = description;
        }

        public AffordanceKind Kind { get; }
        public string PrimaryId { get; }
        public string SecondaryId { get; }
        public string Description { get; }

        public bool Equals(Affordance other)
        {
            return Kind == other.Kind &&
                   PrimaryId == other.PrimaryId &&
                   SecondaryId == other.SecondaryId;
        }

        public override bool Equals(object obj)
        {
            return obj is Affordance other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)Kind;
                hashCode = (hashCode * 397) ^ (PrimaryId != null ? PrimaryId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SecondaryId != null ? SecondaryId.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public static class CapabilityAffordanceResolver
    {
        public static IReadOnlyList<Affordance> ResolvePair(ItemDefinition first, ItemDefinition second)
        {
            var results = new List<Affordance>();
            if (first == null || second == null || first.CanonicalId == second.CanonicalId)
            {
                return results;
            }

            AddSymmetric(
                results,
                first,
                second,
                ItemCapability.SwingTool,
                ItemCapability.Throwable,
                AffordanceKind.Strike,
                "Use {0} to strike {1}.");

            AddSymmetric(
                results,
                first,
                second,
                ItemCapability.SharpEdge,
                ItemCapability.FlexibleLine,
                AffordanceKind.Cut,
                "Use {0} to cut {1}.");

            AddSymmetric(
                results,
                first,
                second,
                ItemCapability.Absorbent,
                ItemCapability.Drinkable,
                AffordanceKind.WetAndClean,
                "Wet {0} using {1}, enabling cleaning.");

            AddSymmetric(
                results,
                first,
                second,
                ItemCapability.Absorbent,
                ItemCapability.Dirty,
                AffordanceKind.CleanDirtySurface,
                "Use {0} to clean up {1}; disgust still matters.");

            AddSymmetric(
                results,
                first,
                second,
                ItemCapability.Drinkable,
                ItemCapability.Dirty,
                AffordanceKind.CleanDirtySurface,
                "Use {0} as part of cleaning {1}; an absorbent tool helps.");

            return results;
        }

        public static IReadOnlyList<Affordance> ResolveSingle(
            ItemDefinition item,
            IReadOnlyCollection<EnvironmentCapability> environment)
        {
            var results = new List<Affordance>();
            if (item == null)
            {
                return results;
            }

            if (item.Has(ItemCapability.Edible))
            {
                results.Add(new Affordance(AffordanceKind.Eat, item.CanonicalId, null, $"Eat {item.DisplayName}."));
            }

            if (item.Has(ItemCapability.Drinkable))
            {
                results.Add(new Affordance(AffordanceKind.Drink, item.CanonicalId, null, $"Drink from {item.DisplayName}."));
            }

            if (item.Has(ItemCapability.LightSource))
            {
                results.Add(new Affordance(AffordanceKind.Illuminate, item.CanonicalId, null, $"Illuminate the jar with {item.DisplayName}."));
                results.Add(new Affordance(AffordanceKind.Signal, item.CanonicalId, null, $"Signal toward the watcher with {item.DisplayName}."));
            }

            if (item.Has(ItemCapability.Comfort))
            {
                results.Add(new Affordance(AffordanceKind.Comfort, item.CanonicalId, null, $"Rest with {item.DisplayName}."));
            }

            if (item.Has(ItemCapability.Entertainment) || item.Has(ItemCapability.Bouncy))
            {
                results.Add(new Affordance(AffordanceKind.Play, item.CanonicalId, null, $"Play with {item.DisplayName}."));
            }

            if ((item.Has(ItemCapability.Lever) || item.Has(ItemCapability.SwingTool)) &&
                (Contains(environment, EnvironmentCapability.LidSeam) ||
                 Contains(environment, EnvironmentCapability.BreakableBoundary)))
            {
                results.Add(new Affordance(
                    AffordanceKind.EscapeAttempt,
                    item.CanonicalId,
                    "jar_boundary",
                    $"Evaluate {item.DisplayName} for a lid or jar escape attempt."));
            }

            return results;
        }

        private static void AddSymmetric(
            ICollection<Affordance> output,
            ItemDefinition first,
            ItemDefinition second,
            ItemCapability primaryCapability,
            ItemCapability secondaryCapability,
            AffordanceKind kind,
            string description)
        {
            if (first.Has(primaryCapability) && second.Has(secondaryCapability))
            {
                output.Add(new Affordance(
                    kind,
                    first.CanonicalId,
                    second.CanonicalId,
                    string.Format(description, first.DisplayName, second.DisplayName)));
            }
            else if (second.Has(primaryCapability) && first.Has(secondaryCapability))
            {
                output.Add(new Affordance(
                    kind,
                    second.CanonicalId,
                    first.CanonicalId,
                    string.Format(description, second.DisplayName, first.DisplayName)));
            }
        }

        private static bool Contains<T>(IEnumerable<T> values, T target)
        {
            if (values == null)
            {
                return false;
            }

            var comparer = EqualityComparer<T>.Default;
            foreach (var value in values)
            {
                if (comparer.Equals(value, target))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
