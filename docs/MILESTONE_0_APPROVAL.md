# Milestone 0 — Foundation Approval

Status: **Approved**

Date: 2026-07-25

## Approved environment

- Unity 6.5.5f1
- Universal Render Pipeline
- Windows standalone support
- Android Build Support
- Android SDK and platform tools
- Android NDK r27c
- CMake 3.22.1
- OpenJDK 17
- Node.js game-brain service

## Verification record

### Gameplay

- Unity project imports and compiles without errors.
- `JarLoop.unity` is generated, opens, and validates successfully.
- Gameplay EditMode tests: 10/10 passed.
- Gameplay PlayMode tests: 3/3 passed.
- Full project EditMode tests: 28/28 passed.
- Mouse/touch pointer abstraction, lid gesture, prompt submission, item spawning, falling physics, and capability pairing are covered.

### Resident character

- Character Unity EditMode tests: 18/18 passed.
- Runtime and test assemblies compile in the live Unity project.
- Seeded personality, preferences, nine needs, appraisal, mood, relationship state, episodic memory, utility planning, intent validation, and save DTOs are implemented.

### Game brain

- Node tests: 19/19 passed.
- Key-free `/health` and item-resolution smoke tests passed.
- `dog shit` resolves to `dog_feces`.
- Unknown items, malformed provider responses, illegal paths, unavailable targets, timeouts, offline providers, and unsafe output fail closed.
- Executable action authority stays with the Unity client.

## Director decision

The repository is approved as a coherent engineering foundation and first playable jar loop.

This approval does not claim the commercial vertical slice is finished. Milestone 1 must integrate the three approved foundations into one continuous resident experience.

## Milestone 1 integration gates

1. Map gameplay `Affordance` values to character `LegalActionOffer` and contract `ActionOffer` records.
2. Feed item-drop, collision, consumption, cleaning, play, and escape events into appraisal, needs, relationship state, and memory.
3. Put the resident decision engine into `JarLoop.unity` with visible placeholder actions and reactions.
4. Connect the Unity client to the local game-brain service through a validated asynchronous adapter.
5. Preserve fully functional offline behavior.
6. Save and reload the resident seed, state, memories, relationship, and jar items.
7. Produce and smoke-test Windows and Android development builds.

## Deferred production gates

- final character model, rig, animation, voice, audio, and environment art;
- production speech-to-text, LLM, and text-to-speech providers;
- authentication, quotas, privacy retention controls, and live operations;
- store disclosures, ratings, accessibility review, and external playtesting;
- launch-scale object catalog and unknown-item visual generation.
