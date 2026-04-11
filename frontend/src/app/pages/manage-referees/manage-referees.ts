import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { dateTimestampProvider } from 'rxjs/internal/scheduler/dateTimestampProvider';

@Component({
  selector: 'app-manage-referees',
  imports: [CommonModule,FormsModule],
  templateUrl: './manage-referees.html',
  styleUrl: './manage-referees.css',
})
export class ManageReferees {
  constructor(private router:Router) {}
  referee = {
    careerStart: 0
  };
  async saveReferee() {
    const response = await fetch('https://localhost:7208/api/referee', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(this.referee)
    });
    if(response.ok) {
       const currentUrl = this.router.url;
    this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
      this.router.navigate([currentUrl]);
    });
    }
  }
  cancel() {
    this.referee = {
      careerStart: 0
    };
  }
}
