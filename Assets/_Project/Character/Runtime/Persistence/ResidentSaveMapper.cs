using System;
using System.Collections.Generic;
using HumanGlassWatcher.Character.Memory;
using HumanGlassWatcher.Character.Model;

namespace HumanGlassWatcher.Character.Persistence
{
    public sealed class ResidentSaveMapper
    {
        public const int CurrentSchemaVersion = 1;

        public ResidentSaveDto Capture(ResidentState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var dto = new ResidentSaveDto
            {
                SchemaVersion = CurrentSchemaVersion,
                ResidentId = state.ResidentId,
                Seed = state.Profile.Seed,
                SimulationTick = state.SimulationTick,
                CurrentPlanId = state.CurrentPlanId,
                Traits = CaptureTraits(state.Profile.Traits),
                Preferences = CapturePreferences(state.Profile.Preferences),
                VoiceIdentity = state.Profile.VoiceIdentity,
                ConversationStyle = state.Profile.ConversationStyle,
                Needs = CaptureNeeds(state.Needs),
                Mood = new MoodDto
                {
                    Valence = state.Mood.Valence,
                    Arousal = state.Mood.Arousal,
                    Dominance = state.Mood.Dominance,
                    Emotion = state.Mood.Emotion.ToString()
                },
                Relationship = CaptureRelationship(state.Relationship)
            };

            for (var index = 0; index < state.Memory.Events.Count; index++)
            {
                var memory = state.Memory.Events[index];
                dto.EpisodicMemories.Add(new EpisodicMemoryDto
                {
                    EventId = memory.EventId,
                    SimulationTick = memory.SimulationTick,
                    EventType = memory.EventType.ToString(),
                    SubjectEntityId = memory.SubjectEntityId,
                    ActorId = memory.ActorId,
                    Valence = memory.Valence,
                    Importance = memory.Importance,
                    Summary = memory.Summary
                });
            }

            return dto;
        }

        public ResidentState Restore(ResidentSaveDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            if (dto.SchemaVersion != CurrentSchemaVersion)
            {
                throw new NotSupportedException(
                    $"Resident save schema {dto.SchemaVersion} is not supported; expected {CurrentSchemaVersion}.");
            }

            if (dto.Traits == null || dto.Preferences == null || dto.Needs == null ||
                dto.Mood == null || dto.Relationship == null)
            {
                throw new ArgumentException("Resident save is missing required state.", nameof(dto));
            }

            var traits = RestoreTraits(dto.Traits);
            traits.Clamp();
            var profile = new ResidentProfile(
                dto.Seed,
                traits,
                new ResidentPreferences(
                    dto.Preferences.LikedItemIds,
                    dto.Preferences.DislikedItemIds,
                    dto.Preferences.LikedTags,
                    dto.Preferences.DislikedTags),
                dto.VoiceIdentity,
                dto.ConversationStyle);
            var needs = RestoreNeeds(dto.Needs);
            needs.Clamp();
            var relationship = RestoreRelationship(dto.Relationship);
            relationship.Clamp();

            if (!Enum.TryParse(dto.Mood.Emotion, out CharacterEmotion emotion))
            {
                emotion = CharacterEmotion.Neutral;
            }

            var memories = new List<ResidentEvent>();
            if (dto.EpisodicMemories != null)
            {
                for (var index = 0; index < dto.EpisodicMemories.Count; index++)
                {
                    var saved = dto.EpisodicMemories[index];
                    if (!Enum.TryParse(saved.EventType, out ResidentEventType eventType))
                    {
                        throw new ArgumentException(
                            $"Unknown resident event type '{saved.EventType}'.",
                            nameof(dto));
                    }

                    memories.Add(new ResidentEvent(
                        saved.EventId,
                        saved.SimulationTick,
                        eventType,
                        saved.SubjectEntityId,
                        saved.ActorId,
                        saved.Valence,
                        saved.Importance,
                        saved.Summary));
                }
            }

            return new ResidentState(
                dto.ResidentId,
                profile,
                needs,
                new MoodState(dto.Mood.Valence, dto.Mood.Arousal, dto.Mood.Dominance, emotion),
                relationship,
                EpisodicMemory.Restore(EpisodicMemory.DefaultCapacity, memories),
                dto.SimulationTick,
                dto.CurrentPlanId);
        }

