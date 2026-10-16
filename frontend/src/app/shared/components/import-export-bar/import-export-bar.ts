import { Component, ElementRef, EventEmitter, Input, Output, ViewChild, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ExportFormat, ImportResult } from '../../../core/models/import-export';
import { exportFileName, saveBlob } from '../../../core/helpers/download.helper';

/**
 * Export (CSV / Excel / JSON) of the current filtered + sorted list and, for admins, import from a file
 * with the backend's row-level error report (RS1 item 86).
 */
@Component({
  selector: 'app-import-export-bar',
  standalone: true,
  templateUrl: './import-export-bar.html',
})
export class ImportExportBar {
  /** Used for the downloaded file name, e.g. "cities" → cities-2026-10-16.csv. */
  @Input({ required: true }) fileBaseName!: string;
  @Input({ required: true }) exportFn!: (format: ExportFormat) => Observable<Blob>;
  @Input() importFn?: (file: File) => Observable<ImportResult>;
  /** Columns the import expects, shown as a hint next to the button. */
  @Input() importColumns = '';
  @Input() canImport = false;
  @Output() imported = new EventEmitter<number>();

  @ViewChild('fileInput') fileInput?: ElementRef<HTMLInputElement>;

  busy = signal<ExportFormat | 'import' | null>(null);
  result = signal<ImportResult | null>(null);
  error = signal<string | null>(null);

  export(format: ExportFormat): void {
    this.error.set(null);
    this.busy.set(format);
    this.exportFn(format).subscribe({
      next: (blob) => {
        saveBlob(blob, exportFileName(this.fileBaseName, format));
        this.busy.set(null);
      },
      error: () => {
        this.error.set('Export failed. Please try again.');
        this.busy.set(null);
      },
    });
  }

  chooseFile(): void {
    this.fileInput?.nativeElement.click();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file || !this.importFn) return;

    this.error.set(null);
    this.result.set(null);
    this.busy.set('import');
    this.importFn(file).subscribe({
      next: (result) => {
        this.result.set(result);
        this.busy.set(null);
        if (result.success) this.imported.emit(result.imported);
      },
      error: (err: HttpErrorResponse) => {
        // 400 = file rejected by the command validator (type, size, missing file).
        this.error.set(err.status === 400 ? this.validationMessage(err) : 'Import failed. Please try again.');
        this.busy.set(null);
      },
    });
  }

  dismiss(): void {
    this.result.set(null);
    this.error.set(null);
  }

  private validationMessage(err: HttpErrorResponse): string {
    const body = err.error;
    const errors = body?.errors;
    if (Array.isArray(errors) && errors.length) return errors.map((e: any) => e.errorMessage ?? e.message ?? e).join(' ');
    if (errors && typeof errors === 'object') return Object.values(errors).flat().join(' ');
    return body?.message ?? 'The file was rejected.';
  }
}
