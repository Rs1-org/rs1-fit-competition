import { Routes } from '@angular/router';
import { Navbar } from './shared/components/navbar/navbar';
import { Home } from './pages/home/home';
import { ManagePlayers } from './pages/manage-players/manage-players';
import { ManageReferees } from './pages/manage-referees/manage-referees';
import { ManageManagers } from './pages/manage-managers/manage-managers';

export const routes: Routes = [
    {path:'', component:Home},
    {path:'manage-players',component:ManagePlayers},
    {path:'manage-referees',component:ManageReferees},
    {path:'manage-managers',component:ManageManagers}
];
