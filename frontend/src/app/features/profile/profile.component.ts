import { Component } from '@angular/core';
import { UserStateService } from 'src/app/core/services/user-state.service';
import { UserDto } from 'src/app/core/models/auth.models';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html'
})
export class ProfileComponent {
    user: UserDto | null = null;
    constructor(private userStateService: UserStateService){}
    ngOnInit(){
        this.userStateService.user$.subscribe((userData)=>{
            this.user = userData
        })
    }
}