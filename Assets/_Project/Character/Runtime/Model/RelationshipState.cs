using System;
using HumanGlassWatcher.Character.Memory;

namespace HumanGlassWatcher.Character.Model
{
    [Serializable]
    public sealed class RelationshipState
    {
        public float Trust;
        public float Affection;
        public float Fear;
        public float Resentment;
        public float Dependency;
        public float PerceivedReliability;

        public static RelationshipState FromPersonality(PersonalityTraits traits)
        {
            return new RelationshipState
            {
                Trust = traits.Trustfulness * 0.4f,
                Affection = traits.Warmth * 0.2f,
                Fear = 0.05f,
                Resentment = 0f,
                Dependency = traits.Attachment * 0.15f,
                PerceivedReliability = 0.25f
            };
        }

        public RelationshipState Clone()
        {
            return (RelationshipState)MemberwiseClone();
        }

        public void Apply(ResidentEvent memory)
        {
            if (memory == null)
            {
                return;
            }

            var amount = memory.Importance * 0.12f;
            switch (memory.EventType)
            {
                case ResidentEventType.GiftReceived:
                case ResidentEventType.ComfortProvided:
                    Trust += amount;
                    Affection += amount * 0.8f;
                    Dependency += amount * 0.25f;
                    break;
                case ResidentEventType.PlayerCausedMess:
                    Trust -= amount * 0.6f;
                    Resentment += amount;
                    break;
                case ResidentEventType.Harmed:
                    Trust -= amount;
                    Fear += amount;
                    Resentment += amount * 0.8f;
                    break;
                case ResidentEventType.PromiseKept:
                    Trust += amount;
                    PerceivedReliability += amount * 1.2f;
                    break;
                case ResidentEventType.PromiseBroken:
                    Trust -= amount;
                    Resentment += amount * 0.8f;
                    PerceivedReliability -= amount * 1.2f;
                    break;
            }

            Clamp();
        }

        public void Clamp()
        {
            Trust = CharacterMath.Clamp01(Trust);
            Affection = CharacterMath.Clamp01(Affection);
            Fear = CharacterMath.Clamp01(Fear);
            Resentment = CharacterMath.Clamp01(Resentment);
            Dependency = CharacterMath.Clamp01(Dependency);
            PerceivedReliability = CharacterMath.Clamp01(PerceivedReliability);
        }
    }
}
