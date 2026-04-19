import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { GenericView } from '../../generic/generic-view/generic-view';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-referee-view',
  imports: [CommonModule, GenericView, RouterModule],
  templateUrl: './referee-view.html',
  styleUrl: './referee-view.css',
})
export class RefereeView implements OnInit {
  selectedReferee: any;
  
  constructor(private route: ActivatedRoute, private cdr: ChangeDetectorRef, private router: Router) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    
    if (id) {
      fetch(`https://localhost:7208/api/referee/${id}`)
        .then(res => res.json())
        .then(data => {
          this.selectedReferee = data;
          this.cdr.detectChanges();
        })
        .catch(err => console.error('Greška pri dohvaćanju:', err));
    }
  }

  goBack() {
    this.router.navigate(['/referee-list']);
  }
}
