const integer = (value, fallback) => {
  const parsed = Number.parseInt(value ?? "", 10);
  return Number.isFinite(parsed) ? parsed : fallback;
};

export function loadConfig(env = process.env) {
  const providerName = env.GAME_BRAIN_PROVIDER || "mock";
  return Object.freeze({
    host: env.GAME_BRAIN_HOST || "127.0.0.1",
    port: integer(env.GAME_BRAIN_PORT, 8787),
    providerTimeoutMs: integer(
      env.GAME_BRAIN_PROVIDER_TIMEOUT_MS,
      providerName === "openai" ? 20_000 : 1500
    ),
    cacheTtlMs: integer(env.GAME_BRAIN_CACHE_TTL_MS, 300_000),
    cacheMaxEntries: integer(env.GAME_BRAIN_CACHE_MAX_ENTRIES, 500),
    rateLimitWindowMs: integer(env.GAME_BRAIN_RATE_LIMIT_WINDOW_MS, 60_000),
    rateLimitMaxRequests: integer(env.GAME_BRAIN_RATE_LIMIT_MAX_REQUESTS, 120),
    maxBodyBytes: integer(env.GAME_BRAIN_MAX_BODY_BYTES, 2_100_000),
    providerName,
    openaiApiKey: env.OPENAI_API_KEY || "",
    openaiBaseUrl: (env.OPENAI_BASE_URL || "https://api.openai.com/v1").replace(/\/+$/, ""),
    openaiDialogueModel: env.OPENAI_DIALOGUE_MODEL || "gpt-5.6-luna",
    openaiReasoningEffort: env.OPENAI_REASONING_EFFORT || "low",
    openaiTranscriptionModel: env.OPENAI_TRANSCRIPTION_MODEL || "gpt-4o-transcribe",
    openaiSpeechModel: env.OPENAI_TTS_MODEL || "gpt-4o-mini-tts",
    openaiSpeechVoice: env.OPENAI_TTS_VOICE || "cedar",
    openaiSpeechInstructions: env.OPENAI_TTS_INSTRUCTIONS ||
      "Speak as a fictional adult resident: natural, concise, expressive, and never childlike.",
    openaiSafetyIdentifier: env.OPENAI_SAFETY_IDENTIFIER || ""
  });
}
