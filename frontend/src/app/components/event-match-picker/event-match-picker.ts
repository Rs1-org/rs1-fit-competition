import { Component, inject, input, output } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, map, of, startWith, switchMap } from 'rxjs';
import { MatchReviewService } from '../../core/services/match-review.service';
import { ReviewMatch } from '../../core/models/match-review';
import { PagedResult } from '../../core/models/paged-result';
import { Pagination } from '../../shared/components/pagination/pagination';
import { AppDateTimePipe } from '../../shared/pipes/regional-format.pipes';
import { apiErrorMessage } from '../../core/utils/api-error';

interface PickerState { loading: boolean; data: PagedResult<ReviewMatch> | null; error: string; }
@Component({ selector: 'app-event-match-picker', imports: [Pagination, AppDateTimePipe],
  templateUrl: './event-match-picker.html', styleUrl: '../../pages/match-events/events.css' })
export class EventMatchPicker {
  readonly selected = input('');
  readonly selectedName = input('Selected match');
  readonly disabled = input(false);
  readonly changeMatch = output<string>();
  private readonly service = inject(MatchReviewService);
  private readonly page$ = new BehaviorSubject(1);
  readonly state = toSignal(this.page$.pipe(switchMap(page => this.service.getMatches(page).pipe(
    map((data): PickerState => ({ loading: false, data, error: '' })),
    startWith({ loading: true, data: null, error: '' } as PickerState),
    catchError(err => of({ loading: false, data: null, error: apiErrorMessage(err, 'Matches could not be loaded.') } as PickerState)),
  ))), { initialValue: { loading: true, data: null, error: '' } as PickerState });
  page(page: number): void { this.page$.next(page); }
  containsSelected(): boolean { return !!this.state().data?.items.some(x => x.publicId === this.selected()); }
}
