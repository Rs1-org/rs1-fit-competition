import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { Router } from '@angular/router';

@Component({
  selector: 'app-manager-list',
  imports: [GenericListComponent, CommonModule, GenericSearch],
  templateUrl: './manager-list.html',
  styleUrl: './manager-list.css',
})
export class ManagerList implements OnInit {

  managers: any[] = [];

  constructor(private cdr: ChangeDetectorRef, private router: Router) {}

  ngOnInit() {
    this.getAllManagers();
  }

  view(manager: any) {
    this.router.navigate(['/manager', manager.id]);
  }

  getAllManagers() {
    fetch('https://localhost:7208/api/manager')
      .then(response => response.json())
      .then(data => {
        this.managers = Array.isArray(data) ? data : [data];
        this.cdr.detectChanges();
      })
      .catch(error => console.error('Greška pri dohvaćanju menadžera:', error));
  }

  edit(manager: any) {
    console.log('Uređujem menadžera:', manager.id);
  }

  delete(manager: any) {
    fetch(`https://localhost:7208/api/manager`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      },
      body: JSON.stringify(manager)
    })
    .then(response => {
      if (response.ok) {
        console.log('Menadžer obrisan:', manager.id);
        this.managers = this.managers.filter(m => m.id !== manager.id);
        this.cdr.detectChanges();
        this.getAllManagers();
      } else {
        console.error('Greška pri brisanju menadžera:', response.statusText);
      }
    })
    .catch(error => console.error('Greška pri brisanju menadžera:', error));
  }

  search(term: string) {
    fetch(`https://localhost:7208/api/manager/search?term=${encodeURIComponent(term)}`)
      .then(res => res.json())
      .then(data => {
        this.managers = Array.isArray(data) ? data : [data];
        this.cdr.detectChanges();
      })
      .catch(error => console.error('Greška pri pretraživanju menadžera:', error));
  }
}
