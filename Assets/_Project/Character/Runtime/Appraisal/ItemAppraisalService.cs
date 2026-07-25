using HumanGlassWatcher.Character.Model;

namespace HumanGlassWatcher.Character.Appraisal
{
    public sealed class ItemAppraisalService
    {
        public AppraisalResult Appraise(ResidentState resident, ItemObservation item)
        {
            var preference = resident.Profile.Preferences.Score(item.CanonicalId, item.Tags) / 32f;
            var danger = item.SafetyRisk + (item.Has(ItemCapability.Toxic) ? 0.5f : 0f);
            var filth = item.Dirtiness + (item.Has(ItemCapability.Dirty) ? 0.45f : 0f);
            var needRelevance = 0f;

            if (item.Has(ItemCapability.Edible))
            {
                needRelevance += resident.Needs.Hunger * (0.35f + (item.Taste * 0.25f));
            }

            if (item.Has(ItemCapability.Drinkable))
            {
                needRelevance += resident.Needs.Thirst * 0.45f;
            }

            if (item.Has(ItemCapability.Comfort))
            {
                needRelevance += resident.Needs.Comfort * 0.35f;
            }

            if (item.Has(ItemCapability.Entertainment) || item.Has(ItemCapability.Bouncy))
            {
                needRelevance += resident.Needs.Stimulation * 0.3f;
            }

            var valence = CharacterMath.Clamp(
                preference + needRelevance - (danger * 0.8f) - (filth * 0.75f),
                -1f,
                1f);
            var arousal = CharacterMath.Clamp01(
                0.15f + (item.Novelty * resident.Profile.Traits.Curiosity * 0.45f) + (danger * 0.5f));
            var dominance = CharacterMath.Clamp(
                resident.Profile.Traits.Resourcefulness - danger - 0.25f,
                -1f,
                1f);

            CharacterEmotion emotion;
            if (filth >= 0.6f)
            {
                emotion = CharacterEmotion.Disgust;
            }
            else if (danger >= 0.65f)
            {
                emotion = CharacterEmotion.Fear;
            }
            else if (valence > 0.35f)
            {
                emotion = CharacterEmotion.Joy;
            }
            else if (item.Novelty > 0.55f)
            {
                emotion = CharacterEmotion.Curiosity;
            }
            else if (valence < -0.25f)
            {
                emotion = CharacterEmotion.Contempt;
            }
            else
            {
                emotion = CharacterEmotion.Neutral;
            }

            return new AppraisalResult(valence, arousal, dominance, emotion);
        }
    }
}
