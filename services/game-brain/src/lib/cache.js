import { clone } from "./values.js";

export class TtlCache {
  constructor({ ttlMs = 300_000, maxEntries = 500, clock = () => Date.now() } = {}) {
    this.ttlMs = ttlMs;
    this.maxEntries = maxEntries;
    this.clock = clock;
    this.entries = new Map();
  }

  get(key) {
    const entry = this.entries.get(key);
    if (!entry) return undefined;
    if (entry.expiresAt <= this.clock()) {
      this.entries.delete(key);
      return undefined;
    }
    this.entries.delete(key);
    this.entries.set(key, entry);
    return clone(entry.value);
  }

  set(key, value) {
    this.entries.delete(key);
    this.entries.set(key, { value: clone(value), expiresAt: this.clock() + this.ttlMs });
    while (this.entries.size > this.maxEntries) {
      this.entries.delete(this.entries.keys().next().value);
    }
  }
}
