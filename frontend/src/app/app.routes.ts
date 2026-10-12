import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { ManagePlayers } from './pages/manage-players/manage-players';
import { ManageReferees } from './pages/manage-referees/manage-referees';
import { ManageManagers } from './pages/manage-managers/manage-managers';
import { AuthGuard } from './core/guards/auth.guard';
import { AdminModeratorGuard } from './core/guards/admin-moderator.guard';

export const routes: Routes = [
  { path: '', component: Home },
  {
    path: 'city-list',
    loadComponent: () =>
      import('./pages/city-list/city-list').then((m) => m.CityList),
  },
  {
    path: 'country-list',
    loadComponent: () =>
      import('./pages/country-list/country-list').then((m) => m.CountryList),
  },
  {
    path: 'stadium-list',
    loadComponent: () =>
      import('./pages/stadium-list/stadium-list').then((m) => m.StadiumList),
  },
  {
    path: 'stadium/:slug',
    loadComponent: () =>
      import('./pages/stadium-detail/stadium-detail').then((m) => m.StadiumDetail),
  },
  {
    path: 'competitions',
    loadComponent: () =>
      import('./pages/competition-list/competition-list').then((m) => m.CompetitionList),
  },
  {
    path: 'competitions/:slug',
    loadComponent: () =>
      import('./pages/competition-detail/competition-detail').then((m) => m.CompetitionDetail),
  },
  {
    path: 'seasons/:slug',
    loadComponent: () =>
      import('./pages/season-detail/season-detail').then((m) => m.SeasonDetail),
  },
  {
    path: 'rounds/:slug',
    loadComponent: () =>
      import('./pages/round-detail/round-detail').then((m) => m.RoundDetail),
  },
  { path: 'manage-players', component: ManagePlayers, canActivate: [AdminModeratorGuard] },
  { path: 'manage-players/:id', component: ManagePlayers, canActivate: [AdminModeratorGuard] },
  { path: 'manage-referees', component: ManageReferees, canActivate: [AdminModeratorGuard] },
  { path: 'manage-referees/:id', component: ManageReferees, canActivate: [AdminModeratorGuard] },
  { path: 'manage-managers', component: ManageManagers, canActivate: [AdminModeratorGuard] },
  { path: 'manage-managers/:id', component: ManageManagers, canActivate: [AdminModeratorGuard] },
  {
    path: 'manage-formation-marks',
    canActivate: [AdminModeratorGuard],
    loadComponent: () =>
      import('./pages/manage-formation-mark/manage-formation-mark').then((m) => m.ManageFormationMark),
  },
  {
    path: 'manage-formation-marks/:slug',
    canActivate: [AdminModeratorGuard],
    loadComponent: () =>
      import('./pages/manage-formation-mark/manage-formation-mark').then((m) => m.ManageFormationMark),
  },
  {
    path: 'manage-formations',
    canActivate: [AdminModeratorGuard],
    loadComponent: () =>
      import('./pages/manage-formation/manage-formation').then((m) => m.ManageFormation),
  },
  {
    path: 'manage-formations/:slug',
    canActivate: [AdminModeratorGuard],
    loadComponent: () =>
      import('./pages/manage-formation/manage-formation').then((m) => m.ManageFormation),
  },
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
    path: 'formation-mark-list',
    loadComponent: () =>
      import('./pages/formation-mark-list/formation-mark-list').then((m) => m.FormationMarkList),
  },
  {
    path: 'formation-list',
    loadComponent: () =>
      import('./pages/formation-list/formation-list').then((m) => m.FormationList),
  },
  {
    path: 'selection-list',
    loadComponent: () =>
      import('./pages/selection-list/selection-list').then((m) => m.SelectionList),
  },
  {
    path: 'image-gallery',
    canActivate: [AdminModeratorGuard],
    loadComponent: () =>
      import('./pages/image-gallery/image-gallery').then((m) => m.ImageGallery),
  },
  {
    path: 'manage-selections',
    canActivate: [AdminModeratorGuard],
    loadComponent: () =>
      import('./pages/manage-selection/manage-selection').then((m) => m.ManageSelection),
  },
  {
    path: 'manage-selections/:slug',
    canActivate: [AdminModeratorGuard],
    loadComponent: () =>
      import('./pages/manage-selection/manage-selection').then((m) => m.ManageSelection),
  },
  {
    path: 'position-list',
    loadComponent: () =>
      import('./pages/position-list/position-list').then((m) => m.PositionList),
  },
  {
    path: 'subposition-list',
    loadComponent: () =>
      import('./pages/subposition-list/subposition-list').then((m) => m.SubPositionList),
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
