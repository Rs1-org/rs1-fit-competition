import { NgTemplateOutlet } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { catchError, map, of, startWith, switchMap } from 'rxjs';
import { AppDateTimePipe, AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { MatchService } from '../../core/services/match.service';
import { LineupService } from '../../core/services/lineup.service';
import { emptyLineup, LineupPlayer, MatchLineup } from '../../core/models/lineup';
import { UserStateService } from '../../core/services/user-state.service';
import { MatchDetailsDto } from '../../core/models/match';
import { roundLabel } from '../../core/helpers/match.helper';
import { saveBlob } from '../../core/helpers/download.helper';
import { apiErrorMessage } from '../../core/utils/api-error';
import { ToastService } from '../../core/services/toast.service';
import { matchPhase } from '../../shared/match/match-display';
import { Breadcrumb, Crumb } from '../../shared/components/breadcrumb/breadcrumb';
import { Crest } from '../../shared/components/crest/crest';
import { ScoreDisplay } from '../../shared/components/score-display/score-display';
import { StatusBadge } from '../../shared/components/status-badge/status-badge';
import { LiveCard } from '../../shared/components/live-card/live-card';
import { SectionCard } from '../../shared/components/section-card/section-card';
import { StatTile } from '../../shared/components/stat-tile/stat-tile';
import { SectionTabs, SectionTab } from '../../shared/components/section-tabs/section-tabs';
import { EmptyState } from '../../shared/components/empty-state/empty-state';
import { HasRoleDirective } from '../../shared/directives/has-role.directive';

@Component({
  selector: 'app-match-detail',
  imports: [NgTemplateOutlet, RouterLink, AppDateTimePipe, AppNumberPipe, Breadcrumb, Crest, ScoreDisplay, StatusBadge, LiveCard, SectionCard, StatTile, SectionTabs, EmptyState, HasRoleDirective],
  templateUrl: './match-detail.html',
  styleUrls: ['../stadium-detail/stadium-detail.css', './match-detail.css'],
})
export class MatchDetail {
  private route = inject(ActivatedRoute);
  private matchService = inject(MatchService);
  private lineupService = inject(LineupService);
  private toast = inject(ToastService);
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

  lineup = toSignal(
    toObservable(this.match).pipe(
      switchMap((match) => {
        if (!match) return of(null as MatchLineup | null);
        return this.lineupService.get(match.publicId).pipe(catchError(() => of(emptyLineup(match.publicId))));
      }),
      startWith(undefined as MatchLineup | null | undefined),
    ),
  );

  downloadReport(match: MatchDetailsDto): void {
    this.matchService.downloadReport(match.publicId).subscribe({
      next: (blob) => saveBlob(blob, `match-report-${match.slug ?? match.publicId}.pdf`),
      error: (err) => this.toast.error(apiErrorMessage(err, 'The report could not be downloaded.')),
    });
  }

  hasScore(match: MatchDetailsDto): boolean {
    return match.homeScore !== null && match.awayScore !== null;
  }

  isLive(match: MatchDetailsDto): boolean {
    return matchPhase(match.statusName, match.isPostponed) === 'live';
  }

  winner(match: MatchDetailsDto, side: 'home' | 'away'): boolean {
    if (match.homeScore === null || match.awayScore === null || match.homeScore === match.awayScore) return false;
    return side === 'home' ? match.homeScore > match.awayScore : match.awayScore > match.homeScore;
  }

  crumbs(match: MatchDetailsDto): Crumb[] {
    return [
      { label: 'Matches', route: '/matches' },
      { label: match.competitionName || 'Competition' },
      { label: `${match.homeSelectionName ?? 'TBD'} vs ${match.awaySelectionName ?? 'TBD'}` },
    ];
  }

  starters(players: LineupPlayer[]): LineupPlayer[] {
    return players.filter((player) => player.isStarter);
  }

  substitutes(players: LineupPlayer[]): LineupPlayer[] {
    return players.filter((player) => !player.isStarter);
  }

  tabs(match: MatchDetailsDto): SectionTab[] {
    return [
      { label: 'Live', route: `/live-matches/${match.publicId}` },
      { label: 'Events', route: `/match-events/${match.publicId}` },
      { label: 'Statistics', route: `/match-statistics/${match.publicId}` },
      { label: 'Reviews', route: `/match-reviews/${match.publicId}` },
    ];
  }

}
