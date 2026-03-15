import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component } from '@angular/core';
import { GenericView } from '../../generic/generic-view/generic-view';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-player-view',
  imports: [CommonModule,GenericView,RouterModule],
  templateUrl: './player-view.html',
  styleUrl: './player-view.css',
})
export class PlayerView {
  selectedPlayer: any;
  
  constructor(private route: ActivatedRoute,private cdr:ChangeDetectorRef,private router: Router) {}
  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    
    if (id) {
      fetch(`https://localhost:7208/api/player/${id}`)
        .then(res => res.json())
        .then(data => {
          this.selectedPlayer = data;
          this.cdr.detectChanges();
        })
        .catch(err => console.error('Greška pri dohvaćanju:', err));
    }
  }
  goBack() {
    this.router.navigate(['/player-list']);
  }
}