        private static PersonalityTraitsDto CaptureTraits(PersonalityTraits traits)
        {
            return new PersonalityTraitsDto
            {
                Optimism = traits.Optimism,
                Patience = traits.Patience,
                Warmth = traits.Warmth,
                AngerTendency = traits.AngerTendency,
                Humor = traits.Humor,
                Curiosity = traits.Curiosity,
                Caution = traits.Caution,
                Resourcefulness = traits.Resourcefulness,
                Honesty = traits.Honesty,
                Impulsiveness = traits.Impulsiveness,
                Trustfulness = traits.Trustfulness,
                Attachment = traits.Attachment,
                Defiance = traits.Defiance,
                DesireForCompany = traits.DesireForCompany,
                FreedomValue = traits.FreedomValue,
                ComfortValue = traits.ComfortValue,
                CleanlinessValue = traits.CleanlinessValue,
                NoveltyValue = traits.NoveltyValue,
                SafetyValue = traits.SafetyValue,
                FairnessValue = traits.FairnessValue
            };
        }

        private static PersonalityTraits RestoreTraits(PersonalityTraitsDto traits)
        {
            return new PersonalityTraits
            {
                Optimism = traits.Optimism,
                Patience = traits.Patience,
                Warmth = traits.Warmth,
                AngerTendency = traits.AngerTendency,
                Humor = traits.Humor,
                Curiosity = traits.Curiosity,
                Caution = traits.Caution,
                Resourcefulness = traits.Resourcefulness,
                Honesty = traits.Honesty,
                Impulsiveness = traits.Impulsiveness,
                Trustfulness = traits.Trustfulness,
                Attachment = traits.Attachment,
                Defiance = traits.Defiance,
                DesireForCompany = traits.DesireForCompany,
                FreedomValue = traits.FreedomValue,
                ComfortValue = traits.ComfortValue,
                CleanlinessValue = traits.CleanlinessValue,
                NoveltyValue = traits.NoveltyValue,
                SafetyValue = traits.SafetyValue,
                FairnessValue = traits.FairnessValue
            };
        }

        private static PreferencesDto CapturePreferences(ResidentPreferences preferences)
        {
            var dto = new PreferencesDto
            {
                LikedItemIds = Sorted(preferences.LikedItemIds),
                DislikedItemIds = Sorted(preferences.DislikedItemIds),
                LikedTags = Sorted(preferences.LikedTags),
                DislikedTags = Sorted(preferences.DislikedTags)
            };
            return dto;
        }

        private static NeedsDto CaptureNeeds(ResidentNeeds needs)
        {
            return new NeedsDto
            {
                Hunger = needs.Hunger,
                Thirst = needs.Thirst,
                Energy = needs.Energy,
                Safety = needs.Safety,
                Comfort = needs.Comfort,
                Hygiene = needs.Hygiene,
                SocialConnection = needs.SocialConnection,
                Stimulation = needs.Stimulation,
                Freedom = needs.Freedom
            };
        }

        private static ResidentNeeds RestoreNeeds(NeedsDto needs)
        {
            return new ResidentNeeds
            {
                Hunger = needs.Hunger,
                Thirst = needs.Thirst,
                Energy = needs.Energy,
                Safety = needs.Safety,
                Comfort = needs.Comfort,
                Hygiene = needs.Hygiene,
                SocialConnection = needs.SocialConnection,
                Stimulation = needs.Stimulation,
                Freedom = needs.Freedom
            };
        }

        private static RelationshipDto CaptureRelationship(RelationshipState relationship)
        {
            return new RelationshipDto
            {
                Trust = relationship.Trust,
                Affection = relationship.Affection,
                Fear = relationship.Fear,
                Resentment = relationship.Resentment,
                Dependency = relationship.Dependency,
                PerceivedReliability = relationship.PerceivedReliability
            };
        }

        private static RelationshipState RestoreRelationship(RelationshipDto relationship)
        {
            return new RelationshipState
            {
                Trust = relationship.Trust,
                Affection = relationship.Affection,
                Fear = relationship.Fear,
                Resentment = relationship.Resentment,
                Dependency = relationship.Dependency,
                PerceivedReliability = relationship.PerceivedReliability
            };
        }

        private static List<string> Sorted(IEnumerable<string> values)
        {
            var result = new List<string>(values);
            result.Sort(StringComparer.Ordinal);
            return result;
        }
    }
}
