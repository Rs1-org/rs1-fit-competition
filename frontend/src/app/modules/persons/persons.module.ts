import { NgModule } from '@angular/core';
import { PersonsRoutingModule } from './persons-routing.module';
import { PlayerList } from '../../pages/player-list/player-list';
import { PlayerView } from '../../pages/player-view/player-view';
import { RefereeList } from '../../pages/referee-list/referee-list';
import { RefereeView } from '../../pages/referee-view/referee-view';
import { ManagerList } from '../../pages/manager-list/manager-list';
import { ManagerView } from '../../pages/manager-view/manager-view';
import { ManagePlayers } from '../../pages/manage-players/manage-players';
import { ManageReferees } from '../../pages/manage-referees/manage-referees';
import { ManageManagers } from '../../pages/manage-managers/manage-managers';

/** Lazy feature area: players, referees and managers. */
@NgModule({
  imports: [
    PersonsRoutingModule,
    PlayerList,
    PlayerView,
    RefereeList,
    RefereeView,
    ManagerList,
    ManagerView,
    ManagePlayers,
    ManageReferees,
    ManageManagers,
  ],
})
export class PersonsModule {}