import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { MatchReviewService } from './match-review.service';
import { environment } from '../../../environments/environment';

describe('Review API contracts', () => {
  it('paginates public reads and keeps owner identity out of write payloads', () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const service = TestBed.inject(MatchReviewService), requests = TestBed.inject(HttpTestingController);
    const id = 'a'.repeat(32), api = environment.apiUrl + '/api/reviews';
    service.getReviews(id, 2).subscribe();
    const read = requests.expectOne(r => r.url === `${api}/match/${id}`);
    expect(read.request.params.get('page')).toBe('2'); read.flush({});
    service.create(id, { rating: 4, comment: 'Review' }).subscribe();
    const write = requests.expectOne(`${api}/match/${id}`);
    expect(write.request.body).toEqual({ rating: 4, comment: 'Review' }); write.flush({ publicId: id });
    service.react(id, -1).subscribe();
    const reaction = requests.expectOne(`${api}/${id}/reaction`);
    expect(reaction.request.body).toEqual({ value: -1 }); reaction.flush({ value: -1 }); requests.verify();
  });
});
