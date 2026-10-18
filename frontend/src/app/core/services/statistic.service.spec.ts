import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { StatisticService } from './statistic.service';
import { TeamStatisticValues } from '../models/statistic';
import { environment } from '../../../environments/environment';

describe('Statistic API contracts', () => {
  it('preserves server paging and sends only expected create/update payloads', () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const service = TestBed.inject(StatisticService), http = TestBed.inject(HttpTestingController), api = `${environment.apiUrl}/api/statistic`;
    const team: TeamStatisticValues = { totalShots: 0, shotsOnTarget: 0, totalRedCards: 0, totalYellowCards: 0, corners: 0, bigChances: 0, possession: 50 };
    const input = { home: team, away: team }, match = 'a'.repeat(32), statistic = 'b'.repeat(32);
    service.getPage({ matchPublicId: match, minTotalShots: 0 }, 2).subscribe();
    const read = http.expectOne(r => r.url === api); expect(read.request.params.get('page')).toBe('2');
    expect(read.request.params.get('minTotalShots')).toBe('0'); read.flush({});
    service.create(match, input).subscribe(); const create = http.expectOne(api);
    expect(create.request.body).toEqual({ matchPublicId: match, ...input }); create.flush({});
    service.update(statistic, input).subscribe(); const update = http.expectOne(`${api}/${statistic}`);
    expect(update.request.body).toEqual(input); expect(update.request.method).toBe('PUT'); update.flush({}); http.verify();
  });
});
