import { Injectable, signal } from '@angular/core';

export interface ConfirmOptions {
  message: string;
  title?: string;
  confirmLabel?: string;
  cancelLabel?: string;
  destructive?: boolean;
}

export function deleteConfirm(name: string, detail?: string): ConfirmOptions {
  return {
    title: `Delete ${name}?`,
    message: detail ? `${detail} This cannot be undone.` : 'This cannot be undone.',
    confirmLabel: 'Delete',
    destructive: true,
  };
}

@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  private _options = signal<ConfirmOptions | null>(null);
  private _resolve?: (value: boolean) => void;

  readonly options = this._options.asReadonly();

  confirm(options: ConfirmOptions): Promise<boolean> {
    this._options.set(options);
    return new Promise<boolean>(resolve => {
      this._resolve = resolve;
    });
  }

  accept() {
    this._options.set(null);
    this._resolve?.(true);
  }

  dismiss() {
    this._options.set(null);
    this._resolve?.(false);
  }
}
