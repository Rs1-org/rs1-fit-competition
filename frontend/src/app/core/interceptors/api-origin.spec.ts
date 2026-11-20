import { environment } from '../../../environments/environment';
import { isRequestToOwnApi } from './api-origin';

describe('isRequestToOwnApi', () => {
  const configured = environment.apiUrl || 'https://localhost:7208';

  it('matches absolute requests on the configured API origin', () => {
    expect(isRequestToOwnApi(`${configured}/api/settings`, configured)).toBe(true);
  });

  it('rejects lookalike hosts that only start with the API hostname', () => {
    const origin = new URL(configured).origin;
    expect(isRequestToOwnApi(`${origin}.evil.com/api/settings`, configured)).toBe(false);
  });

  it('rejects unrelated third-party origins', () => {
    expect(isRequestToOwnApi('https://other.example.com/data', configured)).toBe(false);
  });
});