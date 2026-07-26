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

## Procedural adult presentation

The graybox capsule is no longer the visible resident. The Character presentation assembly installs a recognizable stylized adult at runtime while retaining the original gameplay target and collider:

- adult-proportioned head, neck, torso, hips, articulated upper/lower arms, hands, thighs, shins, and feet;
- readable eye whites, pupils, brows, nose, ears, hair, and a three-part mouth;
- a warm skin/hair/clothing palette using shared URP-compatible materials;
- continuous breathing, weight shift, head motion, and periodic blinking;
- distinct face and body poses for neutral, joy, curiosity, sadness, fear, anger, disgust, surprise, contempt, and relief;
- explicit recoil, inspect, celebrate, disgust, sleep, comfort, and escape-strain reactions;
- mood binding through `ResidentPresentationController.Bind(ResidentState)`;
- a public reaction hook through `SetReaction(ResidentReaction, intensity, duration)`;
- automatic, idempotent scene attachment through `ResidentPresentationInstaller`;
- an editor preview capture at `Human Glass Watcher > Character > Capture Resident Presentation`.

The presentation uses 31 primitive renderers, nine shared instanced materials, no per-part colliders, no textures, no Animator controller, and no per-frame allocations. This keeps the single-resident slice practical for Windows and mid-range Android while providing a much clearer human silhouette and emotional read than the original capsule.

### Scene integration

`ResidentPresentationInstaller` locates the gameplay object named `Resident Target - Juniper` after scene load. It disables only the capsule renderer and facing marker, preserves the target collider, counteracts the placeholder transform scale, and creates the visual rig as `Stylized Adult Presentation`.

When no authoritative state is provided, the installer binds a deterministic seed-1729 Juniper state so idle mood remains stable. Gameplay should retain the returned `ResidentPresentationController` and rebind the actual save-loaded `ResidentState`. Gameplay events should call the reaction hook after the corresponding action has been validated and executed.

### Exact remaining gaps

- Gameplay does not yet pass its authoritative/save-loaded resident state into the installer; the automatic hook currently uses the deterministic fallback state.
- Gameplay action completion events are not yet mapped to presentation reactions, so authored calls to `SetReaction` are required for item-specific recoil, celebration, sleep, cleaning disgust, and escape strain.
- The gameplay-owned UI caption still says `PLACEHOLDER TARGET`; changing that text is outside Character ownership.
- The rig does not yet walk to targets, solve hand IK, align grips to item geometry, or synchronize contact frames with gameplay physics.
- Mouth shapes communicate emotion but are not connected to phoneme/viseme timing; Voice integration owns that later bridge.
- There is one authored procedural appearance. Seeded skin, hair, clothing, body-shape, and accessory variants remain future Character Art scope.
- This is polished procedural slice art, not a final skinned production mesh. Final topology, deformation, authored animation clips, LODs, and platform profiling remain deferred under the vertical-slice exit gate.
