import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Modal } from '../modal/modal';
import { ModalState } from '../../../core/helpers/modal-state.helper';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [Modal],
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.css',
})
export class ConfirmDialog {
  @Input() title = 'Potvrda';
  @Input() message = 'Jeste li sigurni?';
  @Input() confirmLabel = 'Potvrdi';
  @Input() cancelLabel  = 'Odustani';

  @Output() confirmed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  state = new ModalState();

  open()  { this.state.open(); }

  confirm() {
    this.state.close();
    this.confirmed.emit();
  }

  cancel() {
    this.state.close();
    this.cancelled.emit();
  }
}
