import { Component, OnInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { Router } from '@angular/router';
import { Referee } from '../../core/models/person.models';
import { environment } from '../../../environments/environment';
import { UserStateService } from '../../core/services/user-state.service';
import { authJsonHeaders } from '../../core/helpers/auth-fetch.helper';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-referee-list',
  imports: [GenericListComponent, CommonModule, GenericSearch],
  templateUrl: './referee-list.html',
  styleUrl: './referee-list.css',
})
export class RefereeList implements OnInit, OnDestroy {
  referees: Referee[] = [];
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
    this.getAllReferees();
  }

  ngOnDestroy(): void {
    this.rolesSub?.unsubscribe();
  }

  view(referee: Referee): void {
    this.router.navigate(['/referee', referee.id]);
  }

  getAllReferees(): void {
    fetch(`${environment.apiUrl}/api/referee`)
      .then((response) => response.json())
      .then((data) => {
        this.referees = Array.isArray(data) ? data : [data];
        this.cdr.detectChanges();
      })
      .catch((error) => console.error('Greška pri dohvaćanju sudija:', error));
  }

  edit(referee: Referee): void {
    this.router.navigate(['/manage-referees', referee.id]);
  }

  delete(referee: Referee): void {
    fetch(`${environment.apiUrl}/api/referee/${referee.id}`, {
      method: 'DELETE',
      headers: authJsonHeaders(),
    })
      .then((response) => {
        if (response.ok) {
          this.referees = this.referees.filter((r) => r.id !== referee.id);
          this.cdr.detectChanges();
        }
      })
      .catch((error) => console.error('Greška pri brisanju sudije:', error));
  }

  search(term: string): void {
    fetch(`${environment.apiUrl}/api/referee/search?term=${encodeURIComponent(term)}`)
      .then((res) => res.json())
      .then((data) => {
        this.referees = Array.isArray(data) ? data : [data];
        this.cdr.detectChanges();
      })
      .catch((error) => console.error('Greška pri pretraživanju sudija:', error));
  }
}
