import { createHash } from "node:crypto";

export const CONTRACT_VERSION = 1;
export const RESOLVER_VERSION = "v1.0.0";

export function clone(value) {
  return structuredClone(value);
}

export function clamp(value, minimum, maximum, fallback) {
  const numeric = Number(value);
  if (!Number.isFinite(numeric)) return fallback;
  return Math.min(maximum, Math.max(minimum, numeric));
}

export function normalizePrompt(value) {
  return String(value)
    .normalize("NFKC")
    .toLocaleLowerCase("en-US")
    .replace(/[’']/g, "")
    .replace(/[^a-z0-9]+/g, " ")
    .trim()
    .replace(/^(?:a|an|the)\s+/, "")
    .replace(/\s+/g, " ");
}

export function safeText(value, maximum, fallback = "") {
  const text = typeof value === "string"
    ? value.normalize("NFKC").replace(/[\u0000-\u001F\u007F]/g, " ").replace(/\s+/g, " ").trim()
    : "";
  return (text || fallback).slice(0, maximum);
}

export function slug(value, fallback = "item", maximum = 48) {
  const normalized = normalizePrompt(value).replace(/\s+/g, "_");
  const safe = normalized.replace(/[^a-z0-9_]/g, "").replace(/^_+|_+$/g, "");
  return (safe || fallback).slice(0, maximum).replace(/_+$/g, "") || fallback;
}

export function stableHex(value) {
  return `#${createHash("sha256").update(String(value)).digest("hex").slice(0, 6).toUpperCase()}`;
}

export function uniqueAllowed(values, allowed, maximum) {
  if (!Array.isArray(values)) return [];
  return [...new Set(values.filter((value) => allowed.has(value)))].slice(0, maximum);
}
