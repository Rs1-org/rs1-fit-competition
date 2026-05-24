import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { Router } from '@angular/router';
import { Manager } from '../../core/models/person.models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-manager-list',
  imports: [GenericListComponent, CommonModule, GenericSearch],
  templateUrl: './manager-list.html',
  styleUrl: './manager-list.css',
})
export class ManagerList implements OnInit {
  managers: Manager[] = [];

  constructor(
    private cdr: ChangeDetectorRef,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.getAllManagers();
  }

  view(manager: Manager): void {
    this.router.navigate(['/manager', manager.id]);
  }

  getAllManagers(): void {
    fetch(`${environment.apiUrl}/api/manager`)
      .then((response) => response.json())
      .then((data) => {
        this.managers = Array.isArray(data) ? data : [data];
        this.cdr.detectChanges();
      })
      .catch((error) => console.error('Greška pri dohvaćanju menadžera:', error));
  }

  edit(manager: Manager): void {
    this.router.navigate(['/manage-managers', manager.id]);
  }

  delete(manager: Manager): void {
    fetch(`${environment.apiUrl}/api/manager/${manager.id}`, {
      method: 'DELETE',
    })
      .then((response) => {
        if (response.ok) {
          this.managers = this.managers.filter((m) => m.id !== manager.id);
          this.cdr.detectChanges();
        }
      })
      .catch((error) => console.error('Greška pri brisanju menadžera:', error));
  }

  search(term: string): void {
    fetch(`${environment.apiUrl}/api/manager/search?term=${encodeURIComponent(term)}`)
      .then((res) => res.json())
      .then((data) => {
        this.managers = Array.isArray(data) ? data : [data];
        this.cdr.detectChanges();
      })
      .catch((error) => console.error('Greška pri pretraživanju menadžera:', error));
  }
}
