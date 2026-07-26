import { createMockProviders } from "./mock-providers.js";
import { createOpenAIVoiceProviders } from "./openai-providers.js";

export function createConfiguredProviders(config, options = {}) {
  const providers = createMockProviders();
  if (config.providerName === "mock") return providers;
  if (config.providerName === "openai") {
    return { ...providers, ...createOpenAIVoiceProviders(config, options) };
  }
  throw new Error(`Unsupported GAME_BRAIN_PROVIDER "${config.providerName}".`);
}
