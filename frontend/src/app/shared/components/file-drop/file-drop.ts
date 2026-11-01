import { Component, input, output, signal } from '@angular/core';

@Component({
  selector: 'app-file-drop',
  templateUrl: './file-drop.html',
  styleUrl: './file-drop.css',
})
export class FileDrop {
  readonly accept = input('image/jpeg,image/png,image/webp');
  readonly maxBytes = input(8 * 1024 * 1024);
  /** When true, the file is emitted only after Upload is clicked. */
  readonly confirm = input(true);
  readonly existing = input<string | null>(null);
  readonly chosen = output<File>();

  readonly over = signal(false);
  readonly message = signal('');
  readonly pending = signal<File | null>(null);
  readonly preview = signal<string | null>(null);

  shown(): string | null {
    return this.preview() || this.existing();
  }

  onDrag(event: DragEvent, over: boolean): void {
    event.preventDefault();
    this.over.set(over);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.over.set(false);
    const file = event.dataTransfer?.files?.[0];
    if (file) this.take(file);
  }

  onPick(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (file) this.take(file);
  }

  take(file: File): void {
    const allowed = this.accept().split(',').map((type) => type.trim());
    if (file.type && !allowed.includes(file.type)) {
      this.message.set('Use a JPEG, PNG or WebP image.');
      return;
    }
    if (file.size > this.maxBytes()) {
      this.message.set('Image must be 8 MB or smaller.');
      return;
    }
    this.message.set('');
    this.replacePreview(URL.createObjectURL(file));
    if (this.confirm()) this.pending.set(file);
    else this.chosen.emit(file);
  }

  upload(): void {
    const file = this.pending();
    if (!file) return;
    this.pending.set(null);
    this.chosen.emit(file);
  }

  cancel(): void {
    this.pending.set(null);
    this.replacePreview(null);
  }

  private replacePreview(next: string | null): void {
    const current = this.preview();
    if (current?.startsWith('blob:')) URL.revokeObjectURL(current);
    this.preview.set(next);
  }
}
