import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { Router } from '@angular/router';
import { Player } from '../../core/models/person.models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-player-list',
  imports: [GenericListComponent, CommonModule, GenericSearch],
  templateUrl: './player-list.html',
  styleUrl: './player-list.css',
})
export class PlayerList implements OnInit {
  players: Player[] = [];

  constructor(
    private cdr: ChangeDetectorRef,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.getAllPlayers();
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
