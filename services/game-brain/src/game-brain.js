import { resolveKnownItem } from "./catalog.js";
import { contracts, ContractValidationError } from "./lib/contract-registry.js";
import { TtlCache } from "./lib/cache.js";
import { withTimeout } from "./lib/timeout.js";
import {
  clamp,
  CONTRACT_VERSION,
  normalizePrompt,
  RESOLVER_VERSION,
  safeText,
  slug,
  stableHex,
  uniqueAllowed
} from "./lib/values.js";
import { RuleBasedModeration } from "./moderation.js";
import { createMockProviders } from "./providers/mock-providers.js";

const CAPABILITIES = new Set([
  "grabbable", "throwable", "bouncy", "edible", "drinkable", "swing_tool",
  "sharp_edge", "flexible_line", "absorbent", "cleaning_agent", "light_source",
  "comfort", "wearable", "container", "lever", "adhesive", "flammable", "dirty",
  "toxic", "fragile", "entertainment"
]);

const ARCHETYPES = new Set([
  "sphere", "box", "cylinder", "bottle", "food", "cloth", "tool", "organic",
  "idea_object"
]);

const CONTENT_LABELS = new Set([
  "none", "gross", "violence", "sexual", "self_harm", "illegal", "hate",
  "personal_data", "copyright_risk"
]);

const EMOTIONS = new Set([
  "neutral", "joy", "curiosity", "sadness", "fear", "anger", "disgust",
  "surprise", "contempt", "relief"
]);

function parseProviderJson(raw) {
  if (typeof raw === "string") return JSON.parse(raw);
  if (raw && typeof raw === "object" && !Array.isArray(raw)) return raw;
  throw new TypeError("Provider output must be a JSON object or JSON object string.");
}

function labelsFor(moderation) {
  const labels = uniqueAllowed(moderation.labels, CONTENT_LABELS, 9);
  return labels.length ? labels : ["none"];
}

