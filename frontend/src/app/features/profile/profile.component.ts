import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { UserStateService } from '../../core/services/user-state.service';
import { AppDatePipe } from '../../shared/pipes/regional-format.pipes';
import { StatTile } from '../../shared/components/stat-tile/stat-tile';

@Component({
  selector: 'app-profile',
  imports: [RouterLink, AppDatePipe, StatTile],
  templateUrl: './profile.component.html',
  styleUrl: '../../pages/player-view/person-profile.css',
})
export class ProfileComponent {
  private userState = inject(UserStateService);
  readonly user = toSignal(this.userState.user$, { initialValue: null });
  readonly roles = toSignal(this.userState.roles$, { initialValue: [] as string[] });
  readonly premium = this.userState.isPremium;

  initials(): string {
    const person = this.user();
    const letters = `${person?.firstName?.[0] ?? ''}${person?.lastName?.[0] ?? ''}`.toUpperCase();
    return letters || '—';
  }
}
