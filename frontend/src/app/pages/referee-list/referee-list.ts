import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { Router } from '@angular/router';

@Component({
  selector: 'app-referee-list',
  imports: [GenericListComponent, CommonModule, GenericSearch],
  templateUrl: './referee-list.html',
  styleUrl: './referee-list.css',
})
export class RefereeList implements OnInit {

  referees: any[] = [];

  constructor(private cdr: ChangeDetectorRef, private router: Router) {}

  ngOnInit() {
    this.getAllReferees();
  }

  view(referee: any) {
    this.router.navigate(['/referee', referee.id]);
  }

  getAllReferees() {
    fetch('https://localhost:7208/api/referee')
      .then(response => response.json())
      .then(data => {
        this.referees = Array.isArray(data) ? data : [data];
        this.cdr.detectChanges();
      })
      .catch(error => console.error('Greška pri dohvaćanju sudija:', error));
  }

  edit(referee: any) {
    console.log('Uređujem sudiju:', referee.id);
  }

  delete(referee: any) {
    fetch(`https://localhost:7208/api/referee`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      },
      body: JSON.stringify(referee)
    })
    .then(response => {
      if (response.ok) {
        console.log('Sudija obrisan:', referee.id);
        this.referees = this.referees.filter(r => r.id !== referee.id);
        this.cdr.detectChanges();
        this.getAllReferees();
      } else {
        console.error('Greška pri brisanju sudije:', response.statusText);
      }
    })
    .catch(error => console.error('Greška pri brisanju sudije:', error));
  }

  search(term: string) {
    fetch(`https://localhost:7208/api/referee/search?term=${encodeURIComponent(term)}`)
      .then(res => res.json())
      .then(data => {
        this.referees = Array.isArray(data) ? data : [data];
        this.cdr.detectChanges();
      })
      .catch(error => console.error('Greška pri pretraživanju sudija:', error));
  }
}
