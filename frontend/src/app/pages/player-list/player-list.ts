import { Component,OnInit,ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';

interface Player {
  id: string;
  height: number;
  shirtNumber: number;
  transferValue: number;
}

@Component({
  selector: 'app-player-list',
  imports: [GenericListComponent,CommonModule,GenericSearch],
  templateUrl: './player-list.html',
  styleUrl: './player-list.css',
})
export class PlayerList {

players: any[] = [];

constructor(private cdr: ChangeDetectorRef) {}
// constructor() {
//   this.getAllPlayers();
// }
ngOnInit() {
    this.getAllPlayers();
  }
  getAllPlayers() {
    fetch('https://localhost:7208/api/player')
      .then(response => response.json())
      .then(data => {
        console.log(data)
        this.players = Array.isArray(data) ? data : [data];
        this.cdr.detectChanges();
        console.log(this.players);
      });
  }

  edit(player: any) {
    console.log('Uređujem igrača:', player.name);
    // Ovdje bi išla logika za otvaranje modala ili navigaciju
  }

  delete(player: any) {
    fetch(`https://localhost:7208/api/player/${player.id}`, {
      method: 'DELETE',
      headers: {
      'Content-Type': 'application/json', // Ovo rješava "Unsupported Media Type"
      'Accept': 'application/json'
    },
    body: JSON.stringify(player)
    })
    .then(response => {
      if (response.ok) {
        console.log('Igrač obrisan:', player.height);
        this.players = this.players.filter(p => p.id !== player.id);
        this.cdr.detectChanges();
        this.getAllPlayers();
      } else {
        console.error('Greška pri brisanju igrača:', response.statusText);
      }
    })
    .catch(error => console.error('Greška pri brisanju igrača:', error));
  }

  search(term: string) {
  // Šaljemo upit na backend
  fetch(`https://localhost:7208/api/player/search?term=${encodeURIComponent(term)}`)
    .then(res => res.json())
    .then(data => {
      this.players = Array.isArray(data) ? data : [data];
      this.cdr.detectChanges();
    });
  }
}
