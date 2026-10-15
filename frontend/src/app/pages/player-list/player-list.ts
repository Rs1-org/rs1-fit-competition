import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { Router } from '@angular/router';
import { emptyPage } from '../../core/models/paged-result';
import { Player } from '../../core/models/person.models';
import { UserStateService } from '../../core/services/user-state.service';
import { PlayerService } from '../../core/services/player.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { BehaviorSubject, combineLatest, map, switchMap } from 'rxjs';

@Component({
  selector: 'app-player-list',
  imports: [GenericListComponent, CommonModule, GenericSearch],
  templateUrl: './player-list.html',
  styleUrls: ['../catalog.css', './player-list.css'],
})
export class PlayerList {
  private playerService = inject(PlayerService);
  private confirmDialog = inject(ConfirmDialogService);
  private router = inject(Router);
  private userState = inject(UserStateService);

  private searchTerm$ = new BehaviorSubject<string>('');
  private page$ = new BehaviorSubject<number>(1);
  private refresh$ = new BehaviorSubject<void>(undefined);
  readonly pageSize = 10;

  result = toSignal(
    combineLatest([this.page$, this.searchTerm$, this.refresh$, this.playerService.listRefreshChanges$]).pipe(
      switchMap(([page, term]) =>
        this.playerService.getAll(page, this.pageSize, { name: term.trim() || undefined }),
      ),
    ),
    { initialValue: emptyPage<Player>() },
  );

  canManagePersons = toSignal(
    this.userState.roles$.pipe(map(() => this.userState.canManagePersons())),
    { initialValue: this.userState.canManagePersons() },
  );

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
      error: (err) => console.error('Greška pri brisanju igrača:', err),
    });
  }

  search(term: string): void {
    this.searchTerm$.next(term);
    this.page$.next(1);
  }

  onPageChange(page: number): void {
    this.page$.next(page);
  }
}
