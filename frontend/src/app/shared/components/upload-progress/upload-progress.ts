import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-upload-progress',
  templateUrl: './upload-progress.html',
  styleUrl: './upload-progress.css',
})
export class UploadProgress {
  @Input() percent: number | null = null;
}
