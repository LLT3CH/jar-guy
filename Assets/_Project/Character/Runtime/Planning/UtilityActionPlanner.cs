using System;
using System.Collections.Generic;
using HumanGlassWatcher.Character.Appraisal;
using HumanGlassWatcher.Character.Model;

namespace HumanGlassWatcher.Character.Planning
{
    public sealed class UtilityActionPlanner
    {
        public IReadOnlyList<ScoredAction> Rank(
            ResidentState resident,
            IEnumerable<LegalActionOffer> offers,
            IEnumerable<ItemObservation> observations)
        {
            if (resident == null)
            {
                throw new ArgumentNullException(nameof(resident));
            }

            var itemByEntity = new Dictionary<string, ItemObservation>(StringComparer.Ordinal);
            if (observations != null)
            {
                foreach (var observation in observations)
                {
                    if (observation != null)
                    {
                        itemByEntity[observation.EntityId] = observation;
                    }
                }
            }

            var ranked = new List<ScoredAction>();
            if (offers != null)
            {
                foreach (var offer in offers)
                {
                    if (offer != null)
                    {
                        ranked.Add(new ScoredAction(offer, Score(resident, offer, itemByEntity)));
                    }
                }
            }

            ranked.Sort(Compare);
            return ranked;
        }

        public ActionRequest Select(
            ResidentState resident,
            IEnumerable<LegalActionOffer> offers,
            IEnumerable<ItemObservation> observations)
        {
            var ranked = Rank(resident, offers, observations);
            return ranked.Count == 0
                ? ActionRequest.ObserveFallback()
                : new ActionRequest(ranked[0].Offer, ActionRequestSource.LocalUtility);
        }

        private static int Compare(ScoredAction left, ScoredAction right)
        {
            var utilityOrder = right.Utility.CompareTo(left.Utility);
            return utilityOrder != 0
                ? utilityOrder
                : string.CompareOrdinal(left.Offer.ActionId, right.Offer.ActionId);
        }

