import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { authInterceptor } from './auth.interceptor';
import { TokenService } from '../services/token.service';
import { environment } from '../../../environments/environment';

describe('authInterceptor', () => {
  let http: HttpClient;
  let requests: HttpTestingController;
  const accessToken = vi.fn();

  beforeEach(() => {
    accessToken.mockReset().mockReturnValue('secret-access');
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: TokenService, useValue: { getAccessToken: accessToken } },
      ],
    });
    http = TestBed.inject(HttpClient);
    requests = TestBed.inject(HttpTestingController);
  });
  afterEach(() => requests.verify());

  it('adds the bearer token only for the configured API origin', () => {
    const url = `${environment.apiUrl}/api/settings`;
    http.get(url).subscribe();
    const request = requests.expectOne(url);
    expect(request.request.headers.get('Authorization')).toBe('Bearer secret-access');
    request.flush({});
  });

  it('does not attach a token for third-party URLs', () => {
    const url = 'https://example.com.evil.com/api/settings';
    http.get(url).subscribe();
    const request = requests.expectOne(url);
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush({});
  });
});