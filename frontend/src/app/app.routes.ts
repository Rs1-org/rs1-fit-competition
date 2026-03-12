import { Routes } from '@angular/router';
import { Navbar } from './shared/components/navbar/navbar';
import { Home } from './pages/home/home';
import { Modal } from './shared/components/modal/modal';
import { CountryList } from './pages/country-list/country-list';

export const routes: Routes = [
    {path:'', component:CountryList}
];
