import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-manage-players',
  imports: [CommonModule,FormsModule],
  templateUrl: './manage-players.html',
  styleUrl: './manage-players.css',
})
export class ManagePlayers {
  constructor(private router:Router) {}
  player = {
    height: 0,
    shirtNumber: 0,
    transferValue: 0
  };
  get isPlayerInvalid() {
  return (
    !this.player.height || this.player.height <= 0 ||
    !this.player.shirtNumber || this.player.shirtNumber <= 0 ||
    !this.player.transferValue || this.player.transferValue <= 0
  );
}
  async savePlayer() {
    const response = await fetch('https://localhost:7208/api/player', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(this.player)
    });
    if(response.ok) {
       const currentUrl = this.router.url;
    this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
      this.router.navigate([currentUrl]);
    });
    }
  }
  cancel() {
    this.player = {
      height: 0,
      shirtNumber: 0,
      transferValue: 0
    };
  }
}