function genericUnknown(prompt, moderation, tag = "generated_fallback") {
  const promptSlug = slug(prompt, "item", 48);
  const restricted = moderation.decision !== "allow";
  return {
    contractVersion: CONTRACT_VERSION,
    resolverVersion: RESOLVER_VERSION,
    canonicalId: restricted ? "unknown_restricted" : `unknown_${promptSlug}`.slice(0, 64),
    displayName: restricted ? "Unsupported Item" : safeText(prompt, 80, "Unknown Item"),
    sourcePrompt: safeText(prompt, 160, "unknown item"),
    authoredAssetId: null,
    visual: {
      archetype: "idea_object",
      colorHex: restricted ? "#6B6B6B" : stableHex(prompt),
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
    capabilities: restricted ? [] : ["grabbable"],
    tags: restricted ? ["unknown", "moderated"] : ["unknown", tag],
    content: {
      decision: moderation.decision,
      labels: labelsFor(moderation)
    }
  };
}

function sanitizeScale(value) {
  if (!Array.isArray(value) || value.length !== 3) return [0.3, 0.3, 0.3];
  return value.map((entry) => clamp(entry, 0.05, 3, 0.3));
}

function sanitizeConsumable(value) {
  if (!value || typeof value !== "object" || Array.isArray(value)) return null;
  return {
    nutrition: clamp(value.nutrition, -1, 1, 0),
    hydration: clamp(value.hydration, -1, 1, 0),
    toxicity: clamp(value.toxicity, 0, 1, 0),
    taste: clamp(value.taste, -1, 1, 0)
  };
}

function sanitizeTags(values) {
  if (!Array.isArray(values)) return ["unknown"];
  const tags = [...new Set(values.map((value) => slug(value, "", 48)).filter(Boolean))].slice(0, 30);
  return [...new Set(["unknown", ...tags])].slice(0, 32);
}

function sanitizeUnknown(prompt, candidate, moderation) {
  const sourcePrompt = safeText(prompt, 160, "unknown item");
  const archetype = ARCHETYPES.has(candidate.visual?.archetype)
    ? candidate.visual.archetype
    : "idea_object";
  const colorHex = /^#[0-9A-Fa-f]{6}$/.test(candidate.visual?.colorHex)
    ? candidate.visual.colorHex.toUpperCase()
    : stableHex(prompt);
  const capabilities = uniqueAllowed(candidate.capabilities, CAPABILITIES, 24);
  const consumable = capabilities.some((capability) => capability === "edible" || capability === "drinkable")
    ? sanitizeConsumable(candidate.consumable)
    : null;

  return {
    contractVersion: CONTRACT_VERSION,
    resolverVersion: RESOLVER_VERSION,
    canonicalId: `unknown_${slug(prompt, "item", 48)}`.slice(0, 64),
    displayName: safeText(candidate.displayName, 80, sourcePrompt),
    sourcePrompt,
    authoredAssetId: null,
    visual: {
      archetype,
      colorHex,
      scale: sanitizeScale(candidate.visual?.scale)
    },
    physical: {
      massKg: clamp(candidate.physical?.massKg, 0.001, 50, 0.25),
      bounciness: clamp(candidate.physical?.bounciness, 0, 1, 0.1),
      softness: clamp(candidate.physical?.softness, 0, 1, 0.3),
      fragility: clamp(candidate.physical?.fragility, 0, 1, 0.2),
      sharpness: clamp(candidate.physical?.sharpness, 0, 1, 0),
      dirtiness: clamp(candidate.physical?.dirtiness, 0, 1, 0)
    },
    consumable,
    capabilities,
    tags: sanitizeTags(candidate.tags),
    content: {
      decision: moderation.decision,
      labels: labelsFor(moderation)
    }
  };
}

function fallbackDialogue(turnId, spokenLine = "I need a moment to think.", memoryNote = "") {
  return {
    contractVersion: CONTRACT_VERSION,
    turnId,
    spokenLine,
    emotion: "neutral",
    intensity: 0.2,
    selectedActionId: null,
    selectedIntent: "observe",
    targetEntityIds: [],
    memoryNote
  };
}

function uniqueValidActions(request) {
  const knownEntities = new Set(request.knownEntityIds);
  const actionIds = new Set();
  return request.legalActions.filter((action) => {
    if (actionIds.has(action.actionId)) return false;
    actionIds.add(action.actionId);
    return action.targetEntityIds.every((targetId) => knownEntities.has(targetId));
  });
}

function providerName(provider, fallback = "mock") {
  const safe = slug(provider?.name, fallback, 32).replace(/_+$/g, "");
  return safe || fallback;
}

export class GameBrain {
  constructor({
    providers = createMockProviders(),
    moderation = new RuleBasedModeration(),
    providerTimeoutMs = 1500,
    cache = new TtlCache()
  } = {}) {
    this.providers = providers;
    this.moderation = moderation;
    this.providerTimeoutMs = providerTimeoutMs;
    this.cache = cache;
  }

  async resolveItem(request) {
    return (await this.resolveItemWithMeta(request)).value;
  }

  async resolveItemWithMeta(request) {
    contracts.assert("item-resolution-request.schema.json", request);
    const prompt = safeText(request.prompt, 160);
    if (!prompt) {
      throw new ContractValidationError("item-resolution-request.schema.json", [
        "$.prompt: must contain non-whitespace text"
      ]);
    }

    const moderation = await this.moderation.moderateInput(prompt, { kind: "item_prompt" });
    const known = resolveKnownItem(prompt);
    if (known && moderation.decision === "allow") {
      contracts.assert("item-spec.schema.json", known);
      return { value: known, cacheStatus: "LOCAL" };
    }

    if (moderation.decision !== "allow") {
      const restricted = genericUnknown(prompt, moderation);
      contracts.assert("item-spec.schema.json", restricted);
      return { value: restricted, cacheStatus: "MODERATED" };
    }

    const cacheKey = `${RESOLVER_VERSION}:${normalizePrompt(prompt)}`;
    const cached = this.cache.get(cacheKey);
    if (cached) {
      cached.sourcePrompt = prompt;
      contracts.assert("item-spec.schema.json", cached);
      return { value: cached, cacheStatus: "HIT" };
    }

    let result;
    try {
      const raw = await withTimeout(
        (signal) => this.providers.item.resolve({ prompt, signal }),
        this.providerTimeoutMs
      );
      const candidate = parseProviderJson(raw);
      const outputModeration = await this.moderation.moderateItemCandidate(candidate);
      result = outputModeration.allowed
        ? sanitizeUnknown(prompt, candidate, moderation)
        : genericUnknown(prompt, moderation, "unsafe_output_fallback");
    } catch {
      result = genericUnknown(prompt, moderation);
    }

    contracts.assert("item-spec.schema.json", result);
    this.cache.set(cacheKey, result);
    return { value: result, cacheStatus: "MISS" };
  }

  async createDialogueTurn(request) {
    contracts.assert("dialogue-request.schema.json", request);
    const moderation = await this.moderation.moderateInput(request.playerMessage, {
      kind: "dialogue_input"
    });
    if (moderation.decision === "deny") {
      const denied = fallbackDialogue(
        request.turnId,
        "I'm not engaging with that.",
        "The player used language I would not engage with."
      );
      contracts.assert("dialogue-turn.schema.json", denied);
      return denied;
    }

    const legalActions = uniqueValidActions(request);
    const offersById = new Map(legalActions.map((action) => [action.actionId, action]));

    try {
      const raw = await withTimeout(
        (signal) => this.providers.dialogue.generate({ ...request, legalActions, signal }),
        this.providerTimeoutMs
      );
      const candidate = parseProviderJson(raw);
      const selected = typeof candidate.selectedActionId === "string"
        ? offersById.get(candidate.selectedActionId)
        : null;
      if (!selected && candidate.selectedActionId !== null) {
        throw new Error("Provider selected an action that was not offered.");
      }
      const moderatedOutput = await this.moderation.moderateDialogueOutput(candidate.spokenLine);
      if (!moderatedOutput.allowed) throw new Error("Dialogue output was moderated.");

      const result = {
        contractVersion: CONTRACT_VERSION,
        turnId: request.turnId,
        spokenLine: moderatedOutput.text,
        emotion: EMOTIONS.has(candidate.emotion) ? candidate.emotion : "neutral",
        intensity: clamp(candidate.intensity, 0, 1, 0.2),
        selectedActionId: selected?.actionId ?? null,
        selectedIntent: selected?.verb ?? "observe",
        targetEntityIds: selected ? [...selected.targetEntityIds] : [],
        memoryNote: safeText(candidate.memoryNote, 240)
      };
      contracts.assert("dialogue-turn.schema.json", result);
      return result;
    } catch {
      const fallback = fallbackDialogue(request.turnId);
      contracts.assert("dialogue-turn.schema.json", fallback);
      return fallback;
    }
  }

  async summarizeMemory(request) {
    contracts.assert("memory-summary-request.schema.json", request);
    let candidate;
    try {
      const raw = await withTimeout(
        (signal) => this.providers.memory.summarize({ ...request, signal }),
        this.providerTimeoutMs
      );
      candidate = parseProviderJson(raw);
    } catch {
      candidate = {
        summary: request.events.map((event) => event.description).join(" "),
        facts: request.events.map((event) => event.description).slice(0, 5),
        promises: []
      };
    }

    const summaryModeration = await this.moderation.moderateDialogueOutput(candidate.summary);
    if (!summaryModeration.allowed) {
      candidate = { summary: "Earlier events were summarized locally.", facts: [], promises: [] };
    }
    const result = {
      contractVersion: CONTRACT_VERSION,
      residentId: request.residentId,
      summary: safeText(candidate.summary, 1000),
      facts: [...new Set((Array.isArray(candidate.facts) ? candidate.facts : [])
        .map((fact) => safeText(fact, 240)).filter(Boolean))].slice(0, 20),
      promises: [...new Set((Array.isArray(candidate.promises) ? candidate.promises : [])
        .map((promise) => safeText(promise, 240)).filter(Boolean))].slice(0, 10),
      sourceEventIds: request.events.map((event) => event.eventId)
    };
    contracts.assert("memory-summary.schema.json", result);
    return result;
  }

  async transcribe(request) {
    contracts.assert("voice-transcription-request.schema.json", request);
    let candidate;
    let name;
    try {
      candidate = await withTimeout(
        (signal) => this.providers.transcription.transcribe({ ...request, signal }),
        this.providerTimeoutMs
      );
      name = providerName(this.providers.transcription);
    } catch {
      candidate = { transcript: "", language: request.language || "en", durationSeconds: request.durationSeconds };
      name = "offline_fallback";
    }
    const result = {
      contractVersion: CONTRACT_VERSION,
      transcript: safeText(candidate.transcript, 1000),
      language: /^[A-Za-z]{2,3}(?:-[A-Za-z]{2})?$/.test(candidate.language)
        ? candidate.language
        : "en",
      durationSeconds: clamp(candidate.durationSeconds, 0.01, 30, request.durationSeconds),
      provider: name
    };
    contracts.assert("voice-transcription-result.schema.json", result);
    return result;
  }

  async synthesize(request) {
    contracts.assert("voice-synthesis-request.schema.json", request);
    let candidate;
    let name;
    try {
      candidate = await withTimeout(
        (signal) => this.providers.speech.synthesize({ ...request, signal }),
        this.providerTimeoutMs
      );
      name = providerName(this.providers.speech);
    } catch {
      candidate = {
        audioBase64: "UklGRgAAAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQAAAAA=",
        mimeType: "audio/wav"
      };
      name = "offline_fallback";
    }
    const result = {
      contractVersion: CONTRACT_VERSION,
      audioBase64: typeof candidate.audioBase64 === "string" && candidate.audioBase64
        ? candidate.audioBase64.slice(0, 4_000_000)
        : "UklGRg==",
      mimeType: ["audio/wav", "audio/mpeg", "audio/ogg"].includes(candidate.mimeType)
        ? candidate.mimeType
        : "audio/wav",
      provider: name
    };
    contracts.assert("voice-synthesis-result.schema.json", result);
    return result;
  }
}
