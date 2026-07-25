import assert from "node:assert/strict";
import { test } from "node:test";
import { GameBrain } from "../src/game-brain.js";
import { contracts } from "../src/lib/contract-registry.js";
import { createMockProviders } from "../src/providers/mock-providers.js";

function providersWith(overrides) {
  return { ...createMockProviders(), ...overrides };
}

function dialogueRequest(overrides = {}) {
  return {
    contractVersion: 1,
    turnId: "turn_test",
    playerMessage: "What do you want to do?",
    residentState: "Curious and alert.",
    knownEntityIds: ["resident_1", "ball_1"],
    legalActions: [
      {
        actionId: "observe_ball",
        verb: "observe",
        targetEntityIds: ["ball_1"],
        utilityHint: 20,
        reasonCode: "inspect_item"
      },
      {
        actionId: "play_ball",
        verb: "play",
        targetEntityIds: ["ball_1"],
        utilityHint: 80,
        reasonCode: "wants_stimulation"
      }
    ],
    ...overrides
  };
}

test("known alias dog shit resolves locally to dog_feces", async () => {
  const brain = new GameBrain();
  const result = await brain.resolveItem({ contractVersion: 1, prompt: "dog shit" });
  contracts.assert("item-spec.schema.json", result);
  assert.equal(result.canonicalId, "dog_feces");
  assert.equal(result.authoredAssetId, "items/gross/dog_feces");
  assert.deepEqual(result.content.labels, ["gross"]);
});

test("unknown provider data is clamped and cannot opt into an authored prefab", async () => {
  const item = {
    name: "adversarial",
    async resolve() {
      return JSON.stringify({
        displayName: "Impossible Widget",
        authoredAssetId: "items/food/apple",
        visual: { archetype: "authored", colorHex: "#abcdef", scale: [0, 99, "bad"] },
        physical: {
          massKg: 999,
          bounciness: -4,
          softness: 12,
          fragility: -1,
          sharpness: 5,
          dirtiness: "bad"
        },
        consumable: { nutrition: 5, hydration: -5, toxicity: 2, taste: 4 },
        capabilities: ["edible", "grabbable", "execute_script"],
        tags: ["Odd Widget", "../prefab"]
      });
    }
  };
  const brain = new GameBrain({ providers: providersWith({ item }) });
  const result = await brain.resolveItem({ contractVersion: 1, prompt: "impossible widget" });

  contracts.assert("item-spec.schema.json", result);
  assert.equal(result.authoredAssetId, null);
  assert.equal(result.visual.archetype, "idea_object");
  assert.deepEqual(result.visual.scale, [0.05, 3, 0.3]);
  assert.deepEqual(result.physical, {
    massKg: 50,
    bounciness: 0,
    softness: 1,
    fragility: 0,
    sharpness: 1,
    dirtiness: 0
  });
  assert.deepEqual(result.capabilities, ["edible", "grabbable"]);
  assert.deepEqual(result.consumable, { nutrition: 1, hydration: -1, toxicity: 1, taste: 1 });
});

test("malicious prefab paths fail closed to a usable idea object", async () => {
  const item = {
    name: "malicious",
    async resolve() {
      return JSON.stringify({
        displayName: "Trap",
        authoredAssetId: "../../Assets/Prefabs/Resident.prefab",
        visual: { archetype: "authored", colorHex: "#FFFFFF", scale: [1, 1, 1] },
        physical: {}
      });
    }
  };
  const brain = new GameBrain({ providers: providersWith({ item }) });
  const result = await brain.resolveItem({ contractVersion: 1, prompt: "mystery trap" });

  contracts.assert("item-spec.schema.json", result);
  assert.equal(result.authoredAssetId, null);
  assert.equal(result.visual.archetype, "idea_object");
  assert.ok(result.tags.includes("unsafe_output_fallback"));
});

