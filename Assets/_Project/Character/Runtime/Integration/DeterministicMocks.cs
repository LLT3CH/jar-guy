using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HumanGlassWatcher.Character.Model;
using HumanGlassWatcher.Character.Planning;

namespace HumanGlassWatcher.Character.Integration
{
    public sealed class DeterministicPerceptionMock : ICharacterPerceptionPort
    {
        public DeterministicPerceptionMock(CharacterPerceptionSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public CharacterPerceptionSnapshot Snapshot { get; set; }

        public CharacterPerceptionSnapshot Capture()
        {
            return Snapshot;
        }
    }

    public sealed class RecordingAnimationMock : ICharacterAnimationPort
    {
        private readonly List<ActionRequest> previews = new List<ActionRequest>();
        public IReadOnlyList<ActionRequest> Previews => previews;

        public void Preview(ActionRequest requestedAction)
        {
            previews.Add(requestedAction);
        }
    }

    public sealed class RecordingSpeechMock : ICharacterSpeechPort
    {
        private readonly List<SpeechCue> cues = new List<SpeechCue>();
        public IReadOnlyList<SpeechCue> Cues => cues;

        public void Speak(SpeechCue cue)
        {
            cues.Add(cue);
        }
    }

    public sealed class RecordingGameplayRequestMock : IGameplayActionRequestPort
    {
        private readonly List<ActionRequest> requests = new List<ActionRequest>();
        public IReadOnlyList<ActionRequest> Requests => requests;

        public void Request(ActionRequest requestedAction)
        {
            requests.Add(requestedAction);
        }
    }

    public sealed class DeterministicGameBrainMock : IGameBrainPort
    {
        private readonly string selectedActionId;
        private readonly string spokenLine;

        public DeterministicGameBrainMock(string selectedActionId = null, string spokenLine = "I am thinking.")
        {
            this.selectedActionId = selectedActionId;
            this.spokenLine = spokenLine ?? string.Empty;
        }

        public Task<BrainTurn> RequestTurnAsync(
            BrainRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var selection = selectedActionId;
            LegalActionOffer selectedOffer = null;
            if (selection == null && request.LegalOffers.Count > 0)
            {
                var ids = new List<string>();
                for (var index = 0; index < request.LegalOffers.Count; index++)
                {
                    ids.Add(request.LegalOffers[index].ActionId);
                }

                ids.Sort(StringComparer.Ordinal);
                selection = ids[0];
            }

            for (var index = 0; index < request.LegalOffers.Count; index++)
            {
                if (string.Equals(
                    request.LegalOffers[index].ActionId,
                    selection,
                    StringComparison.Ordinal))
                {
                    selectedOffer = request.LegalOffers[index];
                    break;
                }
            }

            var turn = new BrainTurn(
                DialogueIntentGate.SupportedContractVersion,
                "mock_turn_" + request.SimulationTick,
                spokenLine,
                request.Emotion,
                0.5f,
                selection,
                selectedOffer?.Verb ?? ActionVerb.Observe,
                selectedOffer?.TargetEntityIds ?? Array.Empty<string>(),
                string.Empty);
            return Task.FromResult(turn);
        }
    }
}
