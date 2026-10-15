import { Component, inject } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { Router } from '@angular/router';
import { emptyPage } from '../../core/models/paged-result';
import { Player } from '../../core/models/person.models';
import { UserStateService } from '../../core/services/user-state.service';
import { PlayerListFilter, PlayerService } from '../../core/services/player.service';
import { PositionService } from '../../core/services/position.service';
import { SubPositionService } from '../../core/services/subposition.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { BehaviorSubject, combineLatest, debounceTime, filter, map, switchMap } from 'rxjs';

function ageRangeValidator(group: AbstractControl): ValidationErrors | null {
  const { ageMin, ageMax } = group.value;
  return ageMin != null && ageMax != null && ageMax < ageMin ? { ageRange: true } : null;
}

@Component({
  selector: 'app-player-list',
  imports: [GenericListComponent, CommonModule, ReactiveFormsModule],
  templateUrl: './player-list.html',
  styleUrls: ['../catalog.css', './player-list.css'],
})
export class PlayerList {
  private playerService = inject(PlayerService);
  private positionService = inject(PositionService);
  private subPositionService = inject(SubPositionService);
  private confirmDialog = inject(ConfirmDialogService);
  private router = inject(Router);
  private userState = inject(UserStateService);
  private fb = inject(FormBuilder);

  private refresh$ = new BehaviorSubject<void>(undefined);
  private query$ = new BehaviorSubject<{ page: number; filter: PlayerListFilter }>({ page: 1, filter: {} });
  readonly pageSize = 10;

  filterForm = this.fb.group({
    name: this.fb.nonNullable.control('', Validators.maxLength(100)),
    positionId: this.fb.nonNullable.control(''),
    subPositionId: this.fb.nonNullable.control(''),
    ageMin: this.fb.control<number | null>(null, Validators.min(0)),
    ageMax: this.fb.control<number | null>(null, Validators.min(0)),
  }, { validators: ageRangeValidator });

  positions = toSignal(this.positionService.getAll(1, 100).pipe(map((page) => page.items)), { initialValue: [] });
  subPositions = toSignal(this.subPositionService.getAll(1, 100).pipe(map((page) => page.items)), { initialValue: [] });

  result = toSignal(
    combineLatest([this.query$, this.refresh$, this.playerService.listRefreshChanges$]).pipe(
      switchMap(([{ page, filter }]) => this.playerService.getAll(page, this.pageSize, filter)),
    ),
    { initialValue: emptyPage<Player>() },
  );

  canManagePersons = toSignal(
    this.userState.roles$.pipe(map(() => this.userState.canManagePersons())),
    { initialValue: this.userState.canManagePersons() },
  );

  constructor() {
    this.filterForm.valueChanges.pipe(
      debounceTime(300),
      filter(() => this.filterForm.valid),
      takeUntilDestroyed(),
    ).subscribe(() => this.query$.next({ page: 1, filter: this.toFilter() }));

    this.filterForm.controls.positionId.valueChanges.pipe(takeUntilDestroyed())
      .subscribe(() => this.filterForm.controls.subPositionId.setValue(''));
  }

  get filteredSubPositions() {
    const positionId = this.filterForm.controls.positionId.value;
    const all = this.subPositions();
    return positionId ? all.filter((item) => item.positionId === positionId) : all;
  }

  add(): void {
    this.router.navigate(['/manage-players']);
  }

  view(player: Player): void {
    this.router.navigate(['/player', player.id]);
  }

  edit(player: Player): void {
    this.router.navigate(['/manage-players', player.id]);
  }

  async delete(player: Player): Promise<void> {
    if (!player.id) return;

    const confirmed = await this.confirmDialog.confirm({
      message: `Delete player ${player.firstName} ${player.lastName}?`,
    });
    if (!confirmed) return;

    this.playerService.delete(player.id).subscribe({
      next: () => this.refresh$.next(),
      error: (err) => console.error('Could not delete the player:', err),
    });
  }

  resetFilters(): void {
    this.filterForm.reset({ name: '', positionId: '', subPositionId: '' });
  }

  onPageChange(page: number): void {
    this.query$.next({ ...this.query$.value, page });
  }

  private toFilter(): PlayerListFilter {
    const { name, positionId, subPositionId, ageMin, ageMax } = this.filterForm.getRawValue();
    return {
      name: name.trim() || undefined,
      positionId: positionId || undefined,
      subPositionId: subPositionId || undefined,
      ageMin: ageMin ?? undefined,
      ageMax: ageMax ?? undefined,
    };
  }
}
