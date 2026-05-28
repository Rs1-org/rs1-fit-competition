import { Component, OnInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { Router } from '@angular/router';
import { Manager } from '../../core/models/person.models';
import { environment } from '../../../environments/environment';
import { UserStateService } from '../../core/services/user-state.service';
import { authJsonHeaders } from '../../core/helpers/auth-fetch.helper';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-manager-list',
  imports: [GenericListComponent, CommonModule, GenericSearch],
  templateUrl: './manager-list.html',
  styleUrl: './manager-list.css',
})
export class ManagerList implements OnInit, OnDestroy {
  managers: Manager[] = [];
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
    this.getAllManagers();
  }

  ngOnDestroy(): void {
    this.rolesSub?.unsubscribe();
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
      headers: authJsonHeaders(),
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
