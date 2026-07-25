using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HumanGlassWatcher.Core.Items;
using HumanGlassWatcher.Core.Services;
using UnityEngine;

namespace HumanGlassWatcher.Gameplay.Items
{
    public sealed class LocalItemCatalog : IItemResolver
    {
        private const int MaxPromptLength = 160;

        private readonly Dictionary<string, ItemDefinition> definitions;
        private readonly Dictionary<string, string> aliases;
        private readonly HashSet<string> resolvedIds = new HashSet<string>(StringComparer.Ordinal);

        public LocalItemCatalog()
        {
            definitions = BuildDefinitions().ToDictionary(item => item.CanonicalId, StringComparer.Ordinal);
            aliases = BuildAliases();
        }

        public IReadOnlyCollection<ItemDefinition> Definitions => definitions.Values;

        public Task<ItemResolution> ResolveAsync(string prompt, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var normalized = Normalize(prompt);

            if (string.IsNullOrEmpty(normalized))
            {
                return Result(ItemResolutionStatus.Empty, null, "Name an item first.");
            }

            if (prompt.Length > MaxPromptLength || ContainsUnsupportedCharacters(prompt))
            {
                return Result(ItemResolutionStatus.Unsupported, null, "That prompt cannot be represented safely.");
            }

            if (IsUnsafe(normalized))
            {
                return Result(ItemResolutionStatus.Unsafe, null, "That request is outside this playable slice.");
            }

            var canonicalId = aliases.TryGetValue(normalized, out var aliasTarget)
                ? aliasTarget
                : ToCanonicalId(normalized);

            ItemResolutionStatus status;
            ItemDefinition definition;
            if (definitions.TryGetValue(canonicalId, out var known))
            {
                definition = known;
                status = ItemResolutionStatus.Resolved;
            }
            else
            {
                definition = CreateUnknown(canonicalId, normalized);
                status = ItemResolutionStatus.UnknownFallback;
            }

            if (!resolvedIds.Add(definition.CanonicalId))
            {
                return Result(
                    ItemResolutionStatus.Duplicate,
                    null,
                    $"{definition.DisplayName} is already in this demo jar.");
            }

            var feedback = status == ItemResolutionStatus.Resolved
                ? $"Dropping {definition.DisplayName}."
                : $"No authored asset yet: dropping a safe idea-object for “{definition.DisplayName}”.";
            return Result(status, definition, feedback);
        }

        public bool TryGet(string canonicalId, out ItemDefinition definition)
        {
            return definitions.TryGetValue(canonicalId, out definition);
        }

        public void ForgetResolvedItems()
        {
            resolvedIds.Clear();
        }

        public static string Normalize(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(prompt.Length);
            var previousWasSpace = false;
            foreach (var rawCharacter in prompt.Trim().ToLower(CultureInfo.InvariantCulture))
            {
                var character = char.IsLetterOrDigit(rawCharacter) ? rawCharacter : ' ';
                if (character == ' ')
                {
                    if (previousWasSpace)
                    {
                        continue;
                    }

                    previousWasSpace = true;
                }
                else
                {
                    previousWasSpace = false;
                }

                builder.Append(character);
            }

            return builder.ToString().Trim();
        }

        private static Task<ItemResolution> Result(
            ItemResolutionStatus status,
            ItemDefinition definition,
            string feedback)
        {
            return Task.FromResult(new ItemResolution(status, definition, feedback));
        }

        private static bool ContainsUnsupportedCharacters(string prompt)
        {
            return prompt.Any(character => char.IsControl(character) && !char.IsWhiteSpace(character));
        }

        private static bool IsUnsafe(string normalized)
        {
            var deniedFragments = new[]
            {
                "sexual violence",
                "child porn",
                "suicide instructions",
                "how to build a bomb",
                "real person address"
            };
            return deniedFragments.Any(normalized.Contains);
        }

        private static string ToCanonicalId(string normalized)
        {
            var canonical = normalized.Replace(' ', '_');
            if (canonical.Length > 48)
            {
                canonical = canonical.Substring(0, 48).TrimEnd('_');
            }

            return string.IsNullOrEmpty(canonical) ? "unknown_item" : canonical;
        }

        private static ItemDefinition CreateUnknown(string canonicalId, string normalized)
        {
            var displayName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(normalized);
            return new ItemDefinition(
                canonicalId,
                displayName,
                VisualArchetype.IdeaObject,
                new Color(0.68f, 0.42f, 0.92f),
                new Vector3(0.7f, 0.7f, 0.7f),
                0.25f,
                0.15f,
                ItemCapability.Grabbable,
                ItemCapability.Throwable);
        }

