import { TtlCache } from "./lib/cache.js";
import { loadConfig } from "./config.js";
import { GameBrain } from "./game-brain.js";
import { createGameBrainServer } from "./server.js";

const config = loadConfig();

if (config.providerName !== "mock") {
  throw new Error(
    `Provider "${config.providerName}" is not configured. This foundation ships with the key-free "mock" provider only.`
  );
}

const brain = new GameBrain({
  providerTimeoutMs: config.providerTimeoutMs,
  cache: new TtlCache({
    ttlMs: config.cacheTtlMs,
    maxEntries: config.cacheMaxEntries
  })
});

const server = createGameBrainServer({ brain, config });

server.listen(config.port, config.host, () => {
  const address = server.address();
  const host = typeof address === "object" && address ? address.address : config.host;
  const port = typeof address === "object" && address ? address.port : config.port;
  process.stdout.write(`Human Glass Watcher game-brain listening on http://${host}:${port}\n`);
});

function shutdown(signal) {
  process.stdout.write(`Received ${signal}; shutting down.\n`);
  server.close((error) => {
    process.exitCode = error ? 1 : 0;
  });
}

process.once("SIGINT", () => shutdown("SIGINT"));
process.once("SIGTERM", () => shutdown("SIGTERM"));
