import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { MatchService } from './match.service';

describe('MatchService', () => {
  let service: MatchService;
  let http: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(MatchService);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  it('sends paging and only the filters that are set, including a false postponed flag', () => {
    service.getAll(2, 10, { statusPublicId: 's1', isPostponed: false, dateFrom: '2026-10-01' }).subscribe();
    const request = http.expectOne((req) => req.url.endsWith('/api/match/getAll'));
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('statusPublicId')).toBe('s1');
    expect(request.request.params.get('isPostponed')).toBe('false');
    expect(request.request.params.get('dateFrom')).toBe('2026-10-01');
    expect(request.request.params.has('stadiumPublicId')).toBe(false);
    request.flush({ items: [], totalCount: 0, page: 2, pageSize: 10 });
  });

  it('updates by slug and deletes by public id', () => {
    const body = {
      date: '2026-11-01T18:00:00.000Z', followersCount: 0, isPostponed: false, statusPublicId: 's',
      stadiumPublicId: 'st', homeSelectionPublicId: null, awaySelectionPublicId: null, roundPublicId: null,
    };
    service.update('derby', body).subscribe();
    http.expectOne((req) => req.method === 'PUT' && req.url.endsWith('/api/match/update/derby')).flush({});
    service.delete('abc').subscribe();
    http.expectOne((req) => req.method === 'DELETE' && req.url.endsWith('/api/match/delete/abc')).flush('ok');
  });
});
