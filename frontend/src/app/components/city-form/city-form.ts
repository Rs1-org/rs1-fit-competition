import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputBox } from '../../shared/components/input-box/input-box';
import { Modal } from '../../shared/components/modal/modal';
import { CountryListDto } from '../../core/models/country';
import { CountryService } from '../../core/services/country-service';
import { CityService } from '../../core/services/city-services';
import { Button } from '../../shared/components/button/button';
import { Dropdown } from '../../shared/components/dropdown/dropdown';
import { Map } from '../../shared/components/map/map';
import { toFormData } from '../../core/helpers/form-data.helper';
import { ModalState } from '../../core/helpers/modal-state.helper';

@Component({
  selector: 'app-city-form',
  imports: [InputBox,Modal,ReactiveFormsModule,Button,Dropdown,Map],
  templateUrl: './city-form.html',
  styleUrl: './city-form.css',
})
export class CityForm implements OnInit {

  private fb=inject(NonNullableFormBuilder);

  modalState=new ModalState;
  
  isModalOpen=false;
  private countryService=inject(CountryService);
  private cityService=inject(CityService);
  countries:CountryListDto[]=[];

  form=this.fb.group({
    name:this.fb.control('', {validators:Validators.required}),
    population:this.fb.control(0,{validators:Validators.required}),
    isCapital:this.fb.control(false),
    countryId:this.fb.control(1, {validators:Validators.required}),
    latitude:this.fb.control(40, {validators:Validators.required}),
    longitude:this.fb.control(41),
    description:this.fb.control('',{validators:Validators.required})
  });

  ngOnInit(): void {
    this.loadCountries();
  }
  
  loadCountries():void{
    this.countryService.getAll().subscribe({
      next:(data)=>{
        this.countries=data
      },
      error:(err)=>{
        console.error("Error: ", err);
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
    if (this.form.invalid){
      this.form.markAllAsTouched();
      return;
    }


    this.cityService.create(this.form.value).subscribe({
      next: (data) =>{
        this.modalState.close(this.form);
        console.log("City is created: ", data)
      },
      error: (err) => console.error(err)
    });
  }

  dodaj():void{
    this.modalState.open();
  }
  
  
  
}
