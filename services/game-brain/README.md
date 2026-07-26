# Game Brain service

A dependency-light Node service for the vertical slice. It uses Node built-ins only and starts with deterministic mock providers, so no install step or cloud credential is required. Provider credentials stay in this server process and are never part of the Unity request contracts.

## Run

From `services/game-brain`:

```powershell
npm.cmd start
```

If PowerShell permits the npm script shim, `npm start` is equivalent. Direct startup also works:

```powershell
node src/index.js
```

The default address is `http://127.0.0.1:8787`. Check it with:

```powershell
Invoke-RestMethod http://127.0.0.1:8787/health
```

Run all tests:

```powershell
npm.cmd test
```

The mock voice path accepts a real microphone recording, returns a deterministic transcript and dialogue turn, then plays a short WAV cue. The cue proves capture, upload, dialogue, and playback plumbing without pretending to be synthesized speech.

## HTTP routes

All mutation routes require `Content-Type: application/json`.

| Route | Contract response |
| --- | --- |
| `GET /health` | Local health and provider names; never credentials |
| `POST /v1/items/resolve` | `ItemSpec` |
| `POST /v1/dialogue/turn` | `DialogueTurn` |
| `POST /v1/memory/summarize` | `MemorySummary` |
| `POST /v1/voice/transcribe` | `VoiceTranscriptionResult` |
| `POST /v1/voice/synthesize` | `VoiceSynthesisResult` |

Example:

```powershell
$body = '{"contractVersion":1,"prompt":"dog shit"}'
Invoke-RestMethod `
  -Method Post `
  -Uri http://127.0.0.1:8787/v1/items/resolve `
  -ContentType application/json `
  -Body $body
```

## Safety boundary

- The local catalog resolves the twelve authored slice items and aliases before any provider call.
- Unknown provider numbers are clamped, enums and tags are allowlisted, and `authoredAssetId` is always forced to `null`.
- Provider JSON is parsed, moderated, sanitized, and validated against the checked-in v1 schema.
- Invalid JSON, unsafe output, network errors, and timeouts produce deterministic, schema-valid fallbacks.
- Dialogue providers select an `actionId`; the service accepts it only if it matches a client offer whose targets are all present in `knownEntityIds`.
- The service derives `selectedIntent` and `targetEntityIds` from that matched offer. Provider-supplied intent or targets have no authority.
- Input/output moderation, provider timeout, TTL cache, and rate limiter are replaceable seams.

## Provider interfaces

The default providers are in `src/providers/mock-providers.js` and implement these async structural interfaces:

```text
item.resolve({ prompt, signal }) -> JSON object string
dialogue.generate(DialogueRequest + { signal }) -> JSON object string
memory.summarize(MemorySummaryRequest + { signal }) -> JSON object string
transcription.transcribe(VoiceTranscriptionRequest + { signal }) -> object
speech.synthesize(VoiceSynthesisRequest + { signal }) -> object
```

Client-facing voice request/result shapes are versioned in `contracts/v1`. Production adapters should be constructed only inside this server process and read credentials from its environment or secret manager. Credentials must never be accepted from an HTTP request, logged, or returned by `/health`.

## Configuration

| Environment variable | Default |
| --- | --- |
| `GAME_BRAIN_HOST` | `127.0.0.1` |
| `GAME_BRAIN_PORT` | `8787` |
| `GAME_BRAIN_PROVIDER` | `mock` |
| `GAME_BRAIN_PROVIDER_TIMEOUT_MS` | `1500` in mock mode; `20000` in OpenAI mode |
| `GAME_BRAIN_CACHE_TTL_MS` | `300000` |
| `GAME_BRAIN_CACHE_MAX_ENTRIES` | `500` |
| `GAME_BRAIN_RATE_LIMIT_WINDOW_MS` | `60000` |
| `GAME_BRAIN_RATE_LIMIT_MAX_REQUESTS` | `120` |
| `GAME_BRAIN_MAX_BODY_BYTES` | `2100000` |

### Real OpenAI voice/conversation mode

Set the provider and key only in the service environment:

```powershell
$env:GAME_BRAIN_PROVIDER = 'openai'
$env:OPENAI_API_KEY = '<server-side key>'
npm.cmd start
```

Optional server-side settings:

| Environment variable | Default |
| --- | --- |
| `OPENAI_BASE_URL` | `https://api.openai.com/v1` |
| `OPENAI_DIALOGUE_MODEL` | `gpt-5.6-luna` |
| `OPENAI_REASONING_EFFORT` | `low` |
| `OPENAI_TRANSCRIPTION_MODEL` | `gpt-4o-transcribe` |
| `OPENAI_TTS_MODEL` | `gpt-4o-mini-tts` |
| `OPENAI_TTS_VOICE` | `cedar` |
| `OPENAI_TTS_INSTRUCTIONS` | concise fictional-adult delivery |
| `OPENAI_SAFETY_IDENTIFIER` | empty |

OpenAI mode uses multipart transcription, strict JSON-schema dialogue through the Responses API, and WAV speech synthesis. The client-supplied `voiceId` is advisory only: the server always applies its configured voice. Missing `OPENAI_API_KEY`, unsupported provider names, malformed provider JSON, unsafe output, and timeouts fail closed rather than silently downgrading a requested real-provider run.

The checked-in tests stub network calls; they verify endpoint shapes, authorization placement, strict schema configuration, server-owned voice selection, and safe errors without making billable requests.