        private static float Score(
            ResidentState resident,
            LegalActionOffer offer,
            IReadOnlyDictionary<string, ItemObservation> itemByEntity)
        {
            var traits = resident.Profile.Traits;
            var needs = resident.Needs;
            var relationship = resident.Relationship;
            var items = ResolveItems(offer.TargetEntityIds, itemByEntity);
            var preference = PreferenceScore(resident, items);
            var risk = Maximum(items, item => item.SafetyRisk);
            var dirtiness = Maximum(items, item => item.Dirtiness);
            var novelty = Maximum(items, item => item.Novelty);
            var memoryAversion = resident.Memory.AversionTo(offer.TargetEntityIds);
            var utility = offer.UtilityHint * 0.1f;

            switch (offer.Verb)
            {
                case ActionVerb.Observe:
                    utility += 8f + (traits.Curiosity * 12f) + (novelty * traits.NoveltyValue * 12f);
                    break;
                case ActionVerb.Approach:
                    utility += 10f + (traits.Curiosity * 18f) + preference;
                    utility -= (risk * (25f + (traits.Caution * 25f))) + memoryAversion;
                    break;
                case ActionVerb.Avoid:
                    utility += (needs.Safety * 28f) + (risk * (38f + (traits.Caution * 22f)));
                    utility += dirtiness * (20f + (needs.Hygiene * 30f) + (traits.CleanlinessValue * 20f));
                    utility += memoryAversion * 0.8f;
                    break;
                case ActionVerb.Grab:
                    utility += 8f + (traits.Curiosity * 15f) + (traits.Resourcefulness * 10f) + preference;
                    utility -= (risk * (20f + (traits.Caution * 28f))) + memoryAversion;
                    break;
                case ActionVerb.Eat:
                    utility += (needs.Hunger * 78f) + preference + (Average(items, item => item.Taste) * 18f);
                    utility -= (risk * (45f + (traits.Caution * 35f))) + memoryAversion;
                    break;
                case ActionVerb.Drink:
                    utility += (needs.Thirst * 82f) + preference;
                    utility -= (risk * (45f + (traits.Caution * 35f))) + memoryAversion;
                    break;
                case ActionVerb.Throw:
                    utility += (needs.Stimulation * 28f) + (traits.Impulsiveness * 16f) + preference;
                    utility -= (risk * traits.Caution * 25f) + memoryAversion;
                    break;
                case ActionVerb.Strike:
                    utility += (needs.Stimulation * 55f) + (traits.Humor * 12f) +
                               (traits.Resourcefulness * 12f) + preference;
                    utility -= (risk * traits.Caution * 22f) + memoryAversion;
                    if (!Has(items, ItemCapability.SwingTool) ||
                        (!Has(items, ItemCapability.Bouncy) && !Has(items, ItemCapability.Throwable)))
                    {
                        utility = -1000f;
                    }
                    break;
                case ActionVerb.Cut:
                    utility += (traits.Resourcefulness * 35f) + (needs.Freedom * traits.FreedomValue * 25f);
                    utility -= risk * traits.Caution * 25f;
                    break;
                case ActionVerb.Clean:
                    utility += (needs.Hygiene * 72f) + (traits.CleanlinessValue * 25f) + (dirtiness * 25f);
                    utility -= memoryAversion * 0.25f;
                    break;
                case ActionVerb.Wear:
                    utility += (needs.Comfort * 48f) + (traits.ComfortValue * 24f) + preference;
                    utility -= memoryAversion;
                    break;
                case ActionVerb.Rest:
                    utility += (needs.Energy * 78f) + (needs.Comfort * 28f) +
                               (traits.ComfortValue * 22f) + (Average(items, item => item.Comfort) * 18f);
                    break;
                case ActionVerb.Play:
                    utility += (needs.Stimulation * 72f) + (traits.Humor * 15f) +
                               (traits.Curiosity * 10f) + preference;
                    utility -= memoryAversion;
                    break;
                case ActionVerb.Signal:
                    utility += (needs.SocialConnection * 30f) + (needs.Freedom * 25f) +
                               (traits.Resourcefulness * 15f);
                    break;
                case ActionVerb.AttemptEscape:
                    utility += (needs.Freedom * 82f) + (traits.FreedomValue * 34f) +
                               (traits.Resourcefulness * 38f) + (traits.Defiance * 16f);
                    utility += (relationship.Fear * 18f) + (relationship.Resentment * 20f);
                    utility -= (traits.Caution * risk * 36f);
                    utility -= (traits.ComfortValue * (1f - needs.Comfort) * 28f);
                    utility -= (relationship.Trust * 10f) + (traits.Attachment * 10f) +
                               (relationship.Dependency * 8f);
                    break;
                case ActionVerb.Speak:
                    utility += (needs.SocialConnection * 58f) + (traits.DesireForCompany * 20f) +
                               (traits.Warmth * 10f) + (relationship.Trust * 10f);
                    utility -= relationship.Fear * 12f;
                    break;
            }

            if (offer.Verb == ActionVerb.Play ||
                offer.Verb == ActionVerb.Speak ||
                offer.Verb == ActionVerb.Approach)
            {
                utility += resident.Mood.Valence * 8f;
            }

            return utility;
        }

        private static List<ItemObservation> ResolveItems(
            IReadOnlyList<string> targetIds,
            IReadOnlyDictionary<string, ItemObservation> itemByEntity)
        {
            var items = new List<ItemObservation>();
            for (var index = 0; index < targetIds.Count; index++)
            {
                if (itemByEntity.TryGetValue(targetIds[index], out var item))
                {
                    items.Add(item);
                }
            }

            return items;
        }

        private static float PreferenceScore(ResidentState resident, IEnumerable<ItemObservation> items)
        {
            var score = 0f;
            foreach (var item in items)
            {
                score += resident.Profile.Preferences.Score(item.CanonicalId, item.Tags);
            }

            return CharacterMath.Clamp(score, -40f, 35f);
        }

        private static bool Has(IEnumerable<ItemObservation> items, ItemCapability capability)
        {
            foreach (var item in items)
            {
                if (item.Has(capability))
                {
                    return true;
                }
            }

            return false;
        }

        private static float Maximum(
            IEnumerable<ItemObservation> items,
            Func<ItemObservation, float> selector)
        {
            var maximum = 0f;
            foreach (var item in items)
            {
                maximum = Math.Max(maximum, selector(item));
            }

            return maximum;
        }

        private static float Average(
            IReadOnlyCollection<ItemObservation> items,
            Func<ItemObservation, float> selector)
        {
            if (items.Count == 0)
            {
                return 0f;
            }

            var total = 0f;
            foreach (var item in items)
            {
                total += selector(item);
            }

            return total / items.Count;
        }
    }
}
