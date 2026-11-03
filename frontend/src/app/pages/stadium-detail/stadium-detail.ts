import { Component, computed, inject, ViewChild } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AppDayLabelPipe, AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { StadiumService } from '../../core/services/stadium.service';
import { MatchService } from '../../core/services/match.service';
import { UserStateService } from '../../core/services/user-state.service';
import { StadiumDetailsDto } from '../../core/models/stadium';
import { MatchListDto } from '../../core/models/match';
import { PagedResult } from '../../core/models/paged-result';
import { apiErrorMessage } from '../../core/utils/api-error';
import { Map } from '../../shared/components/map/map';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, filter, map, of, startWith, switchMap } from 'rxjs';
import { ErrorBanner } from '../../shared/components/error-banner/error-banner';
import { EmptyState } from '../../shared/components/empty-state/empty-state';
import { Skeleton } from '../../shared/components/skeleton/skeleton';
import { Breadcrumb, Crumb } from '../../shared/components/breadcrumb/breadcrumb';
import { StatTile } from '../../shared/components/stat-tile/stat-tile';
import { Button } from '../../shared/components/button/button';
import { Icon } from '../../shared/components/icon/icon';
import { StadiumForm } from '../../components/stadium-form/stadium-form';
import { matchPhase, scoreText } from '../../shared/match/match-display';
import * as L from 'leaflet';

/** How many matches the side card shows; the full list is one click away. */
const RECENT_MATCHES = 5;

interface StadiumState { data: StadiumDetailsDto | null; loading: boolean; error: string; notFound: boolean; }
interface MatchesState { data: PagedResult<MatchListDto> | null; loading: boolean; error: string; }

@Component({
  selector: 'app-stadium-detail',
  imports: [
    AppDayLabelPipe, AppNumberPipe, Map, RouterLink, ErrorBanner, EmptyState, Skeleton, Breadcrumb, StatTile, Button, Icon, StadiumForm,
  ],
  templateUrl: './stadium-detail.html',
  styleUrl: './stadium-detail.css',
})
export class StadiumDetail {
  @ViewChild(StadiumForm) stadiumForm?: StadiumForm;

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private stadiumService = inject(StadiumService);
  private matchService = inject(MatchService);
  canManage = inject(UserStateService).canManage;

  private readonly reload$ = new BehaviorSubject<void>(undefined);
  private readonly matchesRefresh$ = new BehaviorSubject<void>(undefined);

  state = toSignal(
    combineLatest([this.route.paramMap.pipe(map((params) => params.get('slug') ?? '')), this.reload$]).pipe(
      switchMap(([slug]) => this.stadiumService.getBySlug(slug).pipe(
        map((data): StadiumState => ({ data, loading: false, error: '', notFound: false })),
        startWith<StadiumState>({ data: null, loading: true, error: '', notFound: false }),
        catchError((err) => of<StadiumState>(err instanceof HttpErrorResponse && err.status === 404
          ? { data: null, loading: false, error: '', notFound: true }
          : { data: null, loading: false, error: apiErrorMessage(err, 'The stadium could not be loaded.'), notFound: false })),
      )),
    ),
    { initialValue: { data: null, loading: true, error: '', notFound: false } as StadiumState },
  );

  stadium = computed(() => this.state().data);
  private stadium$ = toObservable(this.stadium).pipe(filter((s): s is StadiumDetailsDto => !!s));

  /** The latest matches at this stadium, newest first. */
  matches = toSignal(
    combineLatest([this.stadium$, this.matchesRefresh$]).pipe(
      switchMap(([s]) => this.matchService.getAll(1, RECENT_MATCHES, { stadiumPublicId: s.publicId, sortBy: 'date', sortDir: 'desc' }).pipe(
        map((data): MatchesState => ({ data, loading: false, error: '' })),
        startWith<MatchesState>({ data: null, loading: true, error: '' }),
        catchError((err) => of<MatchesState>({ data: null, loading: false, error: apiErrorMessage(err, 'Matches could not be loaded.') })),
      )),
    ),
    { initialValue: { data: null, loading: true, error: '' } as MatchesState },
  );

  /** Finished matches here; only the count is needed. null while loading or when it failed. */
  playedCount = toSignal(
    combineLatest([this.stadium$, this.matchesRefresh$]).pipe(
      switchMap(([s]) => this.matchService.getAll(1, 1, { stadiumPublicId: s.publicId, phase: 'results' }).pipe(
        map((data): number | null => data.totalCount),
        startWith(null),
        catchError(() => of(null)),
      )),
    ),
    { initialValue: null },
  );

  crumbs = computed<Crumb[]>(() => [
    { label: 'Stadiums', route: '/stadium-list' },
    { label: this.stadium()?.name ?? 'Stadium' },
  ]);

  builtYear = computed(() => {
    const date = this.stadium()?.dateOfConstruction;
    return date ? new Date(date).getFullYear() : null;
  });

  /** 43.8686° N, 18.4093° E, in the regional number format. */
  coordinates = computed(() => {
    const s = this.stadium();
    if (!s) return '';
    const pipe = new AppNumberPipe();
    const lat = `${pipe.transform(Math.abs(s.latitude), '1.4-4')}° ${s.latitude < 0 ? 'S' : 'N'}`;
    const lng = `${pipe.transform(Math.abs(s.longitude), '1.4-4')}° ${s.longitude < 0 ? 'W' : 'E'}`;
    return `${lat}, ${lng}`;
  });

  mapsUrl = computed(() => {
    const s = this.stadium();
    return s ? `https://www.openstreetmap.org/?mlat=${s.latitude}&mlon=${s.longitude}#map=16/${s.latitude}/${s.longitude}` : '';
  });

  // Straight-line (Haversine) distance from the stadium to its city centre, in km.
  distanceToCityKm = computed(() => {
    const s = this.stadium();
    if (!s || s.cityLatitude == null || s.cityLongitude == null) return null;
    const meters = L.latLng(s.latitude, s.longitude).distanceTo(L.latLng(s.cityLatitude, s.cityLongitude));
    return Math.round(meters / 100) / 10;
  });

  isLive(match: MatchListDto): boolean {
    const phase = matchPhase(match.statusName, match.isPostponed);
    return phase === 'live' || phase === 'half-time';
  }

  score(match: MatchListDto): string {
    return scoreText(match.homeScore, match.awayScore);
  }

  retry(): void {
    this.reload$.next();
  }

  retryMatches(): void {
    this.matchesRefresh$.next();
  }

  edit(): void {
    const s = this.stadium();
    if (s) this.stadiumForm?.openForEdit(s);
  }

  /** A renamed stadium gets a new slug, so follow it; otherwise just reload. */
  onSaved(saved: StadiumDetailsDto): void {
    if (saved?.slug && saved.slug !== this.stadium()?.slug) {
      this.router.navigate(['/stadium', saved.slug], { replaceUrl: true });
    } else {
      this.reload$.next();
    }
  }

  scheduleMatch(publicId: string): void {
    this.router.navigate(['/manage-matches'], { queryParams: { stadium: publicId } });
  }
}
