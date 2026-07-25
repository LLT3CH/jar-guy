# Resident Character System

The character module is a deterministic, engine-side decision system. It owns resident cognition and emits an `ActionRequest`; it never executes gameplay actions.

## Authority boundary

1. Gameplay supplies a `CharacterPerceptionSnapshot` containing item observations and current `LegalActionOffer` values.
2. `UtilityActionPlanner` scores only those offers from needs, personality, preference, relationship, mood, risk, and episodic memory.
3. The character sends the selected request back through `IGameplayActionRequestPort`.
4. Gameplay revalidates the offer and remains the only action executor.
5. A game-brain response must repeat an exact current action ID, verb, and ordered target list. `DialogueIntentGate` verifies all three, then builds the request from the matching local offer rather than service text or service data.
6. Missing, expired, malformed, or fabricated service intents resolve to an offered `Observe` action or the inert `observe_fallback`.

`LegalActionOffer` mirrors `contracts/v1/action-offer.schema.json`. It is intentionally character-local until the Gameplay Lead imports the approved core DTOs; the future adapter should map the shared DTO into this type without changing IDs.

## Determinism

- `DeterministicRandom` owns a SplitMix64 algorithm so profile output is independent of `System.Random` and framework changes.
- A stable seed generates 20 traits/values, item and tag preferences, voice identity, and conversation style.
- Utility scoring has no clock, physics, network, or global-random dependency.
- Equal utility is resolved by ordinal `actionId`, making selection stable across collection order.
- Simulation time is an explicit tick stored in resident state and saves.

## State semantics

All nine need values are **pressure** in `[0, 1]`: `0` is satisfied and `1` is urgent.

- hunger
- thirst
- energy (sleep pressure)
- safety
- comfort
- hygiene
- social connection
- stimulation
- freedom

Mood uses valence `[-1, 1]`, arousal `[0, 1]`, dominance `[-1, 1]`, and a discrete emotion. Relationship state stores trust, affection, fear, resentment, dependency, and perceived reliability. Numeric relationship changes are applied only by local structured-event rules.

The episodic buffer keeps the 25 most recent events whose importance is at least `0.25`. Summaries are display/advisory text, not commands. Negative memories tied to a target add aversion to approach/use actions and make avoidance more attractive.

## Slice behavior

- **Apple:** hunger, taste, preference, risk, and target memories affect `Eat`.
- **Dog feces:** dirtiness and toxicity produce disgust; hygiene, caution, and cleanliness favor `Avoid` or a supplied `Clean` path.
- **Rubber ball + swing tool:** any offered `Strike` with `SwingTool` plus `Bouncy`/`Throwable` targets is scored without exact item-pair rules.
- **Cleaning:** hygiene pressure and cleanliness favor a gameplay-supplied cleaning affordance involving sponge/water.
- **Sleep/comfort:** energy and comfort pressure favor `Rest`, with comfort-capable items adding utility.
- **Escape:** freedom pressure, resourcefulness, freedom values, and defiance compete with risk, comfort, attachment, trust, and dependency.

Physical legality still belongs to gameplay. Defensive capability checks can suppress a malformed strike offer, but the character never invents a missing interaction.

## Save contract

`ResidentSaveDto` schema version `1` round-trips:

- resident ID, seed, simulation tick, and current plan ID;
- all generated traits, preferences, voice identity, and conversation style;
- all nine needs;
- mood;
- all relationship dimensions;
- structured episodic memories.

`ResidentSaveMapper.Restore` rejects unsupported versions and missing required sections. Gameplay-owned jar items remain in the gameplay save; memories retain stable entity IDs for cross-system association.

## Integration checklist

- Map shared gameplay DTO verbs/capabilities explicitly; do not parse arbitrary prompt or dialogue text.
- Preserve `actionId`, `targetEntityIds`, `utilityHint`, and `reasonCode` exactly.
- Capture perception and offers as one coherent snapshot.
- Validate service `selectedActionId`, `selectedIntent`, and `targetEntityIds` against one exact current offer.
- Treat a character `ActionRequest` as a request, then revalidate it against current gameplay state before execution.
- Keep network work outside Unity's main thread; deterministic mocks provide offline behavior.
- Serialize the versioned DTO using the project save layer after that layer is approved.

## Tests

Edit-mode tests live beside the owned module at `Assets/_Project/Character/Tests/EditMode`. They cover profile reproducibility/distinction, all requested slice action rankings, need and memory effects, escape traits, service-intent rejection, mock determinism, important-memory persistence, version rejection, and complete DTO round-trip.
