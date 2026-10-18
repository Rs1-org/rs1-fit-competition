export function apiOrigin(value) {
  if (!value) throw new Error('Set PUBLIC_API_URL to the deployed HTTPS API origin.');
  const url = new URL(value);
  if (
    url.protocol !== 'https:' ||
    url.username ||
    url.password ||
    url.search ||
    url.hash ||
    url.pathname !== '/' ||
    url.hostname === 'localhost' ||
    url.hostname.endsWith('.localhost') ||
    url.hostname === '127.0.0.1' ||
    url.hostname === '[::1]'
  ) {
    throw new Error(
      'PUBLIC_API_URL must be a public HTTPS origin without credentials, path, query or fragment.',
    );
  }
  return url.origin;
}
