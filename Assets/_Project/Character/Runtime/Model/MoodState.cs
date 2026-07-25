using System;

namespace HumanGlassWatcher.Character.Model
{
    [Serializable]
    public sealed class MoodState
    {
        public MoodState(float valence, float arousal, float dominance, CharacterEmotion emotion)
        {
            Valence = CharacterMath.Clamp(valence, -1f, 1f);
            Arousal = CharacterMath.Clamp01(arousal);
            Dominance = CharacterMath.Clamp(dominance, -1f, 1f);
            Emotion = emotion;
        }

        public float Valence { get; private set; }
        public float Arousal { get; private set; }
        public float Dominance { get; private set; }
        public CharacterEmotion Emotion { get; private set; }

        public void Apply(AppraisalResult appraisal, float influence = 0.45f)
        {
            if (appraisal == null)
            {
                return;
            }

            var blend = CharacterMath.Clamp01(influence);
            Valence = Lerp(Valence, appraisal.Valence, blend);
            Arousal = CharacterMath.Clamp01(Lerp(Arousal, appraisal.Arousal, blend));
            Dominance = Lerp(Dominance, appraisal.Dominance, blend);
            Emotion = appraisal.Emotion;
        }

        public void Settle(float hours, PersonalityTraits personality)
        {
            var blend = CharacterMath.Clamp01(Math.Max(0f, hours) * 0.08f);
            var baselineValence = ((personality?.Optimism ?? 0.5f) - 0.5f) * 0.8f;
            Valence = Lerp(Valence, baselineValence, blend);
            Arousal = CharacterMath.Clamp01(Lerp(Arousal, 0.25f, blend));
            Dominance = Lerp(Dominance, 0f, blend);
            if (Arousal < 0.3f && Math.Abs(Valence) < 0.2f)
            {
                Emotion = CharacterEmotion.Neutral;
            }
        }

        private static float Lerp(float from, float to, float amount)
        {
            return from + ((to - from) * amount);
        }
    }

    public sealed class AppraisalResult
    {
        public AppraisalResult(float valence, float arousal, float dominance, CharacterEmotion emotion)
        {
            Valence = CharacterMath.Clamp(valence, -1f, 1f);
            Arousal = CharacterMath.Clamp01(arousal);
            Dominance = CharacterMath.Clamp(dominance, -1f, 1f);
            Emotion = emotion;
        }

        public float Valence { get; }
        public float Arousal { get; }
        public float Dominance { get; }
        public CharacterEmotion Emotion { get; }
    }
}
