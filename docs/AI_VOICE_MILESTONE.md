# AI Voice Milestone

## Implemented vertical slice

The playable `JarLoop` now receives a build-safe Voice-owned runtime overlay after scene load. The overlay supports both typed input and visible hold-to-talk microphone capture. A turn flows through bounded WAV capture, transcription, schema-validated dialogue, short session memory/personality context, WAV synthesis, and Unity audio playback.

The same controller can be exercised in the standalone `VoiceConversationDemo` scene. Mock mode is the default and requires no provider credentials or internet connection.

```text
Unity push-to-talk / typed input
  -> POST /v1/voice/transcribe
  -> POST /v1/dialogue/turn
     (one client-supplied legal speak action + bounded context)
  -> exact client-side action/intent/target validation
  -> POST /v1/voice/synthesize
  -> WAV playback
```

## Security and authority boundaries

- Provider credentials are read only from the Node service environment.
- Unity contracts have no credential field, and `/health` never exposes credentials.
- The microphone is user-activated, permission is requested in context, and capture duration/body size are bounded.
- The service moderates input and output, applies provider timeouts, caches safe responses, exposes a rate-limit seam, and returns schema-valid fallbacks.
- Strict dialogue schema constrains model output. The service still derives intent and targets from an exact client-provided legal `actionId`.
- Unity independently verifies the returned action ID, verb/intent, and target IDs before publishing a validated dialogue event.
- Client-provided prefab paths are never trusted. Item assets remain local allowlisted mappings.
- Real-mode TTS voice and model selection are server-owned. The Unity `voiceId` cannot select arbitrary provider configuration.

## Mock and real modes

Mock mode is deterministic: it accepts actual captured audio, returns a fixed local transcription, chooses the only legal conversation action, incorporates recent context into a predictable reply, and returns a valid short WAV tone. The UI identifies this as an audio cue rather than synthesized speech.

OpenAI mode is enabled with `GAME_BRAIN_PROVIDER=openai` plus a server-side `OPENAI_API_KEY`. It uses audio transcription, strict schema output through the Responses API, and WAV speech synthesis. Configuration and exact commands are in the service and Voice READMEs.

## Failure behavior

- Missing microphone permission: typed input remains usable.
- Service unavailable, timed out, or rate-limited: a local resident bark and explicit offline status are shown.
- Invalid transcription, dialogue JSON, action, target, or WAV: the client fails closed and does not execute the output.
- Speech-only failure: the validated text reply remains visible.
- Mock service: end-to-end plumbing remains testable without keys; it does not claim to generate real speech.

## Known gaps

- Real provider calls have not been exercised with a user credential; the adapter is verified with deterministic HTTP fakes.
- Root integration still needs to run the full repository Unity test pass and produce the final Windows/Android builds.
- Voice memory is bounded and session-local; it is not yet persisted in the Character save/event-memory model.
- Validated dialogue publishes an event but is not wired to the Character/Gameplay action executor.
- Transcript review/cancel-after-transcription, lip-sync markers, and local offline speech recognition/synthesis are not implemented.
- Production client authentication, per-account quotas, telemetry, privacy consent/retention policy, and a public HTTPS deployment are still required.
- The real provider uses a server-configured built-in voice, not a custom voice.

## Verification commands

```powershell
Set-Location services/game-brain
npm.cmd test
```

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.5.5f1\Editor\Unity.exe' `
  -batchmode -nographics `
  -projectPath '<isolated source copy>' `
  -executeMethod HumanGlassWatcher.Voice.Editor.VoiceConversationDemoBuilder.BuildForAutomation `
  -logFile '<isolated source copy>\Logs\voice-import.log' `
  -quit
```

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.5.5f1\Editor\Unity.exe' `
  -batchmode -nographics `
  -projectPath '<isolated source copy>' `
  -runTests -testPlatform EditMode `
  -testFilter HumanGlassWatcher.Voice.Tests `
  -testResults '<isolated source copy>\Logs\voice-tests.xml' `
  -logFile '<isolated source copy>\Logs\voice-tests.log' `
  -quit
```
