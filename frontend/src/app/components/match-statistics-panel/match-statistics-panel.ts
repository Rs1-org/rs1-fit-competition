import { Component, inject, Input, OnChanges } from '@angular/core';
import { RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, catchError, filter, map, of, startWith, switchMap } from 'rxjs';
import { Statistic } from '../../core/models/statistic';
import { StatisticService } from '../../core/services/statistic.service';
import { UserStateService } from '../../core/services/user-state.service';
import { StatisticComparison } from '../statistic-comparison/statistic-comparison';
import { apiErrorMessage } from '../../core/utils/api-error';

interface StatisticState { loading: boolean; data: Statistic | null; error: string; }
@Component({ selector: 'app-match-statistics-panel', imports: [RouterLink, StatisticComparison],
  templateUrl: './match-statistics-panel.html', styleUrl: '../../pages/statistics/statistics.css' })
export class MatchStatisticsPanel implements OnChanges {
  @Input({ required: true }) matchPublicId = '';
  private readonly service = inject(StatisticService);
  readonly canManage = inject(UserStateService).canManage;
  private readonly refresh$ = new BehaviorSubject('');
  readonly state = toSignal(this.refresh$.pipe(filter(Boolean), switchMap(id => this.service.getForMatch(id).pipe(
    map((data): StatisticState => ({ loading: false, data, error: '' })),
    startWith({ loading: true, data: null, error: '' } as StatisticState),
    catchError(err => of({ loading: false, data: null,
      error: err instanceof HttpErrorResponse && err.status === 404 ? '' : apiErrorMessage(err, 'Statistics could not be loaded.') } as StatisticState)),
  ))), { initialValue: { loading: true, data: null, error: '' } as StatisticState });
  ngOnChanges(): void { this.refresh$.next(this.matchPublicId); }
  retry(): void { this.refresh$.next(this.matchPublicId); }
}
