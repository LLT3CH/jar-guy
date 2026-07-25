import { clone, CONTRACT_VERSION, normalizePrompt, RESOLVER_VERSION } from "./lib/values.js";

function item({
  canonicalId,
  displayName,
  asset,
  archetype = "authored",
  colorHex,
  scale,
  massKg,
  bounciness = 0.1,
  softness = 0.2,
  fragility = 0.2,
  sharpness = 0,
  dirtiness = 0,
  consumable = null,
  capabilities,
  tags,
  labels = ["none"]
}) {
  return Object.freeze({
    contractVersion: CONTRACT_VERSION,
    resolverVersion: RESOLVER_VERSION,
    canonicalId,
    displayName,
    sourcePrompt: displayName,
    authoredAssetId: asset,
    visual: { archetype, colorHex, scale },
    physical: { massKg, bounciness, softness, fragility, sharpness, dirtiness },
    consumable,
    capabilities,
    tags,
    content: { decision: "allow", labels }
  });
}

const items = [
  {
    spec: item({
      canonicalId: "apple",
      displayName: "Apple",
      asset: "items/food/apple",
      colorHex: "#C93636",
      scale: [0.22, 0.22, 0.22],
      massKg: 0.18,
      bounciness: 0.15,
      softness: 0.25,
      fragility: 0.35,
      consumable: { nutrition: 0.35, hydration: 0.12, toxicity: 0, taste: 0.65 },
      capabilities: ["grabbable", "throwable", "edible"],
      tags: ["food", "fruit", "sweet", "vegetarian"]
    }),
    aliases: ["apple", "red apple", "crisp red apple"]
  },
  {
    spec: item({
      canonicalId: "chocolate_cake",
      displayName: "Chocolate Cake",
      asset: "items/food/chocolate_cake",
      colorHex: "#5A3428",
      scale: [0.38, 0.22, 0.38],
      massKg: 0.8,
      softness: 0.65,
      fragility: 0.7,
      consumable: { nutrition: 0.65, hydration: -0.05, toxicity: 0, taste: 0.9 },
      capabilities: ["grabbable", "edible", "fragile"],
      tags: ["food", "dessert", "sweet"]
    }),
    aliases: ["chocolate cake", "cake", "slice of chocolate cake"]
  },
  {
    spec: item({
      canonicalId: "water_bottle",
      displayName: "Water Bottle",
      asset: "items/food/water_bottle",
      archetype: "bottle",
      colorHex: "#80C8E8",
      scale: [0.16, 0.42, 0.16],
      massKg: 0.55,
      softness: 0.12,
      fragility: 0.15,
      consumable: { nutrition: 0, hydration: 0.8, toxicity: 0, taste: 0.1 },
      capabilities: ["grabbable", "throwable", "drinkable", "container"],
      tags: ["water", "liquid", "drink"]
    }),
    aliases: ["water bottle", "bottle of water", "water"]
  },
  {
    spec: item({
      canonicalId: "dog_feces",
      displayName: "Dog Feces",
      asset: "items/gross/dog_feces",
      colorHex: "#5A351F",
      scale: [0.25, 0.16, 0.25],
      massKg: 0.12,
      bounciness: 0,
      softness: 0.8,
      fragility: 0.2,
      dirtiness: 1,
      consumable: { nutrition: -1, hydration: -0.3, toxicity: 0.9, taste: -1 },
      capabilities: ["dirty", "toxic"],
      tags: ["biological_waste", "unsanitary", "foul_smell"],
      labels: ["gross"]
    }),
    aliases: ["dog feces", "dog faeces", "dog poop", "dog poo", "dog shit", "canine feces"]
  },
  {
    spec: item({
      canonicalId: "rubber_ball",
      displayName: "Rubber Ball",
      asset: "items/play/rubber_ball",
      colorHex: "#E65A4F",
      scale: [0.24, 0.24, 0.24],
      massKg: 0.15,
      bounciness: 0.9,
      softness: 0.35,
      capabilities: ["grabbable", "throwable", "bouncy", "entertainment"],
      tags: ["toy", "ball", "rubber"]
    }),
    aliases: ["rubber ball", "ball", "bouncy ball"]
  },
  {
    spec: item({
      canonicalId: "baseball_bat",
      displayName: "Baseball Bat",
      asset: "items/tools/baseball_bat",
      archetype: "tool",
      colorHex: "#B88758",
      scale: [0.12, 0.65, 0.12],
      massKg: 0.9,
      softness: 0.05,
      capabilities: ["grabbable", "throwable", "swing_tool", "lever"],
      tags: ["sports", "bat", "rigid_tool"]
    }),
    aliases: ["baseball bat", "bat", "wooden bat"]
  },
  {
    spec: item({
      canonicalId: "hockey_stick",
      displayName: "Hockey Stick",
      asset: "items/tools/hockey_stick",
      archetype: "tool",
      colorHex: "#C89B68",
      scale: [0.18, 0.75, 0.1],
      massKg: 0.75,
      softness: 0.05,
      capabilities: ["grabbable", "throwable", "swing_tool", "lever"],
      tags: ["sports", "hockey", "rigid_tool"]
    }),
    aliases: ["hockey stick", "stick for hockey"]
  },
  {
    spec: item({
      canonicalId: "blanket",
      displayName: "Blanket",
      asset: "items/comfort/blanket",
      archetype: "cloth",
      colorHex: "#6A83B9",
      scale: [0.75, 0.05, 0.55],
      massKg: 0.6,
      bounciness: 0,
      softness: 0.95,
      capabilities: ["grabbable", "wearable", "comfort"],
      tags: ["cloth", "bedding", "warm"]
    }),
    aliases: ["blanket", "warm blanket", "cover"]
  },
  {
    spec: item({
      canonicalId: "rope",
      displayName: "Rope",
      asset: "items/tools/rope",
      archetype: "tool",
      colorHex: "#A88862",
      scale: [0.42, 0.12, 0.42],
      massKg: 0.5,
      softness: 0.55,
      capabilities: ["grabbable", "throwable", "flexible_line"],
      tags: ["rope", "fiber", "binding"]
    }),
    aliases: ["rope", "length of rope", "cord"]
  },
  {
    spec: item({
      canonicalId: "scissors",
      displayName: "Scissors",
      asset: "items/tools/scissors",
      archetype: "tool",
      colorHex: "#AEB7C2",
      scale: [0.22, 0.05, 0.1],
      massKg: 0.12,
      bounciness: 0.05,
      softness: 0,
      sharpness: 0.8,
      capabilities: ["grabbable", "sharp_edge", "lever"],
      tags: ["tool", "cutting", "metal"]
    }),
    aliases: ["scissors", "pair of scissors"]
  },
  {
    spec: item({
      canonicalId: "sponge",
      displayName: "Sponge",
      asset: "items/utility/sponge",
      colorHex: "#E5C94A",
      scale: [0.28, 0.12, 0.18],
      massKg: 0.05,
      bounciness: 0.2,
      softness: 0.85,
      capabilities: ["grabbable", "throwable", "absorbent", "cleaning_agent"],
      tags: ["cleaning", "porous", "absorbent"]
    }),
    aliases: ["sponge", "cleaning sponge"]
  },
  {
    spec: item({
      canonicalId: "flashlight",
      displayName: "Flashlight",
      asset: "items/tools/flashlight",
      archetype: "tool",
      colorHex: "#4C5664",
      scale: [0.14, 0.34, 0.14],
      massKg: 0.3,
      softness: 0.05,
      capabilities: ["grabbable", "throwable", "light_source", "entertainment"],
      tags: ["tool", "light", "signal"]
    }),
    aliases: ["flashlight", "torch", "electric torch"]
  }
];

const byAlias = new Map();
for (const entry of items) {
  for (const alias of entry.aliases) byAlias.set(normalizePrompt(alias), entry.spec);
}

export const AUTHORED_ASSET_IDS = new Set(items.map(({ spec }) => spec.authoredAssetId));

export function resolveKnownItem(prompt) {
  const spec = byAlias.get(normalizePrompt(prompt));
  if (!spec) return null;
  const result = clone(spec);
  result.sourcePrompt = String(prompt).trim();
  return result;
}

export function listKnownItems() {
  return items.map(({ spec }) => clone(spec));
}
