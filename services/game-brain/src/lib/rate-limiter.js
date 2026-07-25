export class MemoryRateLimiter {
  constructor({ windowMs = 60_000, maxRequests = 120, clock = () => Date.now() } = {}) {
    this.windowMs = windowMs;
    this.maxRequests = maxRequests;
    this.clock = clock;
    this.clients = new Map();
  }

  check(clientId) {
    const now = this.clock();
    const current = this.clients.get(clientId);
    const bucket = !current || current.resetAt <= now
      ? { count: 0, resetAt: now + this.windowMs }
      : current;
    bucket.count += 1;
    this.clients.set(clientId, bucket);
    return {
      allowed: bucket.count <= this.maxRequests,
      remaining: Math.max(0, this.maxRequests - bucket.count),
      retryAfterMs: Math.max(0, bucket.resetAt - now)
    };
  }
}
