using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HumanGlassWatcher.Character.Appraisal;
using HumanGlassWatcher.Character.Model;
using HumanGlassWatcher.Character.Planning;

namespace HumanGlassWatcher.Character.Integration
{
    public interface ICharacterAnimationPort
    {
        void Preview(ActionRequest requestedAction);
    }

    public interface ICharacterSpeechPort
    {
        void Speak(SpeechCue cue);
    }

    public interface ICharacterPerceptionPort
    {
        CharacterPerceptionSnapshot Capture();
    }

    public interface IGameBrainPort
    {
        Task<BrainTurn> RequestTurnAsync(BrainRequest request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Gameplay implements this port and remains the final validator/executor.
    /// </summary>
    public interface IGameplayActionRequestPort
    {
        void Request(ActionRequest requestedAction);
    }

    public sealed class SpeechCue
    {
        public SpeechCue(string line, CharacterEmotion emotion, float intensity)
        {
            Line = line ?? string.Empty;
            Emotion = emotion;
            Intensity = CharacterMath.Clamp01(intensity);
        }

        public string Line { get; }
        public CharacterEmotion Emotion { get; }
        public float Intensity { get; }
    }

    public sealed class CharacterPerceptionSnapshot
    {
        private readonly List<ItemObservation> items;
        private readonly List<LegalActionOffer> legalOffers;

        public CharacterPerceptionSnapshot(
            IEnumerable<ItemObservation> items,
            IEnumerable<LegalActionOffer> legalOffers)
        {
            this.items = new List<ItemObservation>(items ?? new ItemObservation[0]);
            this.legalOffers = new List<LegalActionOffer>(legalOffers ?? new LegalActionOffer[0]);
        }

        public IReadOnlyList<ItemObservation> Items => items;
        public IReadOnlyList<LegalActionOffer> LegalOffers => legalOffers;
    }

    public sealed class BrainRequest
    {
        private readonly List<LegalActionOffer> legalOffers;

        public BrainRequest(
            string residentId,
            long simulationTick,
            CharacterEmotion emotion,
            IEnumerable<LegalActionOffer> legalOffers)
        {
            ResidentId = residentId ?? string.Empty;
            SimulationTick = simulationTick;
            Emotion = emotion;
            this.legalOffers = new List<LegalActionOffer>(legalOffers ?? new LegalActionOffer[0]);
        }

        public string ResidentId { get; }
        public long SimulationTick { get; }
        public CharacterEmotion Emotion { get; }
        public IReadOnlyList<LegalActionOffer> LegalOffers => legalOffers;
    }

    /// <summary>
    /// Character-side projection of contracts/v1/dialogue-turn.schema.json.
    /// SpokenLine and MemoryNote are advisory text and are never parsed as commands.
    /// </summary>
    public sealed class BrainTurn
    {
        private readonly string[] targetEntityIds;

        public BrainTurn(
            int contractVersion,
            string turnId,
            string spokenLine,
            CharacterEmotion emotion,
            float intensity,
            string selectedActionId,
            ActionVerb selectedIntent,
            IEnumerable<string> targetEntityIds,
            string memoryNote)
        {
            ContractVersion = contractVersion;
            TurnId = turnId ?? string.Empty;
            SpokenLine = spokenLine ?? string.Empty;
            Emotion = emotion;
            Intensity = CharacterMath.Clamp01(intensity);
            SelectedActionId = selectedActionId;
            SelectedIntent = selectedIntent;
            this.targetEntityIds = CharacterMath.CopyStrings(targetEntityIds);
            MemoryNote = memoryNote ?? string.Empty;
        }

        public int ContractVersion { get; }
        public string TurnId { get; }
        public string SpokenLine { get; }
        public CharacterEmotion Emotion { get; }
        public float Intensity { get; }
        public string SelectedActionId { get; }
        public ActionVerb SelectedIntent { get; }
        public IReadOnlyList<string> TargetEntityIds => targetEntityIds;
        public string MemoryNote { get; }
    }
}
