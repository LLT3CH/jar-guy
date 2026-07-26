using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace HumanGlassWatcher.Voice
{
    public sealed class MicrophoneRecorder
    {
        private const int CaptureFrequency = 16000;
        private bool permissionRequestedThisSession;
        private AudioClip recordingClip;
        private string deviceName;

        public bool IsRecording =>
            recordingClip != null &&
            Microphone.IsRecording(deviceName);

        public bool HasCapture => recordingClip != null;

        public async Task<bool> StartAsync(int maximumSeconds, CancellationToken cancellationToken)
        {
            if (!await EnsurePermissionAsync(cancellationToken))
            {
                return false;
            }

            if (Microphone.devices == null || Microphone.devices.Length == 0)
            {
                throw new InvalidOperationException("No microphone input device is available.");
            }

            deviceName = Microphone.devices[0];
            recordingClip = Microphone.Start(
                deviceName,
                false,
                Mathf.Clamp(maximumSeconds, 1, 30),
                CaptureFrequency);
            if (recordingClip == null)
            {
                throw new InvalidOperationException("Unity could not start microphone capture.");
            }

            var startedAt = Time.realtimeSinceStartup;
            while (Microphone.GetPosition(deviceName) <= 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (Time.realtimeSinceStartup - startedAt > 2f)
                {
                    StopAndDiscard();
                    throw new TimeoutException("Microphone capture did not start within two seconds.");
                }

                await Task.Yield();
            }

            return true;
        }

        public RecordedAudio Stop()
        {
            if (recordingClip == null)
            {
                throw new InvalidOperationException("Microphone capture is not active.");
            }

            var sampleFrames = Microphone.GetPosition(deviceName);
            var clip = recordingClip;
            Microphone.End(deviceName);
            recordingClip = null;

            if (sampleFrames <= 0)
            {
                sampleFrames = clip.samples;
            }

            sampleFrames = Mathf.Clamp(sampleFrames, 1, clip.samples);
            var samples = new float[sampleFrames * clip.channels];
            clip.GetData(samples, 0);
            var duration = sampleFrames / (float)clip.frequency;
            var bytes = WavCodec.EncodePcm16(samples, clip.channels, clip.frequency);
            UnityEngine.Object.Destroy(clip);
            return new RecordedAudio(bytes, duration);
        }

        public void StopAndDiscard()
        {
            if (recordingClip == null)
            {
                return;
            }

            Microphone.End(deviceName);
            UnityEngine.Object.Destroy(recordingClip);
            recordingClip = null;
        }

        private async Task<bool> EnsurePermissionAsync(CancellationToken cancellationToken)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                return true;
            }

            if (permissionRequestedThisSession)
            {
                return false;
            }

            permissionRequestedThisSession = true;
            var completion = new TaskCompletionSource<bool>();
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += _ => completion.TrySetResult(true);
            callbacks.PermissionDenied += _ => completion.TrySetResult(false);
            callbacks.PermissionDeniedAndDontAskAgain += _ => completion.TrySetResult(false);
            Permission.RequestUserPermission(Permission.Microphone, callbacks);
            using (cancellationToken.Register(() => completion.TrySetCanceled()))
            {
                return await completion.Task;
            }
#else
            if (Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                return true;
            }

            if (permissionRequestedThisSession)
            {
                return false;
            }

            permissionRequestedThisSession = true;
            var request = Application.RequestUserAuthorization(UserAuthorization.Microphone);
            while (!request.isDone)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            return Application.HasUserAuthorization(UserAuthorization.Microphone);
#endif
        }
    }
}
