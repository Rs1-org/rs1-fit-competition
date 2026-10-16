import { ExportFormat } from '../models/import-export';

/** e.g. `cities-2026-10-16.xlsx` (built here, so the API doesn't need to expose Content-Disposition). */
export function exportFileName(baseName: string, format: ExportFormat, now = new Date()): string {
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${baseName}-${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}.${format}`;
}

/** Saves a downloaded blob through a temporary link. */
export function saveBlob(blob: Blob, fileName: string): void {
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = fileName;
  link.click();
  URL.revokeObjectURL(url);
}
