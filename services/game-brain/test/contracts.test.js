import assert from "node:assert/strict";
import { readdir, readFile } from "node:fs/promises";
import { test } from "node:test";
import { fileURLToPath } from "node:url";
import { listKnownItems } from "../src/catalog.js";
import { ContractRegistry, contracts } from "../src/lib/contract-registry.js";

const contractDirectory = fileURLToPath(
  new URL("../../../contracts/v1/", import.meta.url)
);
const exampleDirectory = `${contractDirectory}/examples`;

const exampleSchemas = new Map([
  ["apple.item-spec.json", "item-spec.schema.json"],
  ["dog-feces.item-spec.json", "item-spec.schema.json"],
  ["unknown-item.item-spec.json", "item-spec.schema.json"],
  ["item-resolution.request.json", "item-resolution-request.schema.json"],
  ["dialogue.request.json", "dialogue-request.schema.json"],
  ["dialogue.turn.json", "dialogue-turn.schema.json"],
  ["memory-summary.request.json", "memory-summary-request.schema.json"],
  ["memory-summary.json", "memory-summary.schema.json"],
  ["error.json", "error.schema.json"],
  ["voice-transcription.request.json", "voice-transcription-request.schema.json"],
  ["voice-transcription.result.json", "voice-transcription-result.schema.json"],
  ["voice-synthesis.request.json", "voice-synthesis-request.schema.json"],
  ["voice-synthesis.result.json", "voice-synthesis-result.schema.json"]
]);

test("every v1 schema loads and every published example validates", async () => {
  const registry = new ContractRegistry(contractDirectory);
  const schemaFiles = (await readdir(contractDirectory)).filter((name) => name.endsWith(".schema.json"));
  assert.ok(schemaFiles.length >= 10);

  for (const [exampleName, schemaName] of exampleSchemas) {
    const value = JSON.parse(await readFile(`${exampleDirectory}/${exampleName}`, "utf8"));
    const result = registry.validate(schemaName, value);
    assert.equal(result.valid, true, `${exampleName}: ${result.errors.join("; ")}`);
  }
});

test("all twelve authored catalog entries satisfy ItemSpec v1", () => {
  const items = listKnownItems();
  assert.equal(items.length, 12);
  for (const item of items) contracts.assert("item-spec.schema.json", item);
});

test("contract boundary rejects unknown request fields", () => {
  const result = contracts.validate("item-resolution-request.schema.json", {
    contractVersion: 1,
    prompt: "apple",
    prefabPath: "../../Assets/evil.prefab"
  });
  assert.equal(result.valid, false);
  assert.ok(result.errors.some((error) => error.includes("additional property")));
});
