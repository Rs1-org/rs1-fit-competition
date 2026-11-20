import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminModeratorGuard } from '../../core/guards/admin-moderator.guard';
import { ImageGallery } from '../../pages/image-gallery/image-gallery';

const routes: Routes = [
  {
    path: 'image-gallery',
    canActivate: [AdminModeratorGuard],
    component: ImageGallery,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class GalleryRoutingModule {}