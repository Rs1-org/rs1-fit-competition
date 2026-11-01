import { Component, inject } from '@angular/core';
import { Button } from '../button/button';
import { Icon } from '../icon/icon';
import { Modal } from '../modal/modal';
import { ConfirmDialogService } from './confirm-dialog.service';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [Modal, Button, Icon],
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.css',
})
export class ConfirmDialog {
  svc = inject(ConfirmDialogService);
}
