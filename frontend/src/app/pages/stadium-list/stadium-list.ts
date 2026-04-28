import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { Router } from '@angular/router';
import { StadiumService } from '../../core/services/stadium.service';
import { StadiumListDto } from '../../core/models/stadium';
import { StadiumForm } from '../../components/stadium-form/stadium-form';

@Component({
  selector: 'app-stadium-list',
  standalone: true,
  imports: [GenericListComponent, CommonModule, GenericSearch, StadiumForm],
  templateUrl: './stadium-list.html',
  styleUrl: './stadium-list.css',
})
export class StadiumList implements OnInit {
  stadiums: StadiumListDto[] = [];
  private stadiumService = inject(StadiumService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  ngOnInit() {
    this.loadStadiums();
  }

  loadStadiums() {
    this.stadiumService.getAll().subscribe({
      next: (data) => {
        this.stadiums = data;
        this.cdr.detectChanges();
      },
      error: (err) => console.error("Error loading stadiums: ", err)
    });
  }

  view(stadium: StadiumListDto) {
    this.router.navigate(['/stadium', stadium.slug]);
  }

  edit(stadium: StadiumListDto) {
    console.log('Editing stadium:', stadium.name);
    // Logic for editing (could open modal with data)
  }

  delete(stadium: StadiumListDto) {
    if (confirm(`Are you sure you want to delete ${stadium.name}?`)) {
      this.stadiumService.delete(stadium.id).subscribe({
        next: () => {
          this.stadiums = this.stadiums.filter(s => s.id !== stadium.id);
          this.cdr.detectChanges();
        },
        error: (err) => console.error("Error deleting stadium: ", err)
      });
    }
  }

  search(term: string) {
    // If backend has search, use it. Otherwise filter locally.
    if (!term) {
        this.loadStadiums();
        return;
    }
    this.stadiums = this.stadiums.filter(s => 
        s.name?.toLowerCase().includes(term.toLowerCase()) || 
        s.cityName?.toLowerCase().includes(term.toLowerCase())
    );
  }
}
