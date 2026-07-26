using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace HumanGlassWatcher.Voice
{
    public sealed class VoiceConversationController : MonoBehaviour
    {
        [Header("Service")]
        [SerializeField] private string serviceBaseUrl = "http://127.0.0.1:8787";
        [SerializeField, Range(1, 60)] private int requestTimeoutSeconds = 25;
        [SerializeField, Range(1, 30)] private int maximumRecordingSeconds = 15;
        [SerializeField] private string language = "en";

        [Header("Resident context")]
        [SerializeField] private string residentId = "resident_1";
        [SerializeField] private string voiceId = "resident_default";
        [SerializeField, TextArea(2, 5)] private string personality =
            "Wry, curious, cautious, values freedom, and dislikes being patronized.";
        [SerializeField, TextArea(2, 5)] private string initialMemorySummary =
            "This is the first conversation with the player.";

        [Header("Demo UI")]
        [SerializeField] private Text statusText;
        [SerializeField] private Text transcriptText;
        [SerializeField] private Text responseText;
        [SerializeField] private InputField typedInput;
        [SerializeField] private AudioSource replyAudioSource;

        private readonly MicrophoneRecorder microphone = new MicrophoneRecorder();
        private GameBrainVoiceClient client;
        private VoiceConversationMemory memory;
        private CancellationTokenSource activeOperation;
        private bool pushHeld;
        private bool busy;
        private int turnCounter;
        private AudioClip activeReplyClip;

        public event Action<DialogueTurnDto> ValidatedDialogueReceived;

        public bool IsBusy => busy;
        public bool IsRecording => microphone.IsRecording;

        private void Awake()
        {
            EnsureInitialized();
            SetStatus("Ready. Hold the microphone button or type a message.");
        }

        private void OnDisable()
        {
            pushHeld = false;
            CancelActiveOperation();
            microphone.StopAndDiscard();
            busy = false;
        }

        private void OnDestroy()
        {
            if (activeReplyClip != null)
            {
                Destroy(activeReplyClip);
            }
        }

        public void ConfigureDemoUi(
            Text status,
            Text transcript,
            Text response,
            InputField typed,
            AudioSource audioSource)
        {
            statusText = status;
            transcriptText = transcript;
            responseText = response;
            typedInput = typed;
            replyAudioSource = audioSource;
        }

        public void ConfigureService(string url, string configuredResidentId)
        {
            serviceBaseUrl = string.IsNullOrWhiteSpace(url) ? serviceBaseUrl : url;
            residentId = string.IsNullOrWhiteSpace(configuredResidentId)
                ? residentId
                : configuredResidentId;
        }

        public void ConfigureResidentContext(
            string configuredPersonality,
            string configuredMemorySummary)
        {
            personality = string.IsNullOrWhiteSpace(configuredPersonality)
                ? personality
                : configuredPersonality;
            initialMemorySummary = string.IsNullOrWhiteSpace(configuredMemorySummary)
                ? initialMemorySummary
                : configuredMemorySummary;
            memory = new VoiceConversationMemory(initialMemorySummary);
        }

        public async void BeginPushToTalk()
        {
            if (busy || microphone.IsRecording)
            {
                return;
            }

            EnsureInitialized();
            pushHeld = true;
            var cancellationToken = BeginOperation();
            try
            {
                SetStatus("Requesting microphone permission…");
                var started = await microphone.StartAsync(maximumRecordingSeconds, cancellationToken);
                if (!started)
                {
                    SetStatus("Microphone permission denied. Typed conversation remains available.");
                    return;
                }

                if (!pushHeld)
                {
                    microphone.StopAndDiscard();
                    return;
                }

                SetStatus("Listening… release to send.");
            }
            catch (OperationCanceledException)
            {
                microphone.StopAndDiscard();
            }
            catch (Exception error)
            {
                microphone.StopAndDiscard();
                SetStatus($"Microphone unavailable: {error.Message}");
            }
        }

        public async void EndPushToTalk()
        {
            pushHeld = false;
            if (!microphone.HasCapture)
            {
                CancelActiveOperation();
                return;
            }

            busy = true;
            try
            {
                var recorded = microphone.Stop();
                var cancellationToken = BeginOperation();
                SetStatus("Transcribing…");
                var transcription = await client.TranscribeAsync(
                    new VoiceTranscriptionRequestDto
                    {
                        audioBase64 = Convert.ToBase64String(recorded.WavBytes),
                        durationSeconds = recorded.DurationSeconds,
                        language = language
                    },
                    cancellationToken);

                if (!VoiceContractValidator.IsValidTranscription(transcription, out var error))
                {
                    throw new VoiceServiceException(error, 200);
                }

                var line = (transcription.transcript ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(line))
                {
                    UseLocalFallback("I couldn't make that out. Try again or type it.");
                    return;
                }

                await ProcessTranscriptAsync(line, transcription.provider, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                SetStatus("Voice request canceled.");
            }
            catch (Exception)
            {
                UseLocalFallback("I can see you trying to talk, but the connection isn't carrying your voice.");
            }
            finally
            {
                busy = false;
            }
        }

        public async void SubmitTypedFromInput()
        {
            if (busy)
            {
                return;
            }

            EnsureInitialized();
            var line = typedInput == null ? string.Empty : (typedInput.text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(line))
            {
                SetStatus("Type a message first.");
                return;
            }

            typedInput.text = string.Empty;
            busy = true;
            try
            {
                await ProcessTranscriptAsync(line, "typed", BeginOperation());
            }
            catch (OperationCanceledException)
            {
                SetStatus("Conversation request canceled.");
            }
            catch (Exception)
            {
                UseLocalFallback("I heard the idea of that, but the connection dropped before I could answer.");
            }
            finally
            {
                busy = false;
            }
        }

        private async Task ProcessTranscriptAsync(
            string playerLine,
            string inputProvider,
            CancellationToken cancellationToken)
        {
            SetTranscript(playerLine);
            SetStatus("Thinking…");
            var context = memory.Snapshot(residentId, personality);
            var request = new DialogueRequestDto
            {
                turnId = $"voice_turn_{++turnCounter}",
                playerMessage = playerLine,
                residentState = memory.BuildStateDigest(personality),
                conversationContext = context,
                knownEntityIds = new[] { residentId },
                legalActions = new[]
                {
                    new ActionOfferDto
                    {
                        actionId = "speak_reply",
                        verb = "speak",
                        targetEntityIds = new[] { residentId },
                        utilityHint = 50f,
                        reasonCode = "conversation_reply"
                    }
                }
            };

            var dialogue = await client.GenerateDialogueAsync(request, cancellationToken);
            if (!VoiceContractValidator.IsValidDialogue(dialogue, request, out var dialogueError))
            {
                throw new VoiceServiceException(dialogueError, 200);
            }

            SetResponse(dialogue.spokenLine);
            memory.RecordTurn(playerLine, dialogue.spokenLine, dialogue.memoryNote);
            ValidatedDialogueReceived?.Invoke(dialogue);

            try
            {
                SetStatus("Speaking…");
                var speech = await client.SynthesizeAsync(
                    new VoiceSynthesisRequestDto
                    {
                        text = dialogue.spokenLine,
                        voiceId = voiceId
                    },
                    cancellationToken);
                if (!VoiceContractValidator.IsValidSpeech(speech, out var speechError))
                {
                    throw new VoiceServiceException(speechError, 200);
                }

                PlayReply(Convert.FromBase64String(speech.audioBase64));
                SetStatus(speech.provider == "mock"
                    ? $"Mock flow complete ({inputProvider} input; audio cue is not synthesized speech)."
                    : $"Conversation complete ({inputProvider} → {speech.provider}).");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                SetStatus("Text reply ready; synthesized voice is unavailable.");
            }
        }

        private void PlayReply(byte[] wavBytes)
        {
            if (replyAudioSource == null)
            {
                return;
            }

            if (activeReplyClip != null)
            {
                Destroy(activeReplyClip);
            }

            activeReplyClip = WavCodec.CreateAudioClip(wavBytes, "ResidentVoiceReply");
            replyAudioSource.Stop();
            replyAudioSource.clip = activeReplyClip;
            replyAudioSource.Play();
        }

        private void UseLocalFallback(string line)
        {
            SetResponse(line);
            SetStatus("Offline fallback active. Physics and typed play remain available.");
        }

        private CancellationToken BeginOperation()
        {
            CancelActiveOperation();
            activeOperation = new CancellationTokenSource();
            return activeOperation.Token;
        }

        private void CancelActiveOperation()
        {
            if (activeOperation == null)
            {
                return;
            }

            activeOperation.Cancel();
            activeOperation.Dispose();
            activeOperation = null;
        }

        private void EnsureInitialized()
        {
            if (memory == null)
            {
                memory = new VoiceConversationMemory(initialMemorySummary);
            }

            if (client == null)
            {
                client = new GameBrainVoiceClient(
                    serviceBaseUrl,
                    $"voice_{Guid.NewGuid():N}".Substring(0, 32),
                    requestTimeoutSeconds);
            }
        }

        private void SetStatus(string value)
        {
            if (statusText != null)
            {
                statusText.text = value ?? string.Empty;
            }
        }

        private void SetTranscript(string value)
        {
            if (transcriptText != null)
            {
                transcriptText.text = $"You: {value}";
            }
        }

        private void SetResponse(string value)
        {
            if (responseText != null)
            {
                responseText.text = $"Resident: {value}";
            }
        }
    }
}
