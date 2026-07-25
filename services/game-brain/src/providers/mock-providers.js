import { stableHex } from "../lib/values.js";

function titleCase(prompt) {
  return prompt
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 8)
    .map((word) => word.charAt(0).toLocaleUpperCase("en-US") + word.slice(1))
    .join(" ");
}

const dialogueLines = {
  approach: "All right, I'll take a closer look.",
  avoid: "No. I'm keeping my distance from that.",
  grab: "I've got it.",
  eat: "I could eat. Let me check it first.",
  drink: "Finally, something to drink.",
  throw: "Heads up.",
  strike: "Let's see what this can do.",
  cut: "Hold still. I can cut this.",
  clean: "Good. This place needs cleaning.",
  wear: "I suppose this might fit.",
  rest: "I'm going to rest for a while.",
  play: "All right—let's play.",
  signal: "Watch the light. I'm trying to tell you something.",
  attempt_escape: "I'm only testing the edge. Don't make a fuss.",
  speak: "I'm listening.",
  observe: "Give me a moment to look at it."
};

const dialogueEmotions = {
  avoid: "disgust",
  eat: "joy",
  drink: "relief",
  play: "joy",
  attempt_escape: "curiosity",
  observe: "curiosity"
};

export class MockItemResolverProvider {
  name = "mock";

  async resolve({ prompt }) {
    const displayName = titleCase(prompt) || "Unknown Item";
    return JSON.stringify({
      displayName,
      visual: {
        archetype: "idea_object",
        colorHex: stableHex(prompt),
        scale: [0.3, 0.3, 0.3]
      },
      physical: {
        massKg: 0.25,
        bounciness: 0.1,
        softness: 0.3,
        fragility: 0.2,
        sharpness: 0,
        dirtiness: 0
      },
      consumable: null,
      capabilities: ["grabbable"],
      tags: ["unknown", "mock_resolved"]
    });
  }
}

export class MockDialogueProvider {
  name = "mock";

  async generate(request) {
    const action = [...request.legalActions].sort(
      (left, right) => right.utilityHint - left.utilityHint
        || left.actionId.localeCompare(right.actionId)
    )[0];
    const intent = action?.verb || "observe";
    return JSON.stringify({
      spokenLine: dialogueLines[intent],
      emotion: dialogueEmotions[intent] || "neutral",
      intensity: action ? Math.min(1, Math.max(0, Math.abs(action.utilityHint) / 100)) : 0.2,
      selectedActionId: action?.actionId ?? null,
      memoryNote: action
        ? `I chose ${intent} because of ${action.reasonCode}.`
        : "No safe action was available."
    });
  }
}

export class MockMemoryProvider {
  name = "mock";

  async summarize(request) {
    const descriptions = request.events.map((event) => event.description);
    return JSON.stringify({
      summary: descriptions.join(" ").slice(0, 1000),
      facts: descriptions.slice(0, 5),
      promises: descriptions.filter((description) => /\bpromis(?:e|ed)\b/i.test(description)).slice(0, 10)
    });
  }
}

export class MockTranscriptionProvider {
  name = "mock";

  async transcribe(request) {
    return {
      transcript: "Hello in there.",
      language: request.language || "en",
      durationSeconds: request.durationSeconds
    };
  }
}

export class MockSpeechProvider {
  name = "mock";

  async synthesize() {
    return {
      audioBase64: "UklGRgAAAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQAAAAA=",
      mimeType: "audio/wav"
    };
  }
}

export function createMockProviders() {
  return {
    item: new MockItemResolverProvider(),
    dialogue: new MockDialogueProvider(),
    memory: new MockMemoryProvider(),
    transcription: new MockTranscriptionProvider(),
    speech: new MockSpeechProvider()
  };
}
