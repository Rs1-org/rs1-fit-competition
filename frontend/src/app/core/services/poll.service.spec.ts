import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { PollService } from './poll.service';
import { environment } from '../../../environments/environment';

describe('Poll API contract', () => {
  it('paginates reads and sends only an option public ID when voting', () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const service = TestBed.inject(PollService), http = TestBed.inject(HttpTestingController), id = 'a'.repeat(32), option = 'b'.repeat(32);
    service.getPage(id, 2).subscribe(); const read = http.expectOne(r => r.url === `${environment.apiUrl}/api/polls/match/${id}`);
    expect(read.request.params.get('page')).toBe('2'); read.flush({});
    service.vote(id, option).subscribe(); const write = http.expectOne(`${environment.apiUrl}/api/polls/${id}/vote`);
    expect(write.request.body).toEqual({ optionPublicId: option }); write.flush({}); http.verify();
  });
});
