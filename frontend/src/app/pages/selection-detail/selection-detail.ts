import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, map, of, startWith, switchMap } from 'rxjs';
import { SelectionOption, SelectionService } from '../../core/services/selection.service';
import { UserStateService } from '../../core/services/user-state.service';
import { apiErrorMessage } from '../../core/utils/api-error';
import { AppNumberPipe } from '../../shared/pipes/regional-format.pipes';

@Component({
  selector: 'app-selection-detail',
  imports: [RouterLink, AppNumberPipe],
  templateUrl: './selection-detail.html',
  styleUrl: '../club-list/club-pages.css',
})
export class SelectionDetail {
  private readonly service = inject(SelectionService);
  private readonly route = inject(ActivatedRoute);
  private readonly refresh$ = new BehaviorSubject<void>(undefined);
  readonly user = inject(UserStateService);
  readonly state = toSignal(
    combineLatest([this.route.paramMap, this.refresh$]).pipe(
      switchMap(([params]) =>
        this.service.getBySlug(params.get('slug') ?? '').pipe(
          map((data) => ({ data, loading: false, error: '' })),
          startWith({ data: null as SelectionOption | null, loading: true, error: '' }),
          catchError((err) =>
            of({
              data: null,
              loading: false,
              error: apiErrorMessage(err, 'Selection could not be loaded.'),
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
