const integer = (value, fallback) => {
  const parsed = Number.parseInt(value ?? "", 10);
  return Number.isFinite(parsed) ? parsed : fallback;
};

export function loadConfig(env = process.env) {
  return Object.freeze({
    host: env.GAME_BRAIN_HOST || "127.0.0.1",
    port: integer(env.GAME_BRAIN_PORT, 8787),
    providerTimeoutMs: integer(env.GAME_BRAIN_PROVIDER_TIMEOUT_MS, 1500),
    cacheTtlMs: integer(env.GAME_BRAIN_CACHE_TTL_MS, 300_000),
    cacheMaxEntries: integer(env.GAME_BRAIN_CACHE_MAX_ENTRIES, 500),
    rateLimitWindowMs: integer(env.GAME_BRAIN_RATE_LIMIT_WINDOW_MS, 60_000),
    rateLimitMaxRequests: integer(env.GAME_BRAIN_RATE_LIMIT_MAX_REQUESTS, 120),
    maxBodyBytes: integer(env.GAME_BRAIN_MAX_BODY_BYTES, 2_100_000),
    providerName: env.GAME_BRAIN_PROVIDER || "mock"
  });
}
