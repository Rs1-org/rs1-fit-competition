import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { CityList } from './pages/city-list/city-list';
import { CountryList } from './pages/country-list/country-list';
import { ManagePlayers } from './pages/manage-players/manage-players';
import { ManageReferees } from './pages/manage-referees/manage-referees';
import { ManageManagers } from './pages/manage-managers/manage-managers';
import { ManageClubs } from './pages/manage-clubs/manage-clubs';
import { AuthGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'city-list', component: CityList },
  { path: 'country-list', component: CountryList },
  {
    path: 'stadium-list',
    loadComponent: () =>
      import('./pages/stadium-list/stadium-list').then((m) => m.StadiumList),
  },
  { path: 'manage-players', component: ManagePlayers },
  { path: 'manage-players/:id', component: ManagePlayers },
  { path: 'manage-referees', component: ManageReferees },
  { path: 'manage-referees/:id', component: ManageReferees },
  { path: 'manage-managers', component: ManageManagers },
  { path: 'manage-managers/:id', component: ManageManagers },
  {
    path: 'player-list',
    loadComponent: () =>
      import('./pages/player-list/player-list').then((m) => m.PlayerList),
  },
  {
    path: 'player/:id',
    loadComponent: () =>
      import('./pages/player-view/player-view').then((m) => m.PlayerView),
  },
  {path:'manage-clubs',component:ManageClubs, canActivate: [AuthGuard]},
  {
    path: 'referee-list',
    loadComponent: () =>
      import('./pages/referee-list/referee-list').then((m) => m.RefereeList),
  },
  {
    path: 'referee/:id',
    loadComponent: () =>
      import('./pages/referee-view/referee-view').then((m) => m.RefereeView),
  },
  {
    path: 'manager-list',
    loadComponent: () =>
      import('./pages/manager-list/manager-list').then((m) => m.ManagerList),
  },
  {
    path: 'manager/:id',
    loadComponent: () =>
      import('./pages/manager-view/manager-view').then((m) => m.ManagerView),
  },
  {
    path: 'profile',
    loadComponent: () =>
      import('./features/profile/profile.component').then(
        (m) => m.ProfileComponent,
      ),
    canActivate: [AuthGuard],
  },
];
