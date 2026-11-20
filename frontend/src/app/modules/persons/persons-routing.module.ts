import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminModeratorGuard } from '../../core/guards/admin-moderator.guard';
import { PlayerList } from '../../pages/player-list/player-list';
import { PlayerView } from '../../pages/player-view/player-view';
import { RefereeList } from '../../pages/referee-list/referee-list';
import { RefereeView } from '../../pages/referee-view/referee-view';
import { ManagerList } from '../../pages/manager-list/manager-list';
import { ManagerView } from '../../pages/manager-view/manager-view';
import { ManagePlayers } from '../../pages/manage-players/manage-players';
import { ManageReferees } from '../../pages/manage-referees/manage-referees';
import { ManageManagers } from '../../pages/manage-managers/manage-managers';

const routes: Routes = [
  { path: 'player-list', component: PlayerList },
  { path: 'player/:publicId', component: PlayerView },
  { path: 'referee-list', component: RefereeList },
  { path: 'referee/:publicId', component: RefereeView },
  { path: 'manager-list', component: ManagerList },
  { path: 'manager/:publicId', component: ManagerView },
  {
    path: 'manage-players',
    canActivate: [AdminModeratorGuard],
    component: ManagePlayers,
  },
  {
    path: 'manage-players/:publicId',
    canActivate: [AdminModeratorGuard],
    component: ManagePlayers,
  },
  {
    path: 'manage-referees',
    canActivate: [AdminModeratorGuard],
    component: ManageReferees,
  },
  {
    path: 'manage-referees/:publicId',
    canActivate: [AdminModeratorGuard],
    component: ManageReferees,
  },
  {
    path: 'manage-managers',
    canActivate: [AdminModeratorGuard],
    component: ManageManagers,
  },
  {
    path: 'manage-managers/:publicId',
    canActivate: [AdminModeratorGuard],
    component: ManageManagers,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PersonsRoutingModule {}