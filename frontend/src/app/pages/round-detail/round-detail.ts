import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AppDateTimePipe } from '../../shared/pipes/regional-format.pipes';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, filter, map, of, shareReplay, switchMap } from 'rxjs';
import { RoundService } from '../../core/services/round.service';
import { RoundDetailsDto } from '../../core/models/round';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { apiErrorMessage } from '../../core/utils/api-error';

@Component({
  selector: 'app-round-detail',
  standalone: true,
  imports: [RouterLink, AppDateTimePipe, GenericListComponent],
  templateUrl: './round-detail.html',
  styleUrls: ['../catalog.css', '../competition-list/competition-pages.css'],
})
export class RoundDetail {
  private route = inject(ActivatedRoute);
  private roundService = inject(RoundService);

  private page$ = new BehaviorSubject<number>(1);
  readonly pageSize = 10;

  errorMessage = signal('');

  private round$ = this.route.paramMap.pipe(
    map((params) => params.get('slug') ?? ''),
    switchMap((slug) =>
      this.roundService.getBySlug(slug).pipe(
        catchError((err) => {
          this.errorMessage.set(apiErrorMessage(err, 'Round not found.'));
          return of(null);
        })
      )
    ),
    shareReplay(1)
  );

  round = toSignal<RoundDetailsDto | null>(this.round$);

  matches = toSignal(
    combineLatest([this.round$.pipe(filter((r): r is RoundDetailsDto => !!r)), this.page$]).pipe(
      switchMap(([round, page]) => this.roundService.getMatches(round.publicId, page, this.pageSize))
    )
  );

  onPageChange(page: number): void {
    this.page$.next(page);
  }
}
