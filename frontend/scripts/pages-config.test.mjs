import { test } from 'node:test';
import assert from 'node:assert/strict';
import { apiOrigin } from './pages-config.mjs';

test('Pages build accepts an HTTPS API origin and normalizes its trailing slash', () => {
  assert.equal(
    apiOrigin('https://competition-dev-api.onrender.com/'),
    'https://competition-dev-api.onrender.com',
  );
});

test('Pages build rejects missing, local, insecure and non-origin API configuration', () => {
  for (const value of [
    undefined,
    '',
    'invalid',
    'http://example.com',
    'https://localhost',
    'https://127.0.0.1',
    'https://[::1]',
    'https://example.com/api',
    'https://example.com?key=x',
    'https://user:pass@example.com',
    'https://example.com/#fragment',
  ]) {
    assert.throws(() => apiOrigin(value), `Expected rejection: ${value}`);
  }
});
