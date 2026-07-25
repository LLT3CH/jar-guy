using System;

namespace HumanGlassWatcher.Character.Model
{
    [Serializable]
    public sealed class ResidentProfile
    {
        public ResidentProfile(
            int seed,
            PersonalityTraits traits,
            ResidentPreferences preferences,
            string voiceIdentity,
            string conversationStyle)
        {
            Seed = seed;
            Traits = traits ?? throw new ArgumentNullException(nameof(traits));
            Preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
            VoiceIdentity = voiceIdentity ?? string.Empty;
            ConversationStyle = conversationStyle ?? string.Empty;
        }

        public int Seed { get; }
        public PersonalityTraits Traits { get; }
        public ResidentPreferences Preferences { get; }
        public string VoiceIdentity { get; }
        public string ConversationStyle { get; }
    }
}
