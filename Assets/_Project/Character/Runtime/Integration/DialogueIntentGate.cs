using System;
using System.Collections.Generic;
using HumanGlassWatcher.Character.Model;
using HumanGlassWatcher.Character.Planning;

namespace HumanGlassWatcher.Character.Integration
{
    public sealed class IntentValidationResult
    {
        public IntentValidationResult(bool accepted, string rejectionCode, ActionRequest requestedAction)
        {
            Accepted = accepted;
            RejectionCode = rejectionCode ?? string.Empty;
            RequestedAction = requestedAction;
        }

        public bool Accepted { get; }
        public string RejectionCode { get; }
        public ActionRequest RequestedAction { get; }
    }

    public sealed class DialogueIntentGate
    {
        public const int SupportedContractVersion = 1;

        public IntentValidationResult Validate(
            BrainTurn turn,
            IEnumerable<LegalActionOffer> currentLegalOffers)
        {
            var offers = new List<LegalActionOffer>(
                currentLegalOffers ?? new LegalActionOffer[0]);

            if (turn == null)
            {
                return Reject("missing_turn", offers);
            }

            if (turn.ContractVersion != SupportedContractVersion)
            {
                return Reject("unsupported_contract_version", offers);
            }

            if (!CharacterMath.IsStableId(turn.TurnId))
            {
                return Reject("invalid_turn_id", offers);
            }

            if (turn.SpokenLine.Length > 500 || turn.MemoryNote.Length > 240)
            {
                return Reject("invalid_text_length", offers);
            }

            if (string.IsNullOrEmpty(turn.SelectedActionId))
            {
                return Reject("no_intent", offers);
            }

            for (var index = 0; index < offers.Count; index++)
            {
                var offer = offers[index];
                if (string.Equals(offer.ActionId, turn.SelectedActionId, StringComparison.Ordinal))
                {
                    if (turn.SelectedIntent != offer.Verb)
                    {
                        return Reject("intent_verb_mismatch", offers);
                    }

                    if (!TargetsMatch(turn.TargetEntityIds, offer.TargetEntityIds))
                    {
                        return Reject("intent_targets_mismatch", offers);
                    }

                    // Targets and verb come only from the locally supplied offer.
                    return new IntentValidationResult(
                        true,
                        string.Empty,
                        new ActionRequest(offer, ActionRequestSource.ValidatedServiceIntent));
                }
            }

            return Reject("intent_not_offered", offers);
        }

        private static bool TargetsMatch(
            IReadOnlyList<string> serviceTargets,
            IReadOnlyList<string> offeredTargets)
        {
            if (serviceTargets == null || serviceTargets.Count != offeredTargets.Count)
            {
                return false;
            }

            for (var index = 0; index < offeredTargets.Count; index++)
            {
                if (!string.Equals(serviceTargets[index], offeredTargets[index], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private static IntentValidationResult Reject(
            string rejectionCode,
            IReadOnlyList<LegalActionOffer> offers)
        {
            for (var index = 0; index < offers.Count; index++)
            {
                if (offers[index].Verb == ActionVerb.Observe)
                {
                    return new IntentValidationResult(
                        false,
                        rejectionCode,
                        new ActionRequest(offers[index], ActionRequestSource.SafetyFallback));
                }
            }

            return new IntentValidationResult(
                false,
                rejectionCode,
                ActionRequest.ObserveFallback());
        }
    }
}
