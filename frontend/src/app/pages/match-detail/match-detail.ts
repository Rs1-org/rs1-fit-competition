import { Component, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, map, of, switchMap } from 'rxjs';
import { AppDateTimePipe, AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { MatchService } from '../../core/services/match.service';
import { UserStateService } from '../../core/services/user-state.service';
import { MatchDetailsDto } from '../../core/models/match';
import { roundLabel } from '../../core/helpers/match.helper';

@Component({
  selector: 'app-match-detail',
  imports: [RouterLink, AppDateTimePipe, AppNumberPipe],
  templateUrl: './match-detail.html',
  styleUrls: ['../stadium-detail/stadium-detail.css', './match-detail.css'],
})
export class MatchDetail {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private matchService = inject(MatchService);
  canManage = inject(UserStateService).canManage;
  roundLabel = roundLabel;

  /** undefined = loading, null = not found. */
  match = toSignal(
    this.route.paramMap.pipe(
      map((params) => params.get('slug') ?? ''),
      switchMap((slug) =>
        this.matchService.getBySlug(slug).pipe(catchError(() => of(null as MatchDetailsDto | null))),
      ),
    ),
    { initialValue: undefined },
  );

  hasScore(match: MatchDetailsDto): boolean {
    return match.homeScore !== null && match.awayScore !== null;
  }

  goBack(): void {
    this.router.navigate(['/matches']);
  }
}