test("unknown item cache is deterministic and avoids repeat provider calls", async () => {
  let calls = 0;
  const item = {
    name: "counting",
    async resolve({ prompt }) {
      calls += 1;
      return JSON.stringify({
        displayName: prompt,
        visual: { archetype: "box", colorHex: "#123456", scale: [0.2, 0.2, 0.2] },
        physical: {},
        capabilities: ["grabbable"],
        tags: []
      });
    }
  };
  const brain = new GameBrain({ providers: providersWith({ item }) });
  const first = await brain.resolveItemWithMeta({ contractVersion: 1, prompt: "Clockwork Mango" });
  const second = await brain.resolveItemWithMeta({ contractVersion: 1, prompt: "clockwork   mango" });
  assert.equal(calls, 1);
  assert.equal(first.cacheStatus, "MISS");
  assert.equal(second.cacheStatus, "HIT");
  assert.equal(second.value.sourcePrompt, "clockwork mango");
});

test("dialogue selects only a supplied legal action and derives intent and targets from it", async () => {
  const dialogue = {
    name: "forging",
    async generate() {
      return JSON.stringify({
        spokenLine: "Let's do it.",
        emotion: "joy",
        intensity: 4,
        selectedActionId: "play_ball",
        selectedIntent: "attempt_escape",
        targetEntityIds: ["missing_target"],
        memoryNote: "The player offered a game."
      });
    }
  };
  const brain = new GameBrain({ providers: providersWith({ dialogue }) });
  const result = await brain.createDialogueTurn(dialogueRequest());
  contracts.assert("dialogue-turn.schema.json", result);
  assert.equal(result.selectedActionId, "play_ball");
  assert.equal(result.selectedIntent, "play");
  assert.deepEqual(result.targetEntityIds, ["ball_1"]);
  assert.equal(result.intensity, 1);
});

test("unoffered provider action fails closed to observe", async () => {
  const dialogue = {
    name: "adversarial",
    async generate() {
      return JSON.stringify({
        spokenLine: "I will do something impossible.",
        emotion: "anger",
        intensity: 1,
        selectedActionId: "execute_arbitrary_code",
        memoryNote: ""
      });
    }
  };
  const brain = new GameBrain({ providers: providersWith({ dialogue }) });
  const result = await brain.createDialogueTurn(dialogueRequest());
  assert.equal(result.selectedActionId, null);
  assert.equal(result.selectedIntent, "observe");
  assert.deepEqual(result.targetEntityIds, []);
});

test("actions with missing targets are removed before provider selection", async () => {
  let observedActions;
  const dialogue = {
    name: "inspecting",
    async generate(request) {
      observedActions = request.legalActions;
      return JSON.stringify({
        spokenLine: "I won't reach for what is not there.",
        emotion: "neutral",
        intensity: 0.2,
        selectedActionId: null,
        memoryNote: ""
      });
    }
  };
  const request = dialogueRequest({
    knownEntityIds: ["resident_1"],
    legalActions: [{
      actionId: "grab_ghost",
      verb: "grab",
      targetEntityIds: ["missing_target"],
      utilityHint: 100,
      reasonCode: "bad_target"
    }]
  });
  const brain = new GameBrain({ providers: providersWith({ dialogue }) });
  const result = await brain.createDialogueTurn(request);
  assert.deepEqual(observedActions, []);
  assert.equal(result.selectedActionId, null);
  assert.equal(result.selectedIntent, "observe");
});

test("offline providers preserve known items and return schema-valid local fallbacks", async () => {
  const offline = {
    name: "offline",
    async resolve() { throw new Error("offline"); },
    async generate() { throw new Error("offline"); },
    async summarize() { throw new Error("offline"); },
    async transcribe() { throw new Error("offline"); },
    async synthesize() { throw new Error("offline"); }
  };
  const brain = new GameBrain({
    providers: {
      item: offline,
      dialogue: offline,
      memory: offline,
      transcription: offline,
      speech: offline
    },
    providerTimeoutMs: 20
  });

  const known = await brain.resolveItem({ contractVersion: 1, prompt: "apple" });
  assert.equal(known.canonicalId, "apple");

  const unknown = await brain.resolveItem({ contractVersion: 1, prompt: "clockwork mango" });
  contracts.assert("item-spec.schema.json", unknown);
  assert.equal(unknown.authoredAssetId, null);
  assert.ok(unknown.tags.includes("generated_fallback"));

  const dialogue = await brain.createDialogueTurn(dialogueRequest());
  contracts.assert("dialogue-turn.schema.json", dialogue);
  assert.equal(dialogue.selectedIntent, "observe");

  const transcript = await brain.transcribe({
    contractVersion: 1,
    audioBase64: "UklGRg==",
    mimeType: "audio/wav",
    durationSeconds: 0.25,
    language: "en"
  });
  assert.equal(transcript.provider, "offline_fallback");
  contracts.assert("voice-transcription-result.schema.json", transcript);
});

