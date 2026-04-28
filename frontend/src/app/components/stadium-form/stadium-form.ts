import { Component, inject, OnInit } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputBox } from '../../shared/components/input-box/input-box';
import { Modal } from '../../shared/components/modal/modal';
import { CityService } from '../../core/services/city-services';
import { StadiumService } from '../../core/services/stadium.service';
import { Button } from '../../shared/components/button/button';
import { Dropdown } from '../../shared/components/dropdown/dropdown';
import { Map } from '../../shared/components/map/map';
import { ModalState } from '../../core/helpers/modal-state.helper';

@Component({
  selector: 'app-stadium-form',
  standalone: true,
  imports: [InputBox, Modal, ReactiveFormsModule, Button, Dropdown, Map],
  templateUrl: './stadium-form.html',
  styleUrl: './stadium-form.css',
})
export class StadiumForm implements OnInit {
  private fb = inject(NonNullableFormBuilder);
  modalState = new ModalState();
  
  private cityService = inject(CityService);
  private stadiumService = inject(StadiumService);
  cities: any[] = [];

  form = this.fb.group({
    name: this.fb.control('', { validators: Validators.required }),
    capacity: this.fb.control(0, { validators: [Validators.required, Validators.min(0)] }),
    dateOfConstruction: this.fb.control('', { validators: Validators.required }),
    pictureUrl: this.fb.control(''),
    cityId: this.fb.control('', { validators: Validators.required }),
    latitude: this.fb.control(40, { validators: Validators.required }),
    longitude: this.fb.control(41, { validators: Validators.required }),
    geoDescription: this.fb.control('', { validators: Validators.required })
  });

  ngOnInit(): void {
    this.loadCities();
  }
  
  loadCities(): void {
    this.cityService.getAll().subscribe({
      next: (data) => {
        this.cities = data;
      },
      error: (err) => {
        console.error("Error loading cities: ", err);
      }
    });
  }

  onLocationSelected(event: { lat: number, lng: number }) {
    this.form.patchValue({
      latitude: event.lat,
      longitude: event.lng
    });
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.stadiumService.create(this.form.getRawValue()).subscribe({
      next: (data) => {
        this.modalState.close(this.form);
        console.log("Stadium created: ", data);
        // Refresh list would be good here, maybe emit an event
      },
      error: (err) => console.error("Error creating stadium: ", err)
    });
  }

  openModal(): void {
    this.modalState.open();
  }
}
