using System;
using System.Collections.Generic;
using System.Text;

namespace HumanGlassWatcher.Voice
{
    [Serializable]
    public sealed class VoiceTranscriptionRequestDto
    {
        public int contractVersion = 1;
        public string audioBase64 = string.Empty;
        public string mimeType = "audio/wav";
        public float durationSeconds;
        public string language = "en";
    }

    [Serializable]
    public sealed class VoiceTranscriptionResultDto
    {
        public int contractVersion;
        public string transcript;
        public string language;
        public float durationSeconds;
        public string provider;
    }

    [Serializable]
    public sealed class VoiceSynthesisRequestDto
    {
        public int contractVersion = 1;
        public string text = string.Empty;
        public string voiceId = "resident_default";
    }

    [Serializable]
    public sealed class VoiceSynthesisResultDto
    {
        public int contractVersion;
        public string audioBase64;
        public string mimeType;
        public string provider;
    }

    [Serializable]
    public sealed class ConversationTurnDto
    {
        public string speaker;
        public string text;

        public ConversationTurnDto(string speakerValue, string textValue)
        {
            speaker = speakerValue ?? string.Empty;
            text = textValue ?? string.Empty;
        }
    }

    [Serializable]
    public sealed class ConversationContextDto
    {
        public string residentId;
        public string personality;
        public string memorySummary;
        public ConversationTurnDto[] recentTurns;
    }

    [Serializable]
    public sealed class ActionOfferDto
    {
        public string actionId;
        public string verb;
        public string[] targetEntityIds;
        public float utilityHint;
        public string reasonCode;
    }

    [Serializable]
    public sealed class DialogueRequestDto
    {
        public int contractVersion = 1;
        public string turnId;
        public string playerMessage;
        public string residentState;
        public ConversationContextDto conversationContext;
        public string[] knownEntityIds;
        public ActionOfferDto[] legalActions;
    }

    [Serializable]
    public sealed class DialogueTurnDto
    {
        public int contractVersion;
        public string turnId;
        public string spokenLine;
        public string emotion;
        public float intensity;
        public string selectedActionId;
        public string selectedIntent;
        public string[] targetEntityIds;
        public string memoryNote;
    }

    public sealed class VoiceConversationMemory
    {
        private const int MaximumRecentTurns = 12;
        private const int MaximumMemoryLength = 2000;
        private readonly List<ConversationTurnDto> recentTurns = new List<ConversationTurnDto>();
        private string memorySummary;

        public VoiceConversationMemory(string initialMemory)
        {
            memorySummary = Clip(initialMemory, MaximumMemoryLength);
        }

        public string MemorySummary => memorySummary;

        public ConversationContextDto Snapshot(string residentId, string personality)
        {
            return new ConversationContextDto
            {
                residentId = Clip(residentId, 64),
                personality = Clip(personality, 1000),
                memorySummary = memorySummary,
                recentTurns = recentTurns.ToArray()
            };
        }

        public void RecordTurn(string playerLine, string residentLine, string memoryNote)
        {
            AddRecent("player", Clip(playerLine, 500));
            AddRecent("resident", Clip(residentLine, 500));

            var note = Clip(memoryNote, 240);
            if (string.IsNullOrEmpty(note))
            {
                return;
            }

            memorySummary = string.IsNullOrEmpty(memorySummary)
                ? note
                : Clip(memorySummary + " " + note, MaximumMemoryLength);
        }

        public string BuildStateDigest(string personality)
        {
            var builder = new StringBuilder(2000);
            builder.Append("Personality: ").Append(Clip(personality, 1000));
            builder.Append(" Memory: ").Append(memorySummary);
            if (recentTurns.Count > 0)
            {
                builder.Append(" Recent conversation:");
                for (var index = 0; index < recentTurns.Count; index++)
                {
                    builder.Append(' ')
                        .Append(recentTurns[index].speaker)
                        .Append(": ")
                        .Append(recentTurns[index].text);
                }
            }

            return Clip(builder.ToString(), 2000);
        }

