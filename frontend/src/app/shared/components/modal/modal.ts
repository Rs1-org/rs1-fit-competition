import { Component, EventEmitter, Input, Output } from '@angular/core';
import { fadeIn } from '../../../core/animations/motion';

@Component({
  selector: 'app-modal',
  imports: [],
  templateUrl: './modal.html',
  styleUrl: './modal.css',
  animations: [fadeIn],
  host: { '[@fadeIn]': 'in' },
})
export class Modal {
  @Input() title:string='';
  @Input() width:string='500px';
  @Input() height:string='auto';
  @Output() close=new EventEmitter();

}
