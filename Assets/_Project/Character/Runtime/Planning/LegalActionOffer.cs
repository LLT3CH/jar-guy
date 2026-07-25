using System;
using System.Collections.Generic;
using HumanGlassWatcher.Character.Model;

namespace HumanGlassWatcher.Character.Planning
{
    /// <summary>
    /// Mirrors contracts/v1/action-offer.schema.json without taking execution authority.
    /// Gameplay remains responsible for constructing and validating these offers.
    /// </summary>
    public sealed class LegalActionOffer
    {
        private readonly string[] targetEntityIds;

        public LegalActionOffer(
            string actionId,
            ActionVerb verb,
            IEnumerable<string> targetEntityIds,
            float utilityHint,
            string reasonCode)
        {
            if (!CharacterMath.IsStableId(actionId))
            {
                throw new ArgumentException("Action ID must be a stable contract-safe ID.", nameof(actionId));
            }

            if (!IsReasonCode(reasonCode))
            {
                throw new ArgumentException("Reason code must use lower snake case.", nameof(reasonCode));
            }

            var targets = CharacterMath.CopyStrings(targetEntityIds);
            if (targets.Length > 3)
            {
                throw new ArgumentException("An action offer can target at most three entities.", nameof(targetEntityIds));
            }

            var unique = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < targets.Length; index++)
            {
                if (!CharacterMath.IsStableId(targets[index]) || !unique.Add(targets[index]))
                {
                    throw new ArgumentException("Target entity IDs must be unique stable IDs.", nameof(targetEntityIds));
                }
            }

            ActionId = actionId;
            Verb = verb;
            this.targetEntityIds = targets;
            UtilityHint = CharacterMath.Clamp(utilityHint, -100f, 100f);
            ReasonCode = reasonCode;
        }

        public string ActionId { get; }
        public ActionVerb Verb { get; }
        public IReadOnlyList<string> TargetEntityIds => targetEntityIds;
        public float UtilityHint { get; }
        public string ReasonCode { get; }

        private static bool IsReasonCode(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length > 64 || value[0] < 'a' || value[0] > 'z')
            {
                return false;
            }

            for (var index = 1; index < value.Length; index++)
            {
                var character = value[index];
                var lowercaseLetter = character >= 'a' && character <= 'z';
                if (!lowercaseLetter && !char.IsDigit(character) && character != '_')
                {
                    return false;
                }
            }

            return true;
        }
    }

    public sealed class ScoredAction
    {
        public ScoredAction(LegalActionOffer offer, float utility)
        {
            Offer = offer;
            Utility = utility;
        }

        public LegalActionOffer Offer { get; }
        public float Utility { get; }
    }

    public enum ActionRequestSource
    {
        LocalUtility,
        ValidatedServiceIntent,
        SafetyFallback
    }

    public sealed class ActionRequest
    {
        private readonly string[] targetEntityIds;

        public ActionRequest(LegalActionOffer offer, ActionRequestSource source)
        {
            OfferId = offer.ActionId;
            Verb = offer.Verb;
            targetEntityIds = CharacterMath.CopyStrings(offer.TargetEntityIds);
            Source = source;
        }

        private ActionRequest()
        {
            OfferId = "observe_fallback";
            Verb = ActionVerb.Observe;
            targetEntityIds = Array.Empty<string>();
            Source = ActionRequestSource.SafetyFallback;
        }

        public string OfferId { get; }
        public ActionVerb Verb { get; }
        public IReadOnlyList<string> TargetEntityIds => targetEntityIds;
        public ActionRequestSource Source { get; }

        public static ActionRequest ObserveFallback()
        {
            return new ActionRequest();
        }
    }
}
