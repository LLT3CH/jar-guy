using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace HumanGlassWatcher.Voice
{
    public sealed class VoiceServiceException : Exception
    {
        public VoiceServiceException(string code, long statusCode)
            : base($"Game-brain voice request failed: {code} ({statusCode}).")
        {
            Code = code ?? "request_failed";
            StatusCode = statusCode;
        }

        public string Code { get; }
        public long StatusCode { get; }
    }

    public sealed class GameBrainVoiceClient
    {
        private readonly string baseUrl;
        private readonly string clientId;
        private readonly int timeoutSeconds;

        public GameBrainVoiceClient(string serviceBaseUrl, string sessionClientId, int requestTimeoutSeconds)
        {
            if (string.IsNullOrWhiteSpace(serviceBaseUrl))
            {
                throw new ArgumentException("A game-brain service URL is required.", nameof(serviceBaseUrl));
            }

            baseUrl = serviceBaseUrl.Trim().TrimEnd('/');
            clientId = string.IsNullOrWhiteSpace(sessionClientId)
                ? "voice_session"
                : sessionClientId;
            timeoutSeconds = Mathf.Clamp(requestTimeoutSeconds, 1, 60);
        }

        public Task<VoiceTranscriptionResultDto> TranscribeAsync(
            VoiceTranscriptionRequestDto request,
            CancellationToken cancellationToken)
        {
            return PostAsync<VoiceTranscriptionRequestDto, VoiceTranscriptionResultDto>(
                "/v1/voice/transcribe",
                request,
                cancellationToken);
        }

        public Task<DialogueTurnDto> GenerateDialogueAsync(
            DialogueRequestDto request,
            CancellationToken cancellationToken)
        {
            return PostAsync<DialogueRequestDto, DialogueTurnDto>(
                "/v1/dialogue/turn",
                request,
                cancellationToken);
        }

        public Task<VoiceSynthesisResultDto> SynthesizeAsync(
            VoiceSynthesisRequestDto request,
            CancellationToken cancellationToken)
        {
            return PostAsync<VoiceSynthesisRequestDto, VoiceSynthesisResultDto>(
                "/v1/voice/synthesize",
                request,
                cancellationToken);
        }

        private async Task<TResponse> PostAsync<TRequest, TResponse>(
            string path,
            TRequest payload,
            CancellationToken cancellationToken)
        {
            var json = JsonUtility.ToJson(payload);
            using (var request = new UnityWebRequest(baseUrl + path, UnityWebRequest.kHttpVerbPOST))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = timeoutSeconds;
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("X-Client-Id", clientId);

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        request.Abort();
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new VoiceServiceException(
                        string.IsNullOrEmpty(request.error) ? "request_failed" : request.error,
                        request.responseCode);
                }

                var response = JsonUtility.FromJson<TResponse>(request.downloadHandler.text);
                if (response == null)
                {
                    throw new VoiceServiceException("invalid_json_response", request.responseCode);
                }

                return response;
            }
        }
    }
}
