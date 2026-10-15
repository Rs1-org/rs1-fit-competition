import { Directive, inject, Input, OnDestroy, TemplateRef, ViewContainerRef } from '@angular/core';
import { Subscription } from 'rxjs';
import { UserStateService } from '../../core/services/user-state.service';

@Directive({ selector: '[appHasRole]' })
export class HasRoleDirective implements OnDestroy {
  private template = inject(TemplateRef<unknown>);
  private view = inject(ViewContainerRef);
  private userState = inject(UserStateService);
  private roles: string[] = [];
  private visible = false;
  private subscription: Subscription;

  constructor() {
    this.subscription = this.userState.roles$.subscribe(() => this.render());
  }

  @Input() set appHasRole(value: string | string[]) {
    this.roles = Array.isArray(value) ? value : value.split(',').map((role) => role.trim()).filter(Boolean);
    this.render();
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  private render(): void {
    const allowed = this.userState.hasAnyRole(this.roles);
    if (allowed && !this.visible) {
      this.view.createEmbeddedView(this.template);
      this.visible = true;
    } else if (!allowed && this.visible) {
      this.view.clear();
      this.visible = false;
    }
  }
}
