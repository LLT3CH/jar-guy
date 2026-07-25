using System;
using System.Collections.Generic;
using HumanGlassWatcher.Character.Model;

namespace HumanGlassWatcher.Character.Memory
{
    [Serializable]
    public sealed class ResidentEvent
    {
        public ResidentEvent(
            string eventId,
            long simulationTick,
            ResidentEventType eventType,
            string subjectEntityId,
            string actorId,
            float valence,
            float importance,
            string summary)
        {
            if (!CharacterMath.IsStableId(eventId))
            {
                throw new ArgumentException("Event IDs must be stable contract-safe IDs.", nameof(eventId));
            }

            EventId = eventId;
            SimulationTick = Math.Max(0L, simulationTick);
            EventType = eventType;
            SubjectEntityId = subjectEntityId ?? string.Empty;
            ActorId = actorId ?? string.Empty;
            Valence = CharacterMath.Clamp(valence, -1f, 1f);
            Importance = CharacterMath.Clamp01(importance);
            Summary = summary ?? string.Empty;
        }

        public string EventId { get; }
        public long SimulationTick { get; }
        public ResidentEventType EventType { get; }
        public string SubjectEntityId { get; }
        public string ActorId { get; }
        public float Valence { get; }
        public float Importance { get; }
        public string Summary { get; }
    }

    [Serializable]
    public sealed class EpisodicMemory
    {
        public const int DefaultCapacity = 25;
        public const float MinimumImportance = 0.25f;

        private readonly int capacity;
        private readonly List<ResidentEvent> events;

        public EpisodicMemory(int capacity = DefaultCapacity)
        {
            this.capacity = Math.Max(DefaultCapacity, capacity);
            events = new List<ResidentEvent>(this.capacity);
        }

        public int Capacity => capacity;
        public IReadOnlyList<ResidentEvent> Events => events;

        public bool Remember(ResidentEvent memory)
        {
            if (memory == null || memory.Importance < MinimumImportance)
            {
                return false;
            }

            if (events.Count == capacity)
            {
                events.RemoveAt(0);
            }

            events.Add(memory);
            return true;
        }

        public float AversionTo(IEnumerable<string> entityIds)
        {
            var targets = new HashSet<string>(
                CharacterMath.CopyStrings(entityIds),
                StringComparer.Ordinal);

            if (targets.Count == 0)
            {
                return 0f;
            }

            var aversion = 0f;
            for (var index = 0; index < events.Count; index++)
            {
                var memory = events[index];
                if (memory.Valence < 0f && targets.Contains(memory.SubjectEntityId))
                {
                    aversion += -memory.Valence * memory.Importance * 16f;
                }
            }

            return CharacterMath.Clamp(aversion, 0f, 32f);
        }

        public static EpisodicMemory Restore(int capacity, IEnumerable<ResidentEvent> memories)
        {
            var restored = new EpisodicMemory(capacity);
            if (memories == null)
            {
                return restored;
            }

            foreach (var memory in memories)
            {
                restored.Remember(memory);
            }

            return restored;
        }
    }
}
