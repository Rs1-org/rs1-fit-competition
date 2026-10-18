import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ClubService } from './club.service';

describe('ClubService', () => {
  let service: ClubService;
  let http: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ClubService);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  it('sends server paging, false stadium filters and the youngest age group', () => {
    service
      .getAll(2, 10, { name: 'Club', hasStadium: false, clubAge: 0, foundedFrom: '1900-01-01' })
      .subscribe();
    const request = http.expectOne((req) => req.url.endsWith('/api/club'));
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('hasStadium')).toBe('false');
    expect(request.request.params.get('clubAge')).toBe('0');
    expect(request.request.params.get('foundedFrom')).toBe('1900-01-01');
    request.flush({ items: [], totalCount: 0, page: 2, pageSize: 10 });
  });

  it('deletes using the public identifier', () => {
    const publicId = 'a'.repeat(32);
    service.delete(publicId).subscribe();
    const request = http.expectOne((req) => req.url.endsWith('/delete/' + publicId));
    expect(request.request.method).toBe('DELETE');
    request.flush({ publicId });
  });
});
