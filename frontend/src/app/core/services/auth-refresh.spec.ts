import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { TokenService } from './token.service';
import { UserStateService } from './user-state.service';
import { environment } from '../../../environments/environment';

describe('AuthService token renewal', () => {
  let service: AuthService;
  let requests: HttpTestingController;
  const setTokens = vi.fn();
  const url = `${environment.apiUrl}/api/auth/refresh-token`;
  beforeEach(() => {
    setTokens.mockReset();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        AuthService,
        { provide: TokenService, useValue: { getRefreshToken: () => 'old-refresh', setTokens } },
        { provide: UserStateService, useValue: { setUser: vi.fn() } },
        { provide: Router, useValue: {} },
      ],
    });
    service = TestBed.inject(AuthService);
    requests = TestBed.inject(HttpTestingController);
  });
  afterEach(() => requests.verify());

  it('shares one token rotation across concurrent callers and clears the request after completion', () => {
    const first = vi.fn();
    const second = vi.fn();
    service.refreshToken().subscribe(first);
    service.refreshToken().subscribe(second);
    const request = requests.expectOne(url);
    expect(request.request.body).toBe('old-refresh');
    request.flush({ accessToken: 'new-access', refreshToken: 'new-refresh' });
    expect(first).toHaveBeenCalledOnce();
    expect(second).toHaveBeenCalledOnce();
    expect(setTokens).toHaveBeenCalledExactlyOnceWith('new-access', 'new-refresh');
    service.refreshToken().subscribe();
    requests.expectOne(url).flush({ accessToken: 'later-access', refreshToken: 'later-refresh' });
  });

  it('allows a new renewal attempt after a failed request', () => {
    service.refreshToken().subscribe({ error: () => {} });
    requests.expectOne(url).flush({}, { status: 503, statusText: 'Unavailable' });
    service.refreshToken().subscribe();
    requests.expectOne(url).flush({ accessToken: 'new-access', refreshToken: 'new-refresh' });
    expect(setTokens).toHaveBeenCalledOnce();
  });
});