        private static Dictionary<string, string> BuildAliases()
        {
            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["apple"] = "apple",
                ["red apple"] = "apple",
                ["cake"] = "chocolate_cake",
                ["chocolate cake"] = "chocolate_cake",
                ["water"] = "water_bottle",
                ["water bottle"] = "water_bottle",
                ["dog feces"] = "dog_feces",
                ["dog poop"] = "dog_feces",
                ["dog poo"] = "dog_feces",
                ["dog shit"] = "dog_feces",
                ["poop"] = "dog_feces",
                ["rubber ball"] = "rubber_ball",
                ["ball"] = "rubber_ball",
                ["baseball bat"] = "baseball_bat",
                ["bat"] = "baseball_bat",
                ["hockey stick"] = "hockey_stick",
                ["blanket"] = "blanket",
                ["rope"] = "rope",
                ["scissors"] = "scissors",
                ["sponge"] = "sponge",
                ["flashlight"] = "flashlight",
                ["torch"] = "flashlight"
            };
        }

        private static IEnumerable<ItemDefinition> BuildDefinitions()
        {
            yield return new ItemDefinition(
                "apple", "Apple", VisualArchetype.Food, new Color(0.78f, 0.08f, 0.08f),
                new Vector3(0.62f, 0.62f, 0.62f), 0.18f, 0.08f,
                ItemCapability.Grabbable, ItemCapability.Throwable, ItemCapability.Edible);
            yield return new ItemDefinition(
                "chocolate_cake", "Chocolate Cake", VisualArchetype.Food, new Color(0.28f, 0.10f, 0.04f),
                new Vector3(0.9f, 0.45f, 0.9f), 0.55f, 0.02f,
                ItemCapability.Grabbable, ItemCapability.Edible, ItemCapability.Comfort);
            yield return new ItemDefinition(
                "water_bottle", "Water Bottle", VisualArchetype.Bottle, new Color(0.18f, 0.65f, 0.95f),
                new Vector3(0.42f, 1.05f, 0.42f), 0.6f, 0.05f,
                ItemCapability.Grabbable, ItemCapability.Throwable, ItemCapability.Drinkable,
                ItemCapability.Container, ItemCapability.CleaningAgent);
            yield return new ItemDefinition(
                "dog_feces", "Dog Feces", VisualArchetype.Organic, new Color(0.25f, 0.10f, 0.035f),
                new Vector3(0.72f, 0.48f, 0.64f), 0.22f, 0f,
                ItemCapability.Dirty, ItemCapability.Toxic, ItemCapability.Throwable);
            yield return new ItemDefinition(
                "rubber_ball", "Rubber Ball", VisualArchetype.Sphere, new Color(0.96f, 0.52f, 0.06f),
                new Vector3(0.68f, 0.68f, 0.68f), 0.15f, 0.85f,
                ItemCapability.Grabbable, ItemCapability.Throwable, ItemCapability.Bouncy,
                ItemCapability.Entertainment);
            yield return new ItemDefinition(
                "baseball_bat", "Baseball Bat", VisualArchetype.Tool, new Color(0.64f, 0.35f, 0.12f),
                new Vector3(0.36f, 1.65f, 0.36f), 0.9f, 0.05f,
                ItemCapability.Grabbable, ItemCapability.SwingTool, ItemCapability.Lever);
            yield return new ItemDefinition(
                "hockey_stick", "Hockey Stick", VisualArchetype.Tool, new Color(0.72f, 0.78f, 0.86f),
                new Vector3(0.3f, 1.7f, 0.3f), 0.8f, 0.04f,
                ItemCapability.Grabbable, ItemCapability.SwingTool, ItemCapability.Lever);
            yield return new ItemDefinition(
                "blanket", "Blanket", VisualArchetype.Cloth, new Color(0.16f, 0.55f, 0.74f),
                new Vector3(1.35f, 0.2f, 1.05f), 0.7f, 0f,
                ItemCapability.Grabbable, ItemCapability.Comfort, ItemCapability.Wearable);
            yield return new ItemDefinition(
                "rope", "Rope", VisualArchetype.Cylinder, new Color(0.68f, 0.52f, 0.27f),
                new Vector3(0.22f, 1.45f, 0.22f), 0.35f, 0.02f,
                ItemCapability.Grabbable, ItemCapability.FlexibleLine);
            yield return new ItemDefinition(
                "scissors", "Scissors", VisualArchetype.Tool, new Color(0.68f, 0.72f, 0.78f),
                new Vector3(0.56f, 0.18f, 0.92f), 0.25f, 0.02f,
                ItemCapability.Grabbable, ItemCapability.SharpEdge);
            yield return new ItemDefinition(
                "sponge", "Sponge", VisualArchetype.Box, new Color(0.98f, 0.84f, 0.08f),
                new Vector3(0.72f, 0.34f, 0.52f), 0.08f, 0.02f,
                ItemCapability.Grabbable, ItemCapability.Absorbent);
            yield return new ItemDefinition(
                "flashlight", "Flashlight", VisualArchetype.Cylinder, new Color(0.12f, 0.16f, 0.22f),
                new Vector3(0.42f, 0.95f, 0.42f), 0.4f, 0.03f,
                ItemCapability.Grabbable, ItemCapability.LightSource, ItemCapability.Entertainment);
        }
    }
}
