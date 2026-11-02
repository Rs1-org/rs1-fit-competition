import { Component, computed, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { EMPTY, map, switchMap, timer } from 'rxjs';
import { AppDateTimePipe, AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { MatchService } from '../../core/services/match.service';
import { UserStateService } from '../../core/services/user-state.service';
import { MatchDetailsDto } from '../../core/models/match';
import { saveBlob } from '../../core/helpers/download.helper';
import { apiErrorMessage } from '../../core/utils/api-error';
import { ToastService } from '../../core/services/toast.service';
import { goalscorers, kickoffCountdown, latestMinute, matchPhase, statusNote } from '../../shared/match/match-display';
import { Breadcrumb, Crumb } from '../../shared/components/breadcrumb/breadcrumb';
import { Crest } from '../../shared/components/crest/crest';
import { ScoreDisplay } from '../../shared/components/score-display/score-display';
import { StatusBadge } from '../../shared/components/status-badge/status-badge';
import { SectionTabs, SectionTab } from '../../shared/components/section-tabs/section-tabs';
import { EmptyState } from '../../shared/components/empty-state/empty-state';
import { Skeleton } from '../../shared/components/skeleton/skeleton';
import { Icon } from '../../shared/components/icon/icon';
import { HasRoleDirective } from '../../shared/directives/has-role.directive';
import { MatchPageStore } from './match-page.store';

const COUNTDOWN_REFRESH_MS = 30_000;

@Component({
  selector: 'app-match-detail',
  imports: [RouterLink, RouterOutlet, AppDateTimePipe, AppNumberPipe, Breadcrumb, Crest, ScoreDisplay, StatusBadge, SectionTabs, EmptyState, Skeleton, Icon, HasRoleDirective],
  providers: [MatchPageStore],
  templateUrl: './match-detail.html',
  styleUrl: './match-detail.css',
})
export class MatchDetail {
  private store = inject(MatchPageStore);
  private matchService = inject(MatchService);
  private toast = inject(ToastService);
  private userState = inject(UserStateService);
  canManage = this.userState.canManage;

  readonly crestSize = 72;
  match = this.store.match;
  lineup = this.store.lineup;

  private isScheduled = computed(() => {
    const match = this.match();
    return !!match && matchPhase(match.statusName, match.isPostponed) === 'scheduled';
  });
  private now = toSignal(
    toObservable(this.isScheduled).pipe(
      switchMap((scheduled) => (scheduled ? timer(0, COUNTDOWN_REFRESH_MS).pipe(map(() => Date.now())) : EMPTY)),
    ),
    { initialValue: Date.now() },
  );

  homeGoals = computed(() => {
    const match = this.match();
    return match ? goalscorers(this.store.matchEvents(), match.homeSelectionPublicId, match.awaySelectionPublicId) : [];
  });
  awayGoals = computed(() => {
    const match = this.match();
    return match ? goalscorers(this.store.matchEvents(), match.awaySelectionPublicId, match.homeSelectionPublicId) : [];
  });
  liveMinute = computed(() => latestMinute(this.store.matchEvents()));

  connectionNote = computed(() => {
    const match = this.match();
    const phase = match ? matchPhase(match.statusName, match.isPostponed) : null;
    if (phase !== 'live' && phase !== 'half-time') return '';
    const state = this.store.connection();
    if (state === 'reconnecting') return 'Connection lost. Reconnecting; the score may be out of date.';
    if (state === 'unavailable') return 'Match updates unavailable. Reload this page to reconnect.';
    return '';
  });

  statusNote(match: MatchDetailsDto): string {
    const phase = matchPhase(match.statusName, match.isPostponed);
    return phase === 'scheduled' ? kickoffCountdown(match.date, this.now()) : statusNote(phase);
  }

  downloadReport(match: MatchDetailsDto): void {
    this.matchService.downloadReport(match.publicId).subscribe({
      next: (blob) => saveBlob(blob, `match-report-${match.slug ?? match.publicId}.pdf`),
      error: (err) => this.toast.error(apiErrorMessage(err, 'The report could not be downloaded.')),
    });
  }

  winner(match: MatchDetailsDto, side: 'home' | 'away'): boolean {
    if (match.homeScore === null || match.awayScore === null || match.homeScore === match.awayScore) return false;
    return side === 'home' ? match.homeScore > match.awayScore : match.awayScore > match.homeScore;
  }

  crumbs(match: MatchDetailsDto): Crumb[] {
    const name = { label: `${match.homeSelectionName ?? 'TBD'} vs ${match.awaySelectionName ?? 'TBD'}` };
    if (!match.competitionName) return [{ label: 'Matches', route: '/matches' }, name];
    return [
      { label: 'Competitions', route: '/competitions' },
      { label: match.competitionName, route: match.competitionSlug ? `/competitions/${match.competitionSlug}` : null },
      { label: match.seasonName ?? 'Season', route: match.seasonSlug ? `/seasons/${match.seasonSlug}` : null },
      { label: match.roundName ?? 'Round', route: match.roundSlug ? `/rounds/${match.roundSlug}` : null },
      name,
    ];
  }

  tabs(match: MatchDetailsDto): SectionTab[] {
    const base = `/matches/${match.slug}`;
    return [
      { label: 'Summary', route: base },
      { label: 'Line-ups', route: `${base}/lineups` },
      { label: 'Statistics', route: `${base}/statistics` },
      { label: 'Polls', route: `${base}/polls` },
      { label: 'Reviews', route: `${base}/reviews` },
      { label: 'Head-to-head', route: `${base}/head-to-head`, locked: !this.userState.isPremium() },
    ];
  }
}
