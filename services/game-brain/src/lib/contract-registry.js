import { readdirSync, readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";

const CONTRACT_DIRECTORY = fileURLToPath(
  new URL("../../../../contracts/v1/", import.meta.url)
);

export class ContractValidationError extends Error {
  constructor(schemaName, errors) {
    super(`Contract ${schemaName} validation failed: ${errors.join("; ")}`);
    this.name = "ContractValidationError";
    this.schemaName = schemaName;
    this.errors = errors;
  }
}

function deepEqual(left, right) {
  return JSON.stringify(left) === JSON.stringify(right);
}

function valueType(value) {
  if (value === null) return "null";
  if (Array.isArray(value)) return "array";
  if (Number.isInteger(value)) return "integer";
  return typeof value;
}

function pointer(root, fragment) {
  if (!fragment || fragment === "#") return root;
  return fragment
    .replace(/^#\//, "")
    .split("/")
    .map((part) => part.replace(/~1/g, "/").replace(/~0/g, "~"))
    .reduce((value, part) => value?.[part], root);
}

export class ContractRegistry {
  constructor(directory = CONTRACT_DIRECTORY) {
    this.schemas = new Map();
    this.byId = new Map();

    for (const filename of readdirSync(directory).filter((name) => name.endsWith(".schema.json"))) {
      const schema = JSON.parse(readFileSync(`${directory}/${filename}`, "utf8"));
      this.schemas.set(filename, schema);
      if (schema.$id) this.byId.set(schema.$id, schema);
    }
  }

  validate(schemaName, value) {
    const root = this.schemas.get(schemaName);
    if (!root) throw new Error(`Unknown contract schema: ${schemaName}`);
    const errors = [];
    this.#validateNode(root, value, "$", root, errors);
    return { valid: errors.length === 0, errors };
  }

  assert(schemaName, value) {
    const result = this.validate(schemaName, value);
    if (!result.valid) throw new ContractValidationError(schemaName, result.errors);
    return value;
  }

  #resolve(reference, currentRoot) {
    if (reference.startsWith("#")) {
      return { schema: pointer(currentRoot, reference), root: currentRoot };
    }
    const [id, fragment] = reference.split("#");
    const externalRoot = this.byId.get(id);
    return {
      schema: pointer(externalRoot, fragment ? `#/${fragment.replace(/^\//, "")}` : "#"),
      root: externalRoot
    };
  }

  #validateNode(schema, value, path, root, errors) {
    if (!schema) {
      errors.push(`${path}: unresolved schema reference`);
      return;
    }

    if (schema.$ref) {
      const resolved = this.#resolve(schema.$ref, root);
      this.#validateNode(resolved.schema, value, path, resolved.root, errors);
      return;
    }

    if (schema.oneOf) {
      const matches = schema.oneOf.filter((branch) => {
        const branchErrors = [];
        this.#validateNode(branch, value, path, root, branchErrors);
        return branchErrors.length === 0;
      });
      if (matches.length !== 1) errors.push(`${path}: must match exactly one allowed shape`);
      return;
    }

    if (Object.hasOwn(schema, "const") && !deepEqual(schema.const, value)) {
      errors.push(`${path}: must equal ${JSON.stringify(schema.const)}`);
      return;
    }

    if (schema.enum && !schema.enum.some((candidate) => deepEqual(candidate, value))) {
      errors.push(`${path}: must be one of ${schema.enum.join(", ")}`);
      return;
    }

    if (schema.type) {
      const actual = valueType(value);
      const accepted = Array.isArray(schema.type) ? schema.type : [schema.type];
      const matches = accepted.includes(actual) || (actual === "integer" && accepted.includes("number"));
      if (!matches) {
        errors.push(`${path}: expected ${accepted.join("|")}, got ${actual}`);
        return;
      }
    }

    if (typeof value === "string") {
      if (schema.minLength !== undefined && value.length < schema.minLength) {
        errors.push(`${path}: must have length >= ${schema.minLength}`);
      }
      if (schema.maxLength !== undefined && value.length > schema.maxLength) {
        errors.push(`${path}: must have length <= ${schema.maxLength}`);
      }
      if (schema.pattern && !(new RegExp(schema.pattern).test(value))) {
        errors.push(`${path}: must match ${schema.pattern}`);
      }
    }

    if (typeof value === "number") {
      if (!Number.isFinite(value)) errors.push(`${path}: must be finite`);
      if (schema.minimum !== undefined && value < schema.minimum) {
        errors.push(`${path}: must be >= ${schema.minimum}`);
      }
      if (schema.maximum !== undefined && value > schema.maximum) {
        errors.push(`${path}: must be <= ${schema.maximum}`);
      }
    }

    if (Array.isArray(value)) {
      if (schema.minItems !== undefined && value.length < schema.minItems) {
        errors.push(`${path}: must contain at least ${schema.minItems} items`);
      }
      if (schema.maxItems !== undefined && value.length > schema.maxItems) {
        errors.push(`${path}: must contain at most ${schema.maxItems} items`);
      }
      if (schema.uniqueItems) {
        const unique = new Set(value.map((entry) => JSON.stringify(entry)));
        if (unique.size !== value.length) errors.push(`${path}: items must be unique`);
      }
      if (schema.items) {
        value.forEach((entry, index) => this.#validateNode(
          schema.items,
          entry,
          `${path}[${index}]`,
          root,
          errors
        ));
      }
    }

    if (value && typeof value === "object" && !Array.isArray(value)) {
      for (const required of schema.required || []) {
        if (!Object.hasOwn(value, required)) errors.push(`${path}.${required}: is required`);
      }
      for (const [key, entry] of Object.entries(value)) {
        if (schema.properties?.[key]) {
          this.#validateNode(schema.properties[key], entry, `${path}.${key}`, root, errors);
        } else if (schema.additionalProperties === false) {
          errors.push(`${path}.${key}: additional property is not allowed`);
        }
      }
    }
  }
}

export const contracts = new ContractRegistry();
