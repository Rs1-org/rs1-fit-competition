import { Component, input, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, map, of, startWith, switchMap } from 'rxjs';
import { SelectionOption, SelectionService } from '../../core/services/selection.service';
import { emptyPage } from '../../core/models/paged-result';
import { AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { apiErrorMessage } from '../../core/utils/api-error';

@Component({
  selector: 'app-club-selections',
  imports: [RouterLink, AppNumberPipe],
  templateUrl: './club-selections.html',
  styleUrl: './club-selections.css',
})
export class ClubSelections {
  readonly clubPublicId = input.required<string>();
  private readonly service = inject(SelectionService);
  private readonly page$ = new BehaviorSubject(1);
  private readonly refresh$ = new BehaviorSubject<void>(undefined);
  readonly state = toSignal(
    toObservable(this.clubPublicId).pipe(
      switchMap((clubPublicId) => {
        this.page$.next(1);
        return combineLatest([this.page$, this.refresh$]).pipe(
          switchMap(([page]) =>
            this.service.getAll(page, 10, { clubPublicId }).pipe(
              map((data) => ({ data, loading: false, error: '' })),
              startWith({ data: emptyPage<SelectionOption>(), loading: true, error: '' }),
              catchError((err) =>
                of({
                  data: emptyPage<SelectionOption>(),
                  loading: false,
                  error: apiErrorMessage(err, 'Selections could not be loaded.'),
                }),
              ),
            ),
          ),
        );
      }),
    ),
    { initialValue: { data: emptyPage<SelectionOption>(), loading: true, error: '' } },
  );

  page(page: number): void {
    this.page$.next(page);
  }
  retry(): void {
    this.refresh$.next();
  }
}
