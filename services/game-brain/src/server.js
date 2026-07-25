import { randomUUID } from "node:crypto";
import { createServer as createNodeServer } from "node:http";
import { ContractValidationError, contracts } from "./lib/contract-registry.js";
import { MemoryRateLimiter } from "./lib/rate-limiter.js";
import { safeText } from "./lib/values.js";

class HttpError extends Error {
  constructor(status, code, message, retryable = false, details = []) {
    super(message);
    this.status = status;
    this.code = code;
    this.retryable = retryable;
    this.details = details;
  }
}

function sendJson(response, status, value, headers = {}) {
  const body = JSON.stringify(value);
  response.writeHead(status, {
    "content-type": "application/json; charset=utf-8",
    "content-length": Buffer.byteLength(body),
    "cache-control": "no-store",
    ...headers
  });
  response.end(body);
}

async function readJson(request, maximumBytes) {
  const contentType = request.headers["content-type"] || "";
  if (!contentType.toLocaleLowerCase("en-US").startsWith("application/json")) {
    throw new HttpError(415, "unsupported_media_type", "Content-Type must be application/json.");
  }

  const chunks = [];
  let size = 0;
  for await (const chunk of request) {
    size += chunk.length;
    if (size > maximumBytes) {
      throw new HttpError(413, "body_too_large", "The request body exceeds the configured limit.");
    }
    chunks.push(chunk);
  }

  try {
    return JSON.parse(Buffer.concat(chunks).toString("utf8"));
  } catch {
    throw new HttpError(400, "invalid_json", "The request body must contain valid JSON.");
  }
}

function errorBody(error, requestId) {
  let normalized = error;
  if (error instanceof ContractValidationError) {
    normalized = new HttpError(
      400,
      "invalid_request",
      "The request did not match contract v1.",
      false,
      error.errors
    );
  } else if (!(error instanceof HttpError)) {
    normalized = new HttpError(500, "internal_error", "The service could not complete the request.", true);
  }
  const value = {
    contractVersion: 1,
    error: {
      code: normalized.code,
      message: normalized.message,
      retryable: normalized.retryable,
      requestId
    }
  };
  if (normalized.details.length) {
    value.error.details = normalized.details.map((detail) => safeText(detail, 240)).slice(0, 20);
  }
  contracts.assert("error.schema.json", value);
  return { status: normalized.status, value };
}

function clientId(request) {
  const supplied = safeText(request.headers["x-client-id"], 64);
  return supplied || request.socket.remoteAddress || "anonymous";
}

export function createGameBrainServer({
  brain,
  config,
  rateLimiter = new MemoryRateLimiter({
    windowMs: config.rateLimitWindowMs,
    maxRequests: config.rateLimitMaxRequests
  })
}) {
  return createNodeServer(async (request, response) => {
    const requestId = randomUUID();
    response.setHeader("x-request-id", requestId);

    try {
      if (request.method === "GET" && request.url === "/health") {
        sendJson(response, 200, {
          status: "ok",
          contractVersion: 1,
          providers: {
            item: brain.providers.item?.name || "unknown",
            dialogue: brain.providers.dialogue?.name || "unknown",
            memory: brain.providers.memory?.name || "unknown",
            transcription: brain.providers.transcription?.name || "unknown",
            speech: brain.providers.speech?.name || "unknown"
          }
        });
        return;
      }

      const rate = rateLimiter.check(clientId(request));
      response.setHeader("x-ratelimit-remaining", String(rate.remaining));
      if (!rate.allowed) {
        response.setHeader("retry-after", String(Math.max(1, Math.ceil(rate.retryAfterMs / 1000))));
        throw new HttpError(429, "rate_limited", "Too many requests. Try again later.", true);
      }

      const routes = {
        "/v1/items/resolve": async (body) => {
          const resolved = await brain.resolveItemWithMeta(body);
          return { value: resolved.value, headers: { "x-cache": resolved.cacheStatus } };
        },
        "/v1/dialogue/turn": async (body) => ({ value: await brain.createDialogueTurn(body) }),
        "/v1/memory/summarize": async (body) => ({ value: await brain.summarizeMemory(body) }),
        "/v1/voice/transcribe": async (body) => ({ value: await brain.transcribe(body) }),
        "/v1/voice/synthesize": async (body) => ({ value: await brain.synthesize(body) })
      };

      const route = routes[request.url];
      if (request.method !== "POST" || !route) {
        throw new HttpError(404, "not_found", "No route matches this request.");
      }

      const body = await readJson(request, config.maxBodyBytes);
      const result = await route(body);
      sendJson(response, 200, result.value, result.headers);
    } catch (error) {
      const normalized = errorBody(error, requestId);
      sendJson(response, normalized.status, normalized.value);
    }
  });
}
