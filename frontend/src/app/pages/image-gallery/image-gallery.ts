import { HttpErrorResponse, HttpEventType } from '@angular/common/http';
import { Component, OnDestroy, inject, signal } from '@angular/core';
import { forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { GalleryItem, GallerySection, GalleryService } from '../../core/services/gallery.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { UploadProgress } from '../../shared/components/upload-progress/upload-progress';

@Component({
  selector: 'app-image-gallery',
  imports: [UploadProgress],
  templateUrl: './image-gallery.html',
  styleUrl: './image-gallery.css',
})
export class ImageGallery implements OnDestroy {
  private gallery = inject(GalleryService);
  private confirmDialog = inject(ConfirmDialogService);

  sections = signal<GallerySection[]>([]);
  previews = signal<Record<string, string>>({});
  loading = signal(true);
  error = signal('');
  progressKey = signal<string | null>(null);
  progress = signal(0);
  viewer = signal<{ name: string; src: string } | null>(null);

  constructor() {
    this.reload();
  }

  ngOnDestroy(): void {
    this.revokeBlobs(this.previews());
  }

  preview(item: GalleryItem): string {
    return this.previews()[this.key(item)] || '';
  }

  isUploading(item: GalleryItem): boolean {
    return this.progressKey() === this.key(item);
  }

  async upload(item: GalleryItem, event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;

    const key = this.key(item);
    this.error.set('');
    this.progressKey.set(key);
    this.progress.set(0);

    this.gallery.upload(item.kind, item.id, file).subscribe({
      next: (uploadEvent) => {
        if (uploadEvent.type === HttpEventType.UploadProgress) {
          const total = uploadEvent.total ?? file.size;
          this.progress.set(total ? Math.min(100, Math.round((100 * uploadEvent.loaded) / total)) : 0);
        }
        if (uploadEvent.type === HttpEventType.Response) {
          this.progress.set(100);
          this.reload();
        }
      },
      error: (err: HttpErrorResponse) => {
        this.progressKey.set(null);
        this.error.set(this.readError(err));
      },
    });
  }

  async remove(item: GalleryItem): Promise<void> {
    const confirmed = await this.confirmDialog.confirm({
      message: `Obrisati sliku za ${item.name}?`,
    });
    if (!confirmed) return;

    this.error.set('');
    this.gallery.remove(item.kind, item.id).subscribe({
      next: () => this.reload(),
      error: (err: HttpErrorResponse) => this.error.set(this.readError(err)),
    });
  }

  open(item: GalleryItem): void {
    const src = this.preview(item);
    if (!src) return;
    this.viewer.set({ name: item.name, src });
  }

  closeViewer(): void {
    this.viewer.set(null);
  }

  private reload(): void {
    this.loading.set(true);
    this.gallery.list().subscribe({
      next: (sections) => {
        this.sections.set(sections);
        this.loading.set(false);
        this.progressKey.set(null);
        this.loadPreviews(sections);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.progressKey.set(null);
        this.error.set(this.readError(err));
      },
    });
  }

  private loadPreviews(sections: GallerySection[]): void {
    const next: Record<string, string> = {};
    const jobs = [];

    for (const section of sections) {
      for (const item of section.withPicture) {
        const key = this.key(item);
        if (item.pictureUrl) {
          next[key] = item.pictureUrl;
          continue;
        }
        jobs.push(
          this.gallery.image(item.kind, item.id).pipe(
            map((blob) => ({ key, url: URL.createObjectURL(blob) })),
            catchError(() => of(null)),
          ),
        );
      }
    }

    const apply = (extra: Record<string, string>) => {
      const merged = { ...next, ...extra };
      this.revokeBlobs(this.previews(), merged);
      this.previews.set(merged);
    };

    if (!jobs.length) {
      apply({});
      return;
    }

    forkJoin(jobs).subscribe((results) => {
      const extra: Record<string, string> = {};
      for (const result of results) {
        if (result) extra[result.key] = result.url;
      }
      apply(extra);
    });
  }

  private revokeBlobs(previous: Record<string, string>, keep: Record<string, string> = {}): void {
    for (const [key, url] of Object.entries(previous)) {
      if (url.startsWith('blob:') && keep[key] !== url) URL.revokeObjectURL(url);
    }
  }

  private key(item: GalleryItem): string {
    return `${item.kind}:${item.id}`;
  }

  private readError(err: HttpErrorResponse): string {
    const body = err.error;
    const validation = body?.errors?.[0]?.errorMessage;
    return validation || body?.message || 'Radnja nad slikom nije uspjela.';
  }
}
