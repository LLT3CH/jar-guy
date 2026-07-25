import assert from "node:assert/strict";
import { once } from "node:events";
import { test } from "node:test";
import { loadConfig } from "../src/config.js";
import { GameBrain } from "../src/game-brain.js";
import { MemoryRateLimiter } from "../src/lib/rate-limiter.js";
import { createGameBrainServer } from "../src/server.js";

async function withServer(run) {
  const config = {
    ...loadConfig({}),
    host: "127.0.0.1",
    port: 0,
    rateLimitMaxRequests: 100
  };
  const server = createGameBrainServer({ brain: new GameBrain(), config });
  server.listen(0, "127.0.0.1");
  await once(server, "listening");
  const address = server.address();
  try {
    await run(`http://127.0.0.1:${address.port}`);
  } finally {
    server.closeAllConnections();
    await new Promise((resolve) => server.close(resolve));
  }
}

test("fresh key-free HTTP service resolves items and exposes only provider names", async () => {
  await withServer(async (baseUrl) => {
    const healthResponse = await fetch(`${baseUrl}/health`);
    assert.equal(healthResponse.status, 200);
    const health = await healthResponse.json();
    assert.equal(health.status, "ok");
    assert.deepEqual(new Set(Object.values(health.providers)), new Set(["mock"]));
    assert.equal(JSON.stringify(health).includes("key"), false);

    const itemResponse = await fetch(`${baseUrl}/v1/items/resolve`, {
      method: "POST",
      headers: { "content-type": "application/json", "x-client-id": "test_client" },
      body: JSON.stringify({ contractVersion: 1, prompt: "dog shit" })
    });
    assert.equal(itemResponse.status, 200);
    assert.equal(itemResponse.headers.get("x-cache"), "LOCAL");
    const item = await itemResponse.json();
    assert.equal(item.canonicalId, "dog_feces");
  });
});

test("HTTP contract errors are structured and fail closed", async () => {
  await withServer(async (baseUrl) => {
    const response = await fetch(`${baseUrl}/v1/items/resolve`, {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ contractVersion: 1, prompt: "apple", prefabPath: "../../evil" })
    });
    assert.equal(response.status, 400);
    const body = await response.json();
    assert.equal(body.contractVersion, 1);
    assert.equal(body.error.code, "invalid_request");
    assert.equal(body.error.retryable, false);
    assert.ok(body.error.details.some((detail) => detail.includes("additional property")));
  });
});

test("in-memory rate limiter provides an injectable enforcement seam", () => {
  let now = 1000;
  const limiter = new MemoryRateLimiter({
    windowMs: 100,
    maxRequests: 2,
    clock: () => now
  });
  assert.equal(limiter.check("client").allowed, true);
  assert.equal(limiter.check("client").allowed, true);
  assert.equal(limiter.check("client").allowed, false);
  now += 101;
  assert.equal(limiter.check("client").allowed, true);
});
