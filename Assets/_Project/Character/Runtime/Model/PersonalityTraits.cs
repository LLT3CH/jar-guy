using System;

namespace HumanGlassWatcher.Character.Model
{
    [Serializable]
    public sealed class PersonalityTraits
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

        public static PersonalityTraits Neutral()
        {
            return new PersonalityTraits
            {
                Optimism = 0.5f,
                Patience = 0.5f,
                Warmth = 0.5f,
                AngerTendency = 0.5f,
                Humor = 0.5f,
                Curiosity = 0.5f,
                Caution = 0.5f,
                Resourcefulness = 0.5f,
                Honesty = 0.5f,
                Impulsiveness = 0.5f,
                Trustfulness = 0.5f,
                Attachment = 0.5f,
                Defiance = 0.5f,
                DesireForCompany = 0.5f,
                FreedomValue = 0.5f,
                ComfortValue = 0.5f,
                CleanlinessValue = 0.5f,
                NoveltyValue = 0.5f,
                SafetyValue = 0.5f,
                FairnessValue = 0.5f
            };
        }

        public PersonalityTraits Clone()
        {
            return (PersonalityTraits)MemberwiseClone();
        }

        public void Clamp()
        {
            Optimism = CharacterMath.Clamp01(Optimism);
            Patience = CharacterMath.Clamp01(Patience);
            Warmth = CharacterMath.Clamp01(Warmth);
            AngerTendency = CharacterMath.Clamp01(AngerTendency);
            Humor = CharacterMath.Clamp01(Humor);
            Curiosity = CharacterMath.Clamp01(Curiosity);
            Caution = CharacterMath.Clamp01(Caution);
            Resourcefulness = CharacterMath.Clamp01(Resourcefulness);
            Honesty = CharacterMath.Clamp01(Honesty);
            Impulsiveness = CharacterMath.Clamp01(Impulsiveness);
            Trustfulness = CharacterMath.Clamp01(Trustfulness);
            Attachment = CharacterMath.Clamp01(Attachment);
            Defiance = CharacterMath.Clamp01(Defiance);
            DesireForCompany = CharacterMath.Clamp01(DesireForCompany);
            FreedomValue = CharacterMath.Clamp01(FreedomValue);
            ComfortValue = CharacterMath.Clamp01(ComfortValue);
            CleanlinessValue = CharacterMath.Clamp01(CleanlinessValue);
            NoveltyValue = CharacterMath.Clamp01(NoveltyValue);
            SafetyValue = CharacterMath.Clamp01(SafetyValue);
            FairnessValue = CharacterMath.Clamp01(FairnessValue);
        }
    }
}