        private void AddRecent(string speaker, string text)
        {
            recentTurns.Add(new ConversationTurnDto(speaker, text));
            while (recentTurns.Count > MaximumRecentTurns)
            {
                recentTurns.RemoveAt(0);
            }
        }

        private static string Clip(string value, int maximum)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var normalized = value.Replace('\0', ' ').Trim();
            return normalized.Length <= maximum
                ? normalized
                : normalized.Substring(normalized.Length - maximum, maximum);
        }
    }

    public static class VoiceContractValidator
    {
        public static bool IsValidTranscription(VoiceTranscriptionResultDto value, out string error)
        {
            if (value == null || value.contractVersion != 1)
            {
                error = "invalid_transcription_contract";
                return false;
            }

            if (value.transcript == null || value.transcript.Length > 1000)
            {
                error = "invalid_transcript";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public static bool IsValidSpeech(VoiceSynthesisResultDto value, out string error)
        {
            if (value == null || value.contractVersion != 1 || value.mimeType != "audio/wav")
            {
                error = "invalid_speech_contract";
                return false;
            }

            try
            {
                var bytes = Convert.FromBase64String(value.audioBase64 ?? string.Empty);
                if (bytes.Length < 44 ||
                    bytes[0] != 'R' ||
                    bytes[1] != 'I' ||
                    bytes[2] != 'F' ||
                    bytes[3] != 'F')
                {
                    error = "invalid_speech_audio";
                    return false;
                }
            }
            catch (FormatException)
            {
                error = "invalid_speech_base64";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public static bool IsValidDialogue(
            DialogueTurnDto turn,
            DialogueRequestDto request,
            out string error)
        {
            if (turn == null ||
                request == null ||
                turn.contractVersion != 1 ||
                !IsStableId(turn.turnId) ||
                !string.Equals(turn.turnId, request.turnId, StringComparison.Ordinal))
            {
                error = "invalid_dialogue_contract";
                return false;
            }

            if (turn.spokenLine == null ||
                turn.spokenLine.Length > 500 ||
                turn.memoryNote == null ||
                turn.memoryNote.Length > 240 ||
                turn.intensity < 0f ||
                turn.intensity > 1f)
            {
                error = "invalid_dialogue_fields";
                return false;
            }

            if (string.IsNullOrEmpty(turn.selectedActionId))
            {
                var targets = turn.targetEntityIds ?? Array.Empty<string>();
                if (turn.selectedIntent != "observe" || targets.Length != 0)
                {
                    error = "invalid_observe_fallback";
                    return false;
                }

                error = string.Empty;
                return true;
            }

            var offers = request.legalActions ?? Array.Empty<ActionOfferDto>();
            for (var index = 0; index < offers.Length; index++)
            {
                var offer = offers[index];
                if (!string.Equals(offer.actionId, turn.selectedActionId, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!string.Equals(offer.verb, turn.selectedIntent, StringComparison.Ordinal) ||
                    !TargetsMatch(offer.targetEntityIds, turn.targetEntityIds))
                {
                    error = "dialogue_offer_mismatch";
                    return false;
                }

                error = string.Empty;
                return true;
            }

            error = "dialogue_action_not_offered";
            return false;
        }

        private static bool TargetsMatch(string[] offered, string[] returned)
        {
            offered = offered ?? Array.Empty<string>();
            returned = returned ?? Array.Empty<string>();
            if (offered.Length != returned.Length)
            {
                return false;
            }

            for (var index = 0; index < offered.Length; index++)
            {
                if (!string.Equals(offered[index], returned[index], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsStableId(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length > 64)
            {
                return false;
            }

            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                if (!char.IsLetterOrDigit(character) && character != '_' && character != '-')
                {
                    return false;
                }
            }

            return true;
        }
    }
}
