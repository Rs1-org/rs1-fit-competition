import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-skeleton',
  imports: [],
  templateUrl: './skeleton.html',
  styleUrl: './skeleton.css',
})
export class Skeleton {
  rows = input<number>(3);
  avatar = input<boolean>(false);

  rowList = computed(() => Array.from({ length: this.rows() }, (_, index) => index));
}
