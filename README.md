# Human Glass Watcher

Working title for a character-driven sandbox game for Steam and Android.

The player observes a small, fully voiced adult human living inside a glass jar. Sliding the lid open turns it into a search field. The player can name an object, drop it into the jar, and watch the character interpret, use, combine, avoid, enjoy, or exploit it. The character has persistent needs, memories, relationships, preferences, and escape plans.

## Product pillars

1. **Anything in, consequences out** — broad natural-language item entry with believable fallbacks.
2. **A person, not a vending machine** — persistent personality, needs, memory, voice, and agency.
3. **Systems create stories** — capability-based interactions make tools and objects combine naturally.
4. **Conversation matters** — typed and opt-in voice conversation changes trust, mood, and behavior.
5. **The jar is a puzzle** — resourceful characters can repurpose almost anything toward escape.

## Current milestone

The repository is at **Milestone 0: Foundation**. The immediate target is a vertical slice with:

- one stylized jar scene and one placeholder adult character;
- mouse and touch lid gestures;
- a text item prompt and a falling object;
- twelve authored items plus a generic unknown-item fallback;
- needs, personality, reactions, memory, and capability-based interactions;
- typed conversation plus interfaces for speech-to-text, LLM dialogue, and text-to-speech;
- Windows and Android development builds.

See [the vertical-slice definition](docs/VERTICAL_SLICE.md), [technical architecture](docs/TECHNICAL_ARCHITECTURE.md), and [agent work orders](docs/AGENT_WORK_ORDERS.md).

## Locked foundation decisions

- **Engine:** Unity 6.5.5f1, C#, Universal Render Pipeline.
- **Presentation:** stylized 3D diorama with readable, toy-scale physics.
- **Simulation:** deterministic local systems own physics, needs, affordances, saves, and action validation.
- **AI:** a server-side “game brain” interprets unknown items and generates structured dialogue/intent.
- **Voice:** push-to-talk first; microphone use is optional and visibly indicated.
- **Security:** provider keys never ship in the game client.
- **Character scope:** all jar characters are fictional adults in the first release.

## Definition of “anything”

The player-facing promise is “name almost anything and receive a behaviorally believable version.” It is not a promise of a bespoke, photorealistic 3D model for every noun. Known items use authored assets; related items reuse parametric visual archetypes; unknown items receive a safe generated specification and a clear stylized fallback until the asset library grows.
