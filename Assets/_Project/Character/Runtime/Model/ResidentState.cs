using System;
using HumanGlassWatcher.Character.Determinism;
using HumanGlassWatcher.Character.Memory;

namespace HumanGlassWatcher.Character.Model
{
    [Serializable]
    public sealed class ResidentState
    {
        public ResidentState(
            string residentId,
            ResidentProfile profile,
            ResidentNeeds needs,
            MoodState mood,
            RelationshipState relationship,
            EpisodicMemory memory,
            long simulationTick = 0L,
            string currentPlanId = "")
        {
            if (!CharacterMath.IsStableId(residentId))
            {
                throw new ArgumentException("Resident ID must be a stable contract-safe ID.", nameof(residentId));
            }

            ResidentId = residentId;
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
            Needs = needs ?? throw new ArgumentNullException(nameof(needs));
            Mood = mood ?? throw new ArgumentNullException(nameof(mood));
            Relationship = relationship ?? throw new ArgumentNullException(nameof(relationship));
            Memory = memory ?? throw new ArgumentNullException(nameof(memory));
            SimulationTick = Math.Max(0L, simulationTick);
            CurrentPlanId = currentPlanId ?? string.Empty;
        }

        public string ResidentId { get; }
        public ResidentProfile Profile { get; }
        public ResidentNeeds Needs { get; }
        public MoodState Mood { get; }
        public RelationshipState Relationship { get; }
        public EpisodicMemory Memory { get; }
        public long SimulationTick { get; private set; }
        public string CurrentPlanId { get; private set; }

        public static ResidentState Create(int seed, string residentId = "resident")
        {
            var profile = ResidentProfileGenerator.Generate(seed);
            var random = new DeterministicRandom(seed ^ unchecked((int)0x7F4A7C15));
            var needs = ResidentNeeds.Neutral();
            needs.Hunger = random.Range(0.15f, 0.45f);
            needs.Thirst = random.Range(0.15f, 0.4f);
            needs.Energy = random.Range(0.1f, 0.4f);
            needs.SocialConnection = random.Range(0.15f, 0.5f);
            needs.Stimulation = random.Range(0.2f, 0.55f);
            needs.Freedom = CharacterMath.Clamp01(
                (profile.Traits.FreedomValue * 0.45f) + random.Range(0.1f, 0.35f));

            var mood = new MoodState(
                (profile.Traits.Optimism - 0.5f) * 0.8f,
                0.2f + (profile.Traits.Impulsiveness * 0.25f),
                (profile.Traits.Defiance - 0.5f) * 0.35f,
                CharacterEmotion.Neutral);

            return new ResidentState(
                residentId,
                profile,
                needs,
                mood,
                RelationshipState.FromPersonality(profile.Traits),
                new EpisodicMemory());
        }

        public void Advance(float hours)
        {
            var elapsed = Math.Max(0f, hours);
            Needs.AdvanceHours(elapsed);
            Mood.Settle(elapsed, Profile.Traits);
            SimulationTick += (long)Math.Round(elapsed * 3600f);
        }

        public bool Record(ResidentEvent memory)
        {
            if (!Memory.Remember(memory))
            {
                return false;
            }

            Relationship.Apply(memory);
            Mood.Apply(new AppraisalResult(
                memory.Valence,
                memory.Importance,
                memory.Valence * 0.25f,
                EmotionFor(memory)));
            return true;
        }

        public void SetPlan(string planId)
        {
            CurrentPlanId = planId ?? string.Empty;
        }

        private static CharacterEmotion EmotionFor(ResidentEvent memory)
        {
            if (memory.EventType == ResidentEventType.PlayerCausedMess)
            {
                return CharacterEmotion.Disgust;
            }

            if (memory.EventType == ResidentEventType.Harmed)
            {
                return CharacterEmotion.Fear;
            }

            return memory.Valence > 0.2f
                ? CharacterEmotion.Joy
                : memory.Valence < -0.2f
                    ? CharacterEmotion.Anger
                    : CharacterEmotion.Neutral;
        }
    }
}
