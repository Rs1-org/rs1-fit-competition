import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, map, of, startWith, switchMap } from 'rxjs';
import { ClubService } from '../../core/services/club.service';
import { ClubSelections } from '../../components/club-selections/club-selections';
import { UserStateService } from '../../core/services/user-state.service';
import { CLUB_AGES, ClubDetailsDto } from '../../core/models/club';
import { apiErrorMessage } from '../../core/utils/api-error';
import { AppDatePipe } from '../../shared/pipes/regional-format.pipes';

@Component({
  selector: 'app-club-detail',
  imports: [RouterLink, AppDatePipe, ClubSelections],
  templateUrl: './club-detail.html',
  styleUrl: '../club-list/club-pages.css',
})
export class ClubDetail {
  private readonly service = inject(ClubService);
  private readonly route = inject(ActivatedRoute);
  private readonly refresh$ = new BehaviorSubject<void>(undefined);
  readonly user = inject(UserStateService);
  readonly ages = CLUB_AGES;
  readonly state = toSignal(
    combineLatest([this.route.paramMap, this.refresh$]).pipe(
      switchMap(([params]) =>
        this.service.getBySlug(params.get('slug') ?? '').pipe(
          map((data) => ({ data, loading: false, error: '' })),
          startWith({ data: null as ClubDetailsDto | null, loading: true, error: '' }),
          catchError((err) =>
            of({
              data: null,
              loading: false,
              error: apiErrorMessage(err, 'Club could not be loaded.'),
            }),
          ),
        ),
      ),
    ),
    { initialValue: { data: null, loading: true, error: '' } },
  );

  retry(): void {
    this.refresh$.next();
  }
}
