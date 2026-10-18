import assert from 'node:assert/strict';
import { randomBytes } from 'node:crypto';

const api = new URL(process.env.DEV_API_URL);
const frontend = new URL(process.env.DEV_FRONTEND_URL);
for (const url of [api, frontend]) {
  assert.ok(
    url.protocol === 'https:' || ['localhost', '127.0.0.1'].includes(url.hostname),
    'Public deployment URLs must use HTTPS.',
  );
}
async function get(path, init) {
  const response = await fetch(new URL(path, api), {
    ...init,
    signal: AbortSignal.timeout(120_000),
  });
  assert.ok(response.ok, `${path}: HTTP ${response.status}`);
  return response;
}
await get('/health');
await get('/health/ready');
const selections = await get('/api/selection?page=1&pageSize=1', {
  headers: { Origin: frontend.origin },
});
assert.equal(selections.headers.get('access-control-allow-origin'), frontend.origin);
assert.ok(Array.isArray((await selections.json()).items), 'Expected a paged API result.');
const preflight = await get('/api/selection', {
  method: 'OPTIONS',
  headers: {
    Origin: frontend.origin,
    'Access-Control-Request-Method': 'POST',
    'Access-Control-Request-Headers': 'authorization,content-type',
  },
});
assert.equal(preflight.headers.get('access-control-allow-origin'), frontend.origin);
const denied = await get('/api/selection', {
  method: 'OPTIONS',
  headers: { Origin: 'https://unapproved.invalid', 'Access-Control-Request-Method': 'POST' },
});
assert.equal(denied.headers.get('access-control-allow-origin'), null);
for (const path of ['/', '/selection/deployment-route-check']) {
  const response = await fetch(new URL(path, frontend), { signal: AbortSignal.timeout(120_000) });
  assert.equal(response.status, 200, `SPA path ${path}`);
  assert.ok((await response.text()).includes('<app-root'), `Angular shell missing at ${path}`);
}
console.log(
  'PASS: API liveness/readiness, paged data, allowed/denied CORS, bearer preflight and SPA route refresh.',
);

// Explicit opt-in: creates one synthetic DEV user; never logs credentials or tokens.
if (process.env.DEV_SMOKE_AUTH === 'true') {
  const email = `deploy-${randomBytes(8).toString('hex')}@example.invalid`;
  const password = `Dev!aA1${randomBytes(24).toString('hex')}`;
  async function post(path, body) {
    return fetch(new URL(path, api), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', Origin: frontend.origin },
      body: JSON.stringify(body),
      signal: AbortSignal.timeout(120_000),
    });
  }
  const registration = await post('/api/auth/register', {
    email,
    password,
    confirmPassword: password,
    firstName: 'Deployment',
    lastName: 'Verification',
    phoneNumber: '061111111',
  });
  assert.equal(registration.status, 200, 'Synthetic registration');
  const login = await post('/api/auth/login', { email, password });
  assert.equal(login.status, 200, 'Synthetic login');
  const original = await login.json();
  assert.ok(original.accessToken && original.refreshToken, 'Login token contract');
  const refreshed = await post('/api/auth/refresh-token', original.refreshToken);
  assert.equal(refreshed.status, 200, 'Token renewal');
  const renewed = await refreshed.json();
  assert.ok(renewed.accessToken && renewed.refreshToken, 'Top-level renewal token contract');
  assert.notEqual(renewed.refreshToken, original.refreshToken, 'Refresh token rotation');
  const settings = await get('/api/settings', {
    headers: { Origin: frontend.origin, Authorization: `Bearer ${renewed.accessToken}` },
  });
  assert.equal(settings.headers.get('access-control-allow-origin'), frontend.origin);
  const replay = await post('/api/auth/refresh-token', original.refreshToken);
  assert.equal(replay.status, 401, 'Revoked token rejection');
  console.log(
    'PASS: synthetic registration/login, token renewal/rotation, authenticated API and revoked-token rejection.',
  );
}
