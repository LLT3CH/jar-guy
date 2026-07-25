using System;

namespace HumanGlassWatcher.Character.Model
{
    /// <summary>
    /// Each value is pressure in [0, 1]: zero is satisfied, one is urgent.
    /// </summary>
    [Serializable]
    public sealed class ResidentNeeds
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

        public static ResidentNeeds Neutral()
        {
            return new ResidentNeeds
            {
                Hunger = 0.3f,
                Thirst = 0.25f,
                Energy = 0.25f,
                Safety = 0.15f,
                Comfort = 0.2f,
                Hygiene = 0.1f,
                SocialConnection = 0.3f,
                Stimulation = 0.3f,
                Freedom = 0.35f
            };
        }

        public ResidentNeeds Clone()
        {
            return (ResidentNeeds)MemberwiseClone();
        }

        public void AdvanceHours(float hours)
        {
            var elapsed = Math.Max(0f, hours);
            Hunger += elapsed * 0.055f;
            Thirst += elapsed * 0.075f;
            Energy += elapsed * 0.04f;
            Comfort += elapsed * 0.02f;
            Hygiene += elapsed * 0.018f;
            SocialConnection += elapsed * 0.025f;
            Stimulation += elapsed * 0.03f;
            Freedom += elapsed * 0.012f;
            Clamp();
        }

        public void Relieve(ActionVerb verb, float effectiveness)
        {
            var amount = CharacterMath.Clamp01(effectiveness);
            switch (verb)
            {
                case ActionVerb.Eat:
                    Hunger -= amount;
                    break;
                case ActionVerb.Drink:
                    Thirst -= amount;
                    break;
                case ActionVerb.Rest:
                    Energy -= amount;
                    Comfort -= amount * 0.45f;
                    break;
                case ActionVerb.Clean:
                    Hygiene -= amount;
                    Comfort -= amount * 0.2f;
                    break;
                case ActionVerb.Play:
                case ActionVerb.Strike:
                    Stimulation -= amount;
                    break;
                case ActionVerb.Speak:
                    SocialConnection -= amount;
                    break;
                case ActionVerb.AttemptEscape:
                    Freedom -= amount * 0.25f;
                    break;
            }

            Clamp();
        }

        public void Clamp()
        {
            Hunger = CharacterMath.Clamp01(Hunger);
            Thirst = CharacterMath.Clamp01(Thirst);
            Energy = CharacterMath.Clamp01(Energy);
            Safety = CharacterMath.Clamp01(Safety);
            Comfort = CharacterMath.Clamp01(Comfort);
            Hygiene = CharacterMath.Clamp01(Hygiene);
            SocialConnection = CharacterMath.Clamp01(SocialConnection);
            Stimulation = CharacterMath.Clamp01(Stimulation);
            Freedom = CharacterMath.Clamp01(Freedom);
        }
    }
}
