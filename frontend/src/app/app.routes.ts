import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { AdminModeratorGuard } from './core/guards/admin-moderator.guard';

export const routes: Routes = [
  {
    path: 'clubs',
    loadComponent: () => import('./pages/club-list/club-list').then(m => m.ClubList),
  },
  {
    path: 'clubs/:slug',
    loadComponent: () => import('./pages/club-detail/club-detail').then(m => m.ClubDetail),
  },
  {
    path: 'manage-clubs',
    canActivate: [AdminModeratorGuard],
    loadComponent: () => import('./components/club-form/club-form').then(m => m.ClubForm),
  },
  {
    path: 'manage-clubs/:slug',
    canActivate: [AdminModeratorGuard],
    loadComponent: () => import('./components/club-form/club-form').then(m => m.ClubForm),
  },
  {
    path: '',
    loadComponent: () => import('./pages/home/home').then((m) => m.Home),
  },
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
  {
    path: 'manage-players',
    canActivate: [AdminModeratorGuard],
    loadComponent: () => import('./pages/manage-players/manage-players').then((m) => m.ManagePlayers),
  },
  {
    path: 'manage-players/:id',
    canActivate: [AdminModeratorGuard],
    loadComponent: () => import('./pages/manage-players/manage-players').then((m) => m.ManagePlayers),
  },
  {
    path: 'manage-referees',
    canActivate: [AdminModeratorGuard],
    loadComponent: () => import('./pages/manage-referees/manage-referees').then((m) => m.ManageReferees),
  },
  {
    path: 'manage-referees/:id',
    canActivate: [AdminModeratorGuard],
    loadComponent: () => import('./pages/manage-referees/manage-referees').then((m) => m.ManageReferees),
  },
  {
    path: 'manage-managers',
    canActivate: [AdminModeratorGuard],
    loadComponent: () => import('./pages/manage-managers/manage-managers').then((m) => m.ManageManagers),
  },
  {
    path: 'manage-managers/:id',
    canActivate: [AdminModeratorGuard],
    loadComponent: () => import('./pages/manage-managers/manage-managers').then((m) => m.ManageManagers),
  },
  {
    path: 'manage-formation-marks',
    canActivate: [AdminModeratorGuard],
    loadComponent: () =>
      import('./components/formation-mark-form/formation-mark-form').then((m) => m.FormationMarkForm),
  },
  {
    path: 'manage-formation-marks/:slug',
    canActivate: [AdminModeratorGuard],
    loadComponent: () =>
      import('./components/formation-mark-form/formation-mark-form').then((m) => m.FormationMarkForm),
  },
  {
    path: 'manage-formations',
    canActivate: [AdminModeratorGuard],
    loadComponent: () =>
      import('./components/formation-form/formation-form').then((m) => m.FormationForm),
  },
  {
    path: 'manage-formations/:slug',
    canActivate: [AdminModeratorGuard],
    loadComponent: () =>
      import('./components/formation-form/formation-form').then((m) => m.FormationForm),
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
    path: 'settings',
    loadComponent: () => import('./pages/settings/settings').then((m) => m.Settings),
    canActivate: [AuthGuard],
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
