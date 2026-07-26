import assert from "node:assert/strict";
import { test } from "node:test";
import { loadConfig } from "../src/config.js";
import { createConfiguredProviders } from "../src/providers/provider-factory.js";
import {
  createOpenAIVoiceProviders,
  OpenAIProviderError
} from "../src/providers/openai-providers.js";

function config(overrides = {}) {
  return {
    ...loadConfig({
      GAME_BRAIN_PROVIDER: "openai",
      OPENAI_API_KEY: "server-only-test-key",
      OPENAI_BASE_URL: "https://provider.invalid/v1",
      OPENAI_DIALOGUE_MODEL: "dialogue-test-model",
      OPENAI_TRANSCRIPTION_MODEL: "transcription-test-model",
      OPENAI_TTS_MODEL: "speech-test-model",
      OPENAI_TTS_VOICE: "cedar",
      OPENAI_SAFETY_IDENTIFIER: "privacy-safe-user"
    }),
    ...overrides
  };
}

function jsonResponse(value, status = 200) {
  return {
    ok: status >= 200 && status < 300,
    status,
    async json() { return value; },
    async arrayBuffer() {
      return Uint8Array.from([82, 73, 70, 70]).buffer;
    }
  };
}

test("openai mode requires a server-side key before startup", () => {
  const noKey = loadConfig({ GAME_BRAIN_PROVIDER: "openai" });
  assert.throws(
    () => createConfiguredProviders(noKey),
    /OPENAI_API_KEY is required/
  );
});

test("configured openai mode leaves item and memory on deterministic local providers", () => {
  const providers = createConfiguredProviders(config(), {
    fetchImpl: async () => jsonResponse({})
  });
  assert.equal(providers.item.name, "mock");
  assert.equal(providers.memory.name, "mock");
  assert.equal(providers.dialogue.name, "openai");
  assert.equal(providers.transcription.name, "openai");
  assert.equal(providers.speech.name, "openai");
});

test("transcription posts bounded audio as multipart without exposing the key in payload", async () => {
  let call;
  const providers = createOpenAIVoiceProviders(config(), {
    fetchImpl: async (url, options) => {
      call = { url, options };
      return jsonResponse({ text: "I brought an apple.", language: "en" });
    }
  });
  const result = await providers.transcription.transcribe({
    audioBase64: Buffer.from("RIFF test audio").toString("base64"),
    mimeType: "audio/wav",
    durationSeconds: 0.5,
    language: "en-US"
  });

  assert.equal(call.url, "https://provider.invalid/v1/audio/transcriptions");
  assert.equal(call.options.headers.authorization, "Bearer server-only-test-key");
  assert.ok(call.options.body instanceof FormData);
  assert.equal(call.options.body.get("model"), "transcription-test-model");
  assert.equal(call.options.body.get("language"), "en");
  assert.equal(String(call.options.body).includes("server-only-test-key"), false);
  assert.equal(result.transcript, "I brought an apple.");
  assert.equal(result.durationSeconds, 0.5);
});

test("dialogue sends personality and memory through strict structured output", async () => {
  let requestBody;
  const candidate = {
    spokenLine: "Yes, I remember the apple.",
    emotion: "curiosity",
    intensity: 0.6,
    selectedActionId: "speak_reply",
    memoryNote: "The player asked about the apple."
  };
  const providers = createOpenAIVoiceProviders(config(), {
    fetchImpl: async (_url, options) => {
      requestBody = JSON.parse(options.body);
      return jsonResponse({ output_text: JSON.stringify(candidate) });
    }
  });
  const output = await providers.dialogue.generate({
    playerMessage: "Do you remember the apple?",
    residentState: "Calm and attentive.",
    conversationContext: {
      residentId: "resident_1",
      personality: "Wry and cautious.",
      memorySummary: "The player supplied an apple.",
      recentTurns: []
    },
    legalActions: [{
      actionId: "speak_reply",
      verb: "speak",
      targetEntityIds: ["resident_1"],
      utilityHint: 50,
      reasonCode: "conversation_reply"
    }]
  });

  assert.deepEqual(JSON.parse(output), candidate);
  assert.equal(requestBody.model, "dialogue-test-model");
  assert.equal(requestBody.store, false);
  assert.equal(requestBody.safety_identifier, "privacy-safe-user");
  assert.equal(requestBody.text.format.type, "json_schema");
  assert.equal(requestBody.text.format.strict, true);
  assert.equal(requestBody.text.format.schema.additionalProperties, false);
  assert.ok(requestBody.input[1].content.includes("Wry and cautious."));
  assert.ok(requestBody.input[1].content.includes("The player supplied an apple."));
});

test("speech uses only the server-configured voice and returns WAV bytes", async () => {
  let requestBody;
  const providers = createOpenAIVoiceProviders(config(), {
    fetchImpl: async (_url, options) => {
      requestBody = JSON.parse(options.body);
      return jsonResponse({});
    }
  });
  const result = await providers.speech.synthesize({
    text: "I heard you.",
    voiceId: "client_attempted_override"
  });

  assert.equal(requestBody.model, "speech-test-model");
  assert.equal(requestBody.voice, "cedar");
  assert.equal(requestBody.response_format, "wav");
  assert.equal(requestBody.voice.includes("client_attempted_override"), false);
  assert.equal(Buffer.from(result.audioBase64, "base64").toString("ascii"), "RIFF");
  assert.equal(result.mimeType, "audio/wav");
});

test("provider HTTP failures expose status but never the API key", async () => {
  const providers = createOpenAIVoiceProviders(config(), {
    fetchImpl: async () => jsonResponse({}, 401)
  });
  await assert.rejects(
    providers.speech.synthesize({ text: "hello", voiceId: "resident_default" }),
    (error) => {
      assert.ok(error instanceof OpenAIProviderError);
      assert.equal(error.status, 401);
      assert.equal(error.message.includes("server-only-test-key"), false);
      return true;
    }
  );
});
