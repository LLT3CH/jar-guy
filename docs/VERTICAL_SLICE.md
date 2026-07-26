# Vertical Slice

## Purpose

Prove the emotional and systemic loop before scaling the item catalog, art quality, or dialogue cost.

## Playable scope

### Scene

- One transparent jar with readable interior bounds and a movable lid.
- One stylized adult resident with idle, look, walk, grab, eat, drink, play, recoil, speak, sleep, and attempt-escape states.
- Desktop mouse and Android touch input share the same gesture logic.
- The camera frames the jar at all supported aspect ratios.

### Lid and item input

- Horizontal drag of at least 20% of screen width slides the lid open.
- The lid visually transforms into or reveals a focused search field.
- Enter/submit closes the field, restores the lid, resolves the item, and spawns it above the jar.
- Escape/back/cancel restores the lid without spawning.
- Empty, duplicate, unsupported, and unsafe prompts produce intentional feedback.

### Initial catalog

1. apple
2. chocolate cake
3. water bottle
4. dog feces
5. rubber ball
6. baseball bat
7. hockey stick
8. blanket
9. rope
10. scissors
11. sponge
12. flashlight

Each item needs a recognizable lightweight prototype visual, physics profile, capabilities, appraisal effects, and at least one meaningful action. Generic gray boxes are not sufficient for the authored catalog.

### Required combinations

- rubber ball + baseball bat → strike/play;
- rubber ball + hockey stick → strike/play;
- rope + scissors → cut rope;
- sponge + water → clean dirty surfaces or the resident;
- flashlight → illumination, play, or signaling;
- bat/hockey stick + jar/lid → an evaluated escape attempt;
- dog feces + sponge/water → cleaning path with persistent disgust and hygiene consequences.

### Gameplay implementation checkpoint — 2026-07-25

The current gameplay slice implements recognizable low-poly composite visuals for all twelve catalog items. The silhouettes and details distinguish food, drink, waste, ball, bat, hockey stick, blanket, rope, scissors, sponge, and flashlight while keeping their capability-based physics and affordances authoritative. Unknown prompts use a deliberately styled idea parcel rather than an arbitrary prefab or generic catalog block.

The jar now has a pedestal and glass highlight, the lid has a readable grip band, and Juniper is represented by the Character presentation rig with a lightweight Gameplay fallback target. Item drops produce an immediate reaction badge and invoke explicit inspect, celebrate, comfort, recoil, or disgust poses through `ResidentPresentationController.SetReaction`; the Gameplay motion/color cue remains a fallback when the presentation rig is absent. Newly unlocked item-to-item affordances produce a visible combination banner and celebrate pose; this includes the required ball plus bat/hockey-stick strike path.

Remaining gaps are explicit:

- the procedural meshes and adult target are prototype presentation, not final character or prop art;
- gameplay reaction cues drive explicit Character presentation poses but not yet the Character system's full appraisal, needs, memory, or persistence;
- cleaning, eating, injury, jar damage, escape progress, and long-lived hygiene consequences remain affordances rather than executed simulations;
- save/load, voice, dialogue, and service-driven unknown-item specifications require the later integration passes;
- Android device profiling and the 30-object stress target still require integrated build validation.

### Personality and state

- Stable random seed creates a reproducible resident.
- At least eight behavioral traits and nine needs influence utility scores.
- Likes/dislikes alter appraisal but never override physical safety.
- Mood changes over time and after events.
- The resident remembers at least the last 25 important events and a relationship summary.
- Save/load restores the seed, needs, relationship, items, and memories.

### Conversation and voice

- Text conversation works without a microphone.
- Push-to-talk visibly indicates recording and requires explicit permission.
- Speech-to-text, dialogue, and text-to-speech are service interfaces with mock implementations.
- Dialogue service returns structured JSON: spoken line, emotion, validated intent, targets, and compact memory.
- The client rejects actions that are impossible, unsafe for the simulation, or reference missing entities.
- Network failure produces an in-character local response and never blocks physics.

## Performance targets

- 60 FPS target on a mid-range Windows PC.
- 30 FPS minimum on the selected Android baseline device.
- No LLM or network operation on the Unity main thread.
- Item resolution feedback begins within 150 ms; a loading flourish covers remote latency.
- Physics remains stable with 30 ordinary objects in the jar.

## Acceptance scenarios

### VS-01: Delight

Given a hungry, generally cheerful resident, when the player drops an apple, the resident approaches, appraises, eats, speaks positively, and reduces hunger.

### VS-02: Disgust

When dog feces enters a clean jar, the resident recoils, avoids contact when possible, complains, loses hygiene/comfort, and remembers who caused it.

### VS-03: Emergence

Given a rubber ball, adding either a bat or hockey stick creates a valid strike action without a hardcoded `ball + exact item ID` pair.

### VS-04: Agency

Given a resourceful, freedom-driven resident and a usable rigid tool, the resident may test the lid or jar boundary. A comfort-driven resident may decline the same attempt.

### VS-05: Persistence

After saving, closing, and loading, the resident recognizes a repeated gift and retains relationship sentiment.

### VS-06: Voice denial

If microphone permission is denied, all typed play remains available and the game does not request permission repeatedly.

### VS-07: Offline degradation

If the brain service is unreachable, authored item behavior, needs, physics, local barks, and save/load continue.

## Not in this slice

- limitless bespoke 3D generation;
- multiplayer or public sharing;
- user-authored mods;
- photorealistic avatars;
- multiple residents in one jar;
- procedural world outside the jar;
- production monetization;
- final moderation/rating implementation;
- launch-scale localization.

## Exit gate

Do not begin mass asset production until five external testers complete the seven scenarios and at least four describe the resident as distinct, memorable, and responsive.