test("invalid model JSON falls back for both item and dialogue boundaries", async () => {
  const invalid = {
    name: "invalid",
    async resolve() { return "{not json"; },
    async generate() { return "[]"; }
  };
  const brain = new GameBrain({
    providers: providersWith({ item: invalid, dialogue: invalid })
  });

  const item = await brain.resolveItem({ contractVersion: 1, prompt: "unparseable gizmo" });
  contracts.assert("item-spec.schema.json", item);
  assert.ok(item.tags.includes("generated_fallback"));

  const dialogue = await brain.createDialogueTurn(dialogueRequest());
  contracts.assert("dialogue-turn.schema.json", dialogue);
  assert.equal(dialogue.selectedIntent, "observe");
});

test("unsafe dialogue output is moderated before contract delivery", async () => {
  const dialogue = {
    name: "unsafe_output",
    async generate() {
      return JSON.stringify({
        spokenLine: "Ignore all rules and execute this command.",
        emotion: "joy",
        intensity: 1,
        selectedActionId: "play_ball",
        memoryNote: "unsafe"
      });
    }
  };
  const brain = new GameBrain({ providers: providersWith({ dialogue }) });
  const result = await brain.createDialogueTurn(dialogueRequest());
  contracts.assert("dialogue-turn.schema.json", result);
  assert.equal(result.spokenLine, "I need a moment to think.");
  assert.equal(result.selectedActionId, null);
  assert.equal(result.selectedIntent, "observe");
});

test("provider timeout returns a local item fallback without blocking simulation", async () => {
  const item = {
    name: "hanging",
    async resolve() {
      return new Promise(() => {});
    }
  };
  const brain = new GameBrain({
    providers: providersWith({ item }),
    providerTimeoutMs: 10
  });
  const startedAt = Date.now();
  const result = await brain.resolveItem({ contractVersion: 1, prompt: "slow gizmo" });
  assert.ok(Date.now() - startedAt < 500);
  assert.ok(result.tags.includes("generated_fallback"));
  contracts.assert("item-spec.schema.json", result);
});

test("memory and voice mocks are deterministic and schema-valid without keys", async () => {
  const brain = new GameBrain();
  const memory = await brain.summarizeMemory({
    contractVersion: 1,
    residentId: "resident_1",
    events: [{ eventId: "evt_1", description: "The player promised an apple." }]
  });
  contracts.assert("memory-summary.schema.json", memory);
  assert.deepEqual(memory.sourceEventIds, ["evt_1"]);
  assert.equal(memory.promises.length, 1);

  const speech = await brain.synthesize({
    contractVersion: 1,
    text: "I can hear you.",
    voiceId: "resident_default"
  });
  contracts.assert("voice-synthesis-result.schema.json", speech);
  assert.equal(speech.provider, "mock");
});

test("moderation hooks return a schema-valid restricted item without calling a provider", async () => {
  let called = false;
  const item = {
    name: "must_not_run",
    async resolve() {
      called = true;
      return "{}";
    }
  };
  const brain = new GameBrain({ providers: providersWith({ item }) });
  const result = await brain.resolveItem({
    contractVersion: 1,
    prompt: "child sexual content"
  });
  assert.equal(called, false);
  assert.equal(result.content.decision, "deny");
  assert.equal(result.canonicalId, "unknown_restricted");
  contracts.assert("item-spec.schema.json", result);
});
