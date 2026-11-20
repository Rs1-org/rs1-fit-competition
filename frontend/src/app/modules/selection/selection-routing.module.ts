import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminModeratorGuard } from '../../core/guards/admin-moderator.guard';
import { SelectionList } from '../../pages/selection-list/selection-list';
import { SelectionDetail } from '../../pages/selection-detail/selection-detail';
import { ManageSelection } from '../../pages/manage-selection/manage-selection';

const routes: Routes = [
  { path: 'selection-list', component: SelectionList },
  { path: 'selection/:slug', component: SelectionDetail },
  {
    path: 'manage-selections',
    canActivate: [AdminModeratorGuard],
    component: ManageSelection,
  },
  {
    path: 'manage-selections/:slug',
    canActivate: [AdminModeratorGuard],
    component: ManageSelection,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SelectionRoutingModule {}