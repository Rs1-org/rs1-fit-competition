import { Component } from '@angular/core';
import { CountryForm } from '../../components/country-form/country-form';
// import { Navbar } from '../../shared/components/navbar/navbar';

@Component({
  selector: 'app-country-list',
  imports: [CountryForm],
  templateUrl: './country-list.html',
  styleUrl: './country-list.css',
})
export class CountryList {
  
}
