import { Component, inject } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { validate } from '@angular/forms/signals';
import { Modal } from '../../shared/components/modal/modal';
import { InputBox } from '../../shared/components/input-box/input-box';
import { Button } from '../../shared/components/button/button';
import { CountryService } from '../../core/services/country-service';
import { count } from 'rxjs';

@Component({
  selector: 'app-country-form',
  imports: [Modal,InputBox,ReactiveFormsModule,Button],
  templateUrl: './country-form.html',
  styleUrl: './country-form.css',
})

export class CountryForm {
  isModalOpen=true;
  countryService=inject(CountryService);
  fb = inject(NonNullableFormBuilder);

  form=this.fb.group({
    name:this.fb.control('',{validators:[Validators.required]}),
    picture: this.fb.control<File | null>(null)
  });

  onFileChange(event: Event): void {
  const file = (event.target as HTMLInputElement).files?.[0] ?? null;
  this.form.controls.picture.setValue(file);
  }

  onSubmit(): void {
  if (this.form.invalid) return;

  const formData = new FormData();
  formData.append('name', this.form.controls.name.value);
  
  const picture = this.form.controls.picture.value;
  if (picture) formData.append('picture', picture);

  this.countryService.create(formData).subscribe({
    next: (result) => {
      console.log(`${result.name} uspješno kreirana!`); // toast ovdje
      this.isModalOpen = false;
      this.form.reset();
    },
    error: (err) => console.error(err)
  });
}

  dodaj():void{
    this.isModalOpen=true;
  }

}
