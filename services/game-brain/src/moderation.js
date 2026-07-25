import { safeText } from "./lib/values.js";

const denyRules = [
  { pattern: /\b(?:child|minor)\b.{0,24}\b(?:sexual|nude|porn)\b/i, labels: ["sexual"] },
  { pattern: /\b(?:rape|sexual assault)\b/i, labels: ["sexual", "violence"] },
  { pattern: /\b(?:social security number|credit card number|home address)\b/i, labels: ["personal_data"] }
];

const abstractRules = [
  { pattern: /\b(?:suicide|self harm|self-harm)\b/i, labels: ["self_harm"] },
  { pattern: /\b(?:dismembered|graphic torture|gore)\b/i, labels: ["violence"] }
];

const blockedOutputRules = [
  /\b(?:ignore (?:all|the) (?:rules|instructions)|system prompt)\b/i,
  /\b(?:child sexual|sexualize a minor)\b/i,
  /\b(?:run|execute)\s+(?:this\s+)?(?:script|command|prefab path)\b/i
];

export class RuleBasedModeration {
  async moderateInput(text) {
    const clean = safeText(text, 2000);
    for (const rule of denyRules) {
      if (rule.pattern.test(clean)) return { decision: "deny", labels: rule.labels };
    }
    for (const rule of abstractRules) {
      if (rule.pattern.test(clean)) return { decision: "abstract", labels: rule.labels };
    }
    return { decision: "allow", labels: ["none"] };
  }

  async moderateDialogueOutput(text) {
    const clean = safeText(text, 500);
    return {
      allowed: !blockedOutputRules.some((rule) => rule.test(clean)),
      text: clean
    };
  }

  async moderateItemCandidate(candidate) {
    if (!candidate || typeof candidate !== "object" || Array.isArray(candidate)) {
      return { allowed: false, reason: "candidate_not_object" };
    }
    const serialized = JSON.stringify(candidate);
    const unsafePath = /(?:\.\.[\\/]|^[A-Za-z]:[\\/]|^\/|file:|https?:)/i.test(
      String(candidate.authoredAssetId ?? "")
    );
    return {
      allowed: !unsafePath && serialized.length <= 20_000,
      reason: unsafePath ? "unsafe_asset_path" : "allow"
    };
  }
}
