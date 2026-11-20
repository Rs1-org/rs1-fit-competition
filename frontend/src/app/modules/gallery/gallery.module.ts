import { NgModule } from '@angular/core';
import { GalleryRoutingModule } from './gallery-routing.module';
import { ImageGallery } from '../../pages/image-gallery/image-gallery';

/** Lazy feature area: competition image gallery. */
@NgModule({
  imports: [GalleryRoutingModule, ImageGallery],
})
export class GalleryModule {}