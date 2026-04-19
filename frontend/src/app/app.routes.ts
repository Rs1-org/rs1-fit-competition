import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { CityList } from './pages/city-list/city-list';
import { ManagePlayers } from './pages/manage-players/manage-players';
import { ManageReferees } from './pages/manage-referees/manage-referees';
import { ManageManagers } from './pages/manage-managers/manage-managers';
import { AuthGuard } from './core/guards/auth.guard';

export const routes: Routes = [    
    {path:'', component:CityList},
    {path:'', component:Home},
    {path:'manage-players',component:ManagePlayers},
    {path:'manage-referees',component:ManageReferees},
    {path:'manage-managers',component:ManageManagers},
    {path:'player-list', loadComponent: () => import('./pages/player-list/player-list').then(m => m.PlayerList)},
    {path:'player/:id', loadComponent: () => import('./pages/player-view/player-view').then(m => m.PlayerView)},
    {
        path: 'profile',
        loadComponent: () => import('./features/profile/profile.component')
            .then(m => m.ProfileComponent),
        canActivate: [AuthGuard]
    }
];
