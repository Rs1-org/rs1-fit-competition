import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { Router } from '@angular/router';
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
  private refresh$ = new BehaviorSubject<void>(undefined);

  players = toSignal(
    combineLatest([this.searchTerm$, this.refresh$, this.playerService.listRefreshChanges$]).pipe(
      switchMap(([term]) =>
        term.trim() ? this.playerService.search(term) : this.playerService.getAll(),
      ),
    ),
    { initialValue: [] as Player[] },
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
      message: `Jeste li sigurni da želite obrisati igrača ${player.firstName} ${player.lastName}?`,
    });
    if (!confirmed) return;

    this.playerService.delete(player.id).subscribe({
      next: () => this.refresh$.next(),
      error: (err) => console.error('Greška pri brisanju igrača:', err),
    });
  }

  search(term: string): void {
    this.searchTerm$.next(term);
  }
}
