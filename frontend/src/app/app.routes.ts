import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { AuthGuard } from './core/guards/auth.guard';

export const routes: Routes = [
    { path: '', component: Home },
    {
        path: 'profile',
        loadComponent: () => import('./features/profile/profile.component')
            .then(m => m.ProfileComponent),
        canActivate: [AuthGuard]
    }
];
