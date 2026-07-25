using System;
using System.Collections.Generic;

namespace HumanGlassWatcher.Character.Persistence
{
    [Serializable]
    public sealed class ResidentSaveDto
    {
        public int SchemaVersion;
        public string ResidentId;
        public int Seed;
        public long SimulationTick;
        public string CurrentPlanId;
        public PersonalityTraitsDto Traits;
        public PreferencesDto Preferences;
        public string VoiceIdentity;
        public string ConversationStyle;
        public NeedsDto Needs;
        public MoodDto Mood;
        public RelationshipDto Relationship;
        public List<EpisodicMemoryDto> EpisodicMemories = new List<EpisodicMemoryDto>();
    }

    [Serializable]
    public sealed class PersonalityTraitsDto
    {
        public float Optimism;
        public float Patience;
        public float Warmth;
        public float AngerTendency;
        public float Humor;
        public float Curiosity;
        public float Caution;
        public float Resourcefulness;
        public float Honesty;
        public float Impulsiveness;
        public float Trustfulness;
        public float Attachment;
        public float Defiance;
        public float DesireForCompany;
        public float FreedomValue;
        public float ComfortValue;
        public float CleanlinessValue;
        public float NoveltyValue;
        public float SafetyValue;
        public float FairnessValue;
    }

    [Serializable]
    public sealed class PreferencesDto
    {
        public List<string> LikedItemIds = new List<string>();
        public List<string> DislikedItemIds = new List<string>();
        public List<string> LikedTags = new List<string>();
        public List<string> DislikedTags = new List<string>();
    }

    [Serializable]
    public sealed class NeedsDto
    {
        public float Hunger;
        public float Thirst;
        public float Energy;
        public float Safety;
        public float Comfort;
        public float Hygiene;
        public float SocialConnection;
        public float Stimulation;
        public float Freedom;
    }

    [Serializable]
    public sealed class MoodDto
    {
        public float Valence;
        public float Arousal;
        public float Dominance;
        public string Emotion;
    }

    [Serializable]
    public sealed class RelationshipDto
    {
        public float Trust;
        public float Affection;
        public float Fear;
        public float Resentment;
        public float Dependency;
        public float PerceivedReliability;
    }

    [Serializable]
    public sealed class EpisodicMemoryDto
    {
        public string EventId;
        public long SimulationTick;
        public string EventType;
        public string SubjectEntityId;
        public string ActorId;
        public float Valence;
        public float Importance;
        public string Summary;
    }
}
