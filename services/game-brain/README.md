# Game Brain service

A dependency-light Node service for the vertical slice. It uses Node built-ins only and starts with deterministic mock providers, so no install step or cloud credential is required.

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
| `GAME_BRAIN_PROVIDER_TIMEOUT_MS` | `1500` |
| `GAME_BRAIN_CACHE_TTL_MS` | `300000` |
| `GAME_BRAIN_CACHE_MAX_ENTRIES` | `500` |
| `GAME_BRAIN_RATE_LIMIT_WINDOW_MS` | `60000` |
| `GAME_BRAIN_RATE_LIMIT_MAX_REQUESTS` | `120` |
| `GAME_BRAIN_MAX_BODY_BYTES` | `2100000` |

Only `mock` is implemented in this foundation. Selecting another provider fails at startup instead of silently falling back to a partially configured cloud adapter.
