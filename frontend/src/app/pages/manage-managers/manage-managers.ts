import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Route, Router } from '@angular/router';

@Component({
  selector: 'app-manage-managers',
  imports: [CommonModule,FormsModule],
  templateUrl: './manage-managers.html',
  styleUrl: './manage-managers.css',
})
export class ManageManagers {
  constructor(private router:Router) {}
  manager = {
    careerStart: ''
  };
  async saveManager() {
    const response = await fetch('https://localhost:7208/api/manager', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(this.manager)
    });
    if(response.ok) {
      const currentUrl = this.router.url;
    this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
      this.router.navigate([currentUrl]);
    });
    };
    
  }
  cancel() {
    this.manager = {
      careerStart: ''
    };
  }
}
