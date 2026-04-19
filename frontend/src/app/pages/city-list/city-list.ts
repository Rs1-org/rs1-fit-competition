import { Component } from '@angular/core';
import { Navbar } from "../../shared/components/navbar/navbar";
import { CityForm } from "../../components/city-form/city-form";
import { ModalState } from '../../core/helpers/modal-state.helper';

@Component({
  selector: 'app-city-list',
  imports: [Navbar, CityForm],
  templateUrl: './city-list.html',
  styleUrl: './city-list.css',
})
export class CityList {}
