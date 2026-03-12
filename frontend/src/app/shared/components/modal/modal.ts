import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-modal',
  imports: [],
  templateUrl: './modal.html',
  styleUrl: './modal.css',
})
export class Modal {
  @Input() title:string='';
  @Input() width:string='500px';
  @Input() height:string='auto';
  @Output() close=new EventEmitter();


}
