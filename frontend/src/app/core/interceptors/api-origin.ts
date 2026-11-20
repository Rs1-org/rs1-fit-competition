export function isRequestToOwnApi(requestUrl: string, configuredApiUrl: string): boolean {
  const trimmed = (configuredApiUrl ?? '').trim();
  let apiOrigin: string;
  if (!trimmed) {
    if (typeof window === 'undefined' || !window.location?.origin) {
      return false;
    }
    apiOrigin = window.location.origin;
  } else {
    try {
      apiOrigin = new URL(trimmed).origin;
    } catch {
      return false;
    }
  }

  try {
    const base = typeof window !== 'undefined' ? window.location.origin : undefined;
    const resolved = base ? new URL(requestUrl, base) : new URL(requestUrl);
    return resolved.origin === apiOrigin;
  } catch {
    return false;
  }
}