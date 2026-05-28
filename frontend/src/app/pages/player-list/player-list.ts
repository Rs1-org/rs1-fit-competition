import { Component, OnInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { Router } from '@angular/router';
import { Player } from '../../core/models/person.models';
import { environment } from '../../../environments/environment';
import { UserStateService } from '../../core/services/user-state.service';
import { authJsonHeaders } from '../../core/helpers/auth-fetch.helper';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-player-list',
  imports: [GenericListComponent, CommonModule, GenericSearch],
  templateUrl: './player-list.html',
  styleUrl: './player-list.css',
})
export class PlayerList implements OnInit, OnDestroy {
  players: Player[] = [];
  canManagePersons = false;
  private rolesSub?: Subscription;

  constructor(
    private cdr: ChangeDetectorRef,
    private router: Router,
    private userState: UserStateService,
  ) {}

  ngOnInit(): void {
    this.canManagePersons = this.userState.canManagePersons();
    this.rolesSub = this.userState.roles$.subscribe(() => {
      this.canManagePersons = this.userState.canManagePersons();
      this.cdr.detectChanges();
    });
    this.getAllPlayers();
  }

  ngOnDestroy(): void {
    this.rolesSub?.unsubscribe();
  }

  view(player: Player): void {
    this.router.navigate(['/player', player.id]);
  }

  getAllPlayers(): void {
    fetch(`${environment.apiUrl}/api/player`)
      .then((response) => response.json())
      .then((data) => {
        this.players = Array.isArray(data) ? data : [data];
        this.cdr.detectChanges();
      });
  }

  edit(player: Player): void {
    this.router.navigate(['/manage-players', player.id]);
  }

  delete(player: Player): void {
    fetch(`${environment.apiUrl}/api/player/${player.id}`, {
      method: 'DELETE',
      headers: authJsonHeaders(),
    })
      .then((response) => {
        if (response.ok) {
          this.players = this.players.filter((p) => p.id !== player.id);
          this.cdr.detectChanges();
        }
      })
      .catch((error) => console.error('Greška pri brisanju igrača:', error));
  }

  search(term: string): void {
    fetch(`${environment.apiUrl}/api/player/search?term=${encodeURIComponent(term)}`)
      .then((res) => res.json())
      .then((data) => {
        this.players = Array.isArray(data) ? data : [data];
        this.cdr.detectChanges();
      });
  }
}
