import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { ClubAge, ClubResponseDto, StadiumOption } from '../../core/models/club';
import { CityListDto } from '../../core/models/city';
import { ClubService } from '../../core/services/club-service';
import { Navbar } from '../../shared/components/navbar/navbar';

@Component({
  selector: 'app-manage-clubs',
  imports: [CommonModule, ReactiveFormsModule, Navbar],
  templateUrl: './manage-clubs.html',
  styleUrl: './manage-clubs.css'
})
export class ManageClubs implements OnInit {
  private fb = inject(NonNullableFormBuilder);
  private clubService = inject(ClubService);

  clubs: ClubResponseDto[] = [];
  cities: CityListDto[] = [];
  stadiums: StadiumOption[] = [];
  clubAges: ClubAge[] = ['U7', 'U9', 'U11', 'U13', 'U16', 'U19', 'U21', 'Senior'];
  editingSlug: string | null = null;
  errorMessage = '';

  form = this.fb.group({
    name: this.fb.control('', [Validators.required, Validators.maxLength(100)]),
    foundingDate: this.fb.control('', Validators.required),
    cityId: this.fb.control('', Validators.required),
    clubAge: this.fb.control<ClubAge>('Senior', Validators.required),
    stadiumId: this.fb.control('', Validators.required)
  });

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    forkJoin({
      clubs: this.clubService.getAll(),
      cities: this.clubService.getCities(),
      stadiums: this.clubService.getStadiums()
    }).subscribe({
      next: ({ clubs, cities, stadiums }) => {
        this.clubs = clubs;
        this.cities = cities;
        this.stadiums = stadiums;
      },
      error: () => {
        this.errorMessage = 'Failed to load club data.';
      }
    });
  }

  submit(): void {
    this.errorMessage = '';
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const request = this.form.getRawValue();
    const action = this.editingSlug
      ? this.clubService.update(this.editingSlug, request)
      : this.clubService.create(request);

    action.subscribe({
      next: () => {
        this.resetForm();
        this.loadData();
      },
      error: (error) => {
        this.errorMessage = error?.error?.message || 'Club could not be saved.';
      }
    });
  }

  edit(club: ClubResponseDto): void {
    this.editingSlug = club.slug || null;
    this.form.patchValue({
      name: club.name,
      foundingDate: club.foundingDate || '',
      cityId: club.cityId || '',
      clubAge: club.clubAge,
      stadiumId: club.stadiumId || ''
    });
  }

  delete(club: ClubResponseDto): void {
    this.clubService.delete(club.id).subscribe({
      next: () => this.loadData(),
      error: () => {
        this.errorMessage = 'Club could not be deleted.';
      }
    });
  }

  resetForm(): void {
    this.editingSlug = null;
    this.form.reset({
      name: '',
      foundingDate: '',
      cityId: '',
      clubAge: 'Senior',
      stadiumId: ''
    });
  }
}
