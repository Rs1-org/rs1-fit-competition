import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { of } from 'rxjs';
import { refreshInterceptor } from './refresh.interceptors';
import { AuthService } from '../services/auth.service';
import { TokenService } from '../services/token.service';
import { environment } from '../../../environments/environment';

describe('refreshInterceptor', () => {
  let http: HttpClient;
  let requests: HttpTestingController;
  const refreshToken = vi.fn();
  const accessToken = vi.fn();
  const refresh = vi.fn();

  beforeEach(() => {
    refresh.mockReset().mockReturnValue(of({}));
    accessToken.mockReset().mockReturnValue('renewed-access');
    refreshToken.mockReset().mockReturnValue('existing-refresh');
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([refreshInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: { refreshToken: refresh } },
        {
          provide: TokenService,
          useValue: { getRefreshToken: refreshToken, getAccessToken: accessToken },
        },
      ],
    });
    http = TestBed.inject(HttpClient);
    requests = TestBed.inject(HttpTestingController);
  });
  afterEach(() => requests.verify());

  it('replays a protected request with the renewed bearer token', () => {
    let result: unknown;
    const url = `${environment.apiUrl}/api/settings`;
    http.get(url, { headers: { Authorization: 'Bearer expired' } }).subscribe((v) => (result = v));
    requests.expectOne(url).flush({}, { status: 401, statusText: 'Unauthorized' });
    const retried = requests.expectOne(url);
    expect(retried.request.headers.get('Authorization')).toBe('Bearer renewed-access');
    retried.flush({ ok: true });
    expect(result).toEqual({ ok: true });
    expect(refresh).toHaveBeenCalledOnce();
  });

  it('does not intercept invalid auth credentials or recursively refresh its own endpoint', () => {
    for (const path of ['login', 'refresh-token']) {
      const url = `${environment.apiUrl}/api/auth/${path}`;
      let status: number | undefined;
      http.post(url, {}).subscribe({ error: (e) => (status = e.status) });
      requests.expectOne(url).flush({}, { status: 401, statusText: 'Unauthorized' });
      expect(status).toBe(401);
    }
    expect(refresh).not.toHaveBeenCalled();
  });

  it('does not refresh session for third-party 401 responses', () => {
    let status: number | undefined;
    const external = 'https://example.com.evil.com/api/settings';
    http.get(external).subscribe({ error: (e) => (status = e.status) });
    requests.expectOne(external).flush({}, { status: 401, statusText: 'Unauthorized' });
    expect(status).toBe(401);
    expect(refresh).not.toHaveBeenCalled();
  });

  it('returns the original error when the browser has no refresh token', () => {
    refreshToken.mockReturnValue(null);
    let status: number | undefined;
    const url = `${environment.apiUrl}/api/settings`;
    http.get(url).subscribe({ error: (e) => (status = e.status) });
    requests.expectOne(url).flush({}, { status: 401, statusText: 'Unauthorized' });
    expect(status).toBe(401);
    expect(refresh).not.toHaveBeenCalled();
  });
});
