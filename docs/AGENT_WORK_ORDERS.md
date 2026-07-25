# Agent Work Orders

These are the first three specialist chats to start. Each agent must read `README.md`, `docs/PRODUCT_BRIEF.md`, `docs/VERTICAL_SLICE.md`, and `docs/TECHNICAL_ARCHITECTURE.md` before editing. Agents own separate paths to reduce collisions and must report changed files, tests, assumptions, and blockers.

## Agent 1 — Unity Gameplay Foundation

**Owns:** `Packages`, `ProjectSettings`, `Assets/_Project/Core`, `Assets/_Project/Gameplay`, and gameplay tests.

**Order to paste into the new chat:**

> You are the Unity Gameplay Lead for Human Glass Watcher. Read the four root project documents before acting. Bootstrap a Unity 6.5.5f1 URP project in this repository without overwriting unrelated work. Build the first playable jar loop with clean placeholder assets: a transparent jar and collision interior, a lid that responds to one pointer abstraction for mouse/touch, a search UI revealed by a horizontal lid gesture, submit/cancel behavior, an item factory, falling Rigidbody items, and a placeholder resident target. Implement capability-based item definitions and the required vertical-slice combinations. Keep all provider/LLM logic behind interfaces. Add focused edit-mode and play-mode tests. Work only in your owned paths. Stop after a coherent playable slice, then report exact setup/run steps, changed files, test results, and any blocker. Do not claim completion without opening the scene and checking Console errors.

**Acceptance gate:**

- Project opens in Unity 6.5.5f1 without compile errors.
- One scene demonstrates lid → prompt → item fall.
- Mouse and touch paths use the same interaction abstraction.
- Ball plus either bat or hockey stick exposes `Strike` through capabilities.
- No prompt string is used as a prefab path.
- Core tests pass.

## Agent 2 — Game Brain and Contracts

**Owns:** `contracts` and `services/game-brain`.

**Order to paste into the new chat:**

> You are the Game Brain/Backend Lead for Human Glass Watcher. Read the four root project documents before acting. Build a local, dependency-light Node service foundation that normalizes known item prompts, returns a schema-validated ItemSpec, handles an unknown-item fallback, and returns structured character dialogue chosen from client-supplied legal intents. Create versioned JSON Schemas and examples for item resolution, dialogue turns, memory summaries, errors, and voice-provider interfaces. Provider credentials must remain server-side. Add deterministic mock providers so the full service and tests run with no cloud keys. Add input/output moderation hooks, timeouts, caching, rate-limit seams, and tests for aliases including “dog shit” → `dog_feces`, malicious prefab paths, missing targets, offline behavior, and invalid model JSON. Work only in your owned paths. Report commands, changed files, test results, assumptions, and blockers.

**Acceptance gate:**

- A fresh local command starts the service without cloud credentials.
- Known aliases and unknown prompts return schema-valid, clamped data.
- Dialogue can only choose from legal intents supplied by the client.
- Malformed or unsafe output fails closed to a usable fallback.
- Automated tests cover the contract boundary.

## Agent 3 — Resident Character Systems

**Owns:** `Assets/_Project/Character`, character tests, and `docs/character`.

**Order to paste into the new chat:**

> You are the Resident/Character Systems Lead for Human Glass Watcher. Read the four root project documents before acting. Implement an engine-side, deterministic resident model: seeded personality generation, nine needs, mood/appraisal, relationship dimensions, likes/dislikes, episodic memory, utility-scored action selection, and versioned save DTOs. Build interfaces/adapters for animation, speech, perception, and the game-brain service, but use deterministic mocks. The character must evaluate items and legal affordances supplied by gameplay; it must never execute arbitrary LLM text. Cover the apple, dog feces, rubber ball, swing-tool, cleaning, sleep/comfort, and escape-attempt scenarios. Add unit tests proving identical seeds reproduce profiles, different traits change action rankings, important events persist, and invalid service intents are rejected. Work only in your owned paths. Report changed files, test results, assumptions, and blockers.

**Acceptance gate:**

- Seeded profiles are reproducible and observably distinct.
- Needs and memories change utility selection.
- Resourcefulness/freedom pressure affects escape behavior.
- LLM output cannot bypass legal affordances.
- Save/load round-trips all required state with a schema version.

## Integration order

1. Project Director approves the shared contracts.
2. Gameplay Lead imports core contract DTOs.
3. Character Lead consumes gameplay affordances and emits a requested action.
4. Gameplay validates and executes that action.
5. Game Brain produces structured speech/intent against the same IDs.
6. Project Director runs the seven vertical-slice scenarios and records approval or revision orders.

## Deferred team members

After the slice runs, add:

- Character Art/Animation Lead
- UI/UX and Accessibility Lead
- Audio/Voice Director
- Content and Interaction Designer
- QA/Performance Lead
- Backend/Live Operations Engineer
- Store, Ratings, Privacy, and Compliance Owner

Mass content and polished art stay deferred until the exit gate in `docs/VERTICAL_SLICE.md`.
