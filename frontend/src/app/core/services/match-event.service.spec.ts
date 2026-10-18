import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { MatchEventService } from './match-event.service';
import { environment } from '../../../environments/environment';

describe('Event API contract', () => {
  it('sends all filters and paging without empty optional references', () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const service = TestBed.inject(MatchEventService), http = TestBed.inject(HttpTestingController);
    service.getPage({ matchPublicId: 'a'.repeat(32), eventTypePublicId: 'b'.repeat(32), selectionPublicId: 'c'.repeat(32), playerPublicId: 'd'.repeat(32), minuteFrom: 0, minuteTo: 50 }, 2).subscribe();
    const request = http.expectOne(r => r.url === `${environment.apiUrl}/api/events`);
    expect(request.request.params.get('minuteFrom')).toBe('0'); expect(request.request.params.get('minuteTo')).toBe('50');
    expect(request.request.params.get('page')).toBe('2'); expect(request.request.params.get('pageSize')).toBe('20');
    expect(request.request.params.get('playerPublicId')).toBe('d'.repeat(32)); request.flush({});
    service.getPage({ playerPublicId: '', minuteFrom: null }).subscribe();
    const empty = http.expectOne(r => r.url === `${environment.apiUrl}/api/events`);
    expect(empty.request.params.has('playerPublicId')).toBe(false); expect(empty.request.params.has('minuteFrom')).toBe(false);
    empty.flush({}); http.verify();
  });
});
