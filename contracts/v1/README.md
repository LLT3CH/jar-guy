# Contract v1

This folder is the integration boundary between Unity and the game-brain service.

## Rules

- Unknown fields are rejected.
- Numeric values are clamped and validated on both sides.
- `canonicalId` is data, never a filesystem or Addressables path.
- Only a catalog-owned, allowlisted `authoredAssetId` may map to an authored prefab.
- Unknown-item providers cannot return an authored asset; their value is always forced to `null`.
- The game client sends legal `ActionOffer` records.
- Dialogue may return only one of those exact `actionId` values or `null`.
- Dialogue intent and target IDs are copied from the matched offer, never trusted from provider text.
- A missing target or a missing, expired, or invalid action ID becomes `observe`; it is never executed optimistically.
- Voice requests contain bounded audio/text data only. Provider credentials are server configuration and are not part of any client contract.
- `ConversationContext` carries bounded personality, advisory memory, and up to twelve recent text turns; it never grants action authority.
- Breaking changes require a new versioned folder.

The Project Director approves this v1 boundary for vertical-slice implementation. Additive changes still require review if they change executable behavior.

## Schemas

| Boundary | Request | Response |
| --- | --- | --- |
| Item resolution | `item-resolution-request.schema.json` | `item-spec.schema.json` |
| Dialogue | `dialogue-request.schema.json`, optional `conversation-context.schema.json`, plus `action-offer.schema.json` | `dialogue-turn.schema.json` |
| Memory compression | `memory-summary-request.schema.json` | `memory-summary.schema.json` |
| Speech-to-text | `voice-transcription-request.schema.json` | `voice-transcription-result.schema.json` |
| Text-to-speech | `voice-synthesis-request.schema.json` | `voice-synthesis-result.schema.json` |
| Service failures | — | `error.schema.json` |

Examples for every row are under `examples/`.
