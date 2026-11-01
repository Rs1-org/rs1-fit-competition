import { Injectable, signal } from '@angular/core';

export type ToastKind = 'success' | 'error';

export interface Toast {
  id: number;
  kind: ToastKind;
  message: string;
}

const DURATION: Record<ToastKind, number> = { success: 4000, error: 6000 };

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 0;
  private _toasts = signal<Toast[]>([]);

  readonly toasts = this._toasts.asReadonly();

  success(message: string): void { this.show('success', message); }
  error(message: string): void { this.show('error', message); }

  dismiss(id: number): void {
    this._toasts.update(toasts => toasts.filter(toast => toast.id !== id));
  }

  private show(kind: ToastKind, message: string): void {
    const id = this.nextId++;
    this._toasts.update(toasts => [...toasts, { id, kind, message }]);
    setTimeout(() => this.dismiss(id), DURATION[kind]);
  }
}
