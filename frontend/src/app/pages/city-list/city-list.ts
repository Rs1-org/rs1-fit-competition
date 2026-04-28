import { Component } from '@angular/core';
import { CityForm } from "../../components/city-form/city-form";

@Component({
  selector: 'app-city-list',
  imports: [CityForm],
  templateUrl: './city-list.html',
  styleUrl: './city-list.css',
})
export class CityList {}
