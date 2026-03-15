import { signal } from '@angular/core';
import { FormGroup } from '@angular/forms';

export class ModalState {
  _isOpen = signal(false);

  open() {
    this._isOpen.set(true);
  }

  close(form?: FormGroup) {
    this._isOpen.set(false);
    if (form) {
      form.reset();
      form.markAsUntouched();
      form.markAsPristine();
    }
  }
  isOpen():boolean{
    return this._isOpen();
  }
}