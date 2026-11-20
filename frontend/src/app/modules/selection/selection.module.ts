import { NgModule } from '@angular/core';
import { SelectionRoutingModule } from './selection-routing.module';
import { SelectionList } from '../../pages/selection-list/selection-list';
import { SelectionDetail } from '../../pages/selection-detail/selection-detail';
import { ManageSelection } from '../../pages/manage-selection/manage-selection';

/** Lazy feature area: national teams (selections). Standalone page components are imported into this NgModule. */
@NgModule({
  imports: [SelectionRoutingModule, SelectionList, SelectionDetail, ManageSelection],
})
export class SelectionModule {}