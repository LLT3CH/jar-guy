export class ProviderTimeoutError extends Error {
  constructor(timeoutMs) {
    super(`Provider timed out after ${timeoutMs}ms`);
    this.name = "ProviderTimeoutError";
    this.timeoutMs = timeoutMs;
  }
}

export async function withTimeout(operation, timeoutMs) {
  const controller = new AbortController();
  let timer;
  try {
    return await Promise.race([
      Promise.resolve().then(() => operation(controller.signal)),
      new Promise((_, reject) => {
        timer = setTimeout(() => {
          controller.abort();
          reject(new ProviderTimeoutError(timeoutMs));
        }, timeoutMs);
      })
    ]);
  } finally {
    clearTimeout(timer);
  }
}
