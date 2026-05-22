import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { GenericView } from '../../generic/generic-view/generic-view';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-manager-view',
  imports: [CommonModule, GenericView, RouterModule],
  templateUrl: './manager-view.html',
  styleUrl: './manager-view.css',
})
export class ManagerView implements OnInit {
  selectedManager: any;
  
  constructor(private route: ActivatedRoute, private cdr: ChangeDetectorRef, private router: Router) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    
    if (id) {
      fetch(`https://localhost:7208/api/manager/${id}`)
        .then(res => res.json())
        .then(data => {
          this.selectedManager = data;
          this.cdr.detectChanges();
        })
        .catch(err => console.error('Greška pri dohvaćanju:', err));
    }
  }

  goBack() {
    this.router.navigate(['/manager-list']);
  }
}
