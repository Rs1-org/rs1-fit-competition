import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { SelectionService } from './selection.service';

describe('SelectionService club contract', () => {
  let service: SelectionService;
  let http: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(SelectionService);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  it('combines the public club filter with paging and existing filters', () => {
    service
      .getAll(2, 10, { clubPublicId: 'a'.repeat(32), name: 'Youth', followersMin: 0 })
      .subscribe();
    const request = http.expectOne((req) => req.url.endsWith('/api/selection'));
    expect(request.request.params.get('clubPublicId')).toBe('a'.repeat(32));
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('name')).toBe('Youth');
    expect(request.request.params.get('followersMin')).toBe('0');
    request.flush({ items: [] });
  });

  it('sends the club public ID on create and update', () => {
    const body = {
      name: 'Youth',
      abbreviation: 'YTH',
      followersCount: 0,
      countryPublicId: 'b'.repeat(32),
      clubPublicId: 'a'.repeat(32),
    };
    service.create(body).subscribe();
    const create = http.expectOne((req) => req.method === 'POST');
    expect(create.request.body).toEqual(body);
    create.flush({});
    service.update('youth', body).subscribe();
    const update = http.expectOne((req) => req.method === 'PUT');
    expect(update.request.body.clubPublicId).toBe(body.clubPublicId);
    update.flush({});
  });
});
