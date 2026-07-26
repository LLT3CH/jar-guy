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

function createMockAudioCueBase64() {
  const sampleRate = 16_000;
  const sampleCount = 4000;
  const buffer = Buffer.alloc(44 + (sampleCount * 2));
  buffer.write("RIFF", 0);
  buffer.writeUInt32LE(buffer.length - 8, 4);
  buffer.write("WAVE", 8);
  buffer.write("fmt ", 12);
  buffer.writeUInt32LE(16, 16);
  buffer.writeUInt16LE(1, 20);
  buffer.writeUInt16LE(1, 22);
  buffer.writeUInt32LE(sampleRate, 24);
  buffer.writeUInt32LE(sampleRate * 2, 28);
  buffer.writeUInt16LE(2, 32);
  buffer.writeUInt16LE(16, 34);
  buffer.write("data", 36);
  buffer.writeUInt32LE(sampleCount * 2, 40);
  for (let index = 0; index < sampleCount; index += 1) {
    const envelope = Math.min(1, index / 320, (sampleCount - index) / 320);
    const sample = Math.sin((index / sampleRate) * Math.PI * 2 * 440) * 0.12 * envelope;
    buffer.writeInt16LE(Math.round(sample * 32767), 44 + (index * 2));
  }
  return buffer.toString("base64");
}

export const MOCK_AUDIO_CUE_WAV_BASE64 = createMockAudioCueBase64();

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
    const playerMessage = String(request.playerMessage || "").trim();
    const memory = request.conversationContext?.memorySummary || "";
    let spokenLine = dialogueLines[intent];
    if (/\b(?:hello|hi|hey)\b/i.test(playerMessage)) {
      spokenLine = "Hello. The acoustics in this jar are strange, but I heard you.";
    } else if (/\bremember\b/i.test(playerMessage)) {
      spokenLine = memory
        ? "I remember. You don't get to decide which parts matter to me."
        : "Not yet. Give me something worth remembering.";
    } else if (playerMessage) {
      spokenLine = `I heard you say, "${playerMessage.slice(0, 120)}"`;
    }
    return JSON.stringify({
      spokenLine,
      emotion: dialogueEmotions[intent] || "neutral",
      intensity: action ? Math.min(1, Math.max(0, Math.abs(action.utilityHint) / 100)) : 0.2,
      selectedActionId: action?.actionId ?? null,
      memoryNote: playerMessage
        ? `The player said: ${playerMessage.slice(0, 200)}`
        : action
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
      audioBase64: MOCK_AUDIO_CUE_WAV_BASE64,
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
