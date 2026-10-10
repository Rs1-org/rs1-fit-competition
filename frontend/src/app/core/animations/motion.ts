import { animate, query, stagger, style, transition, trigger } from '@angular/animations';

const ease = '240ms cubic-bezier(0.2, 0.8, 0.2, 1)';

export const routeAnimations = trigger('routeAnimations', [
  transition('* => *', [
    query(':enter', [
      style({ opacity: 0, transform: 'translateY(12px)' }),
      animate(ease, style({ opacity: 1, transform: 'none' })),
    ], { optional: true }),
  ]),
]);

export const listStagger = trigger('listStagger', [
  transition('* => *', [
    query(':enter', [
      style({ opacity: 0, transform: 'translateY(10px)' }),
      stagger(40, animate(ease, style({ opacity: 1, transform: 'none' }))),
    ], { optional: true }),
    query(':leave', [
      animate('140ms ease', style({ opacity: 0 })),
    ], { optional: true }),
  ]),
]);

export const fadeIn = trigger('fadeIn', [
  transition(':enter', [
    style({ opacity: 0 }),
    animate('200ms ease', style({ opacity: 1 })),
  ]),
  transition(':leave', [
    animate('150ms ease', style({ opacity: 0 })),
  ]),
]);

export const popIn = trigger('popIn', [
  transition(':enter', [
    style({ opacity: 0, transform: 'translateY(12px) scale(0.98)' }),
    animate(ease, style({ opacity: 1, transform: 'none' })),
  ]),
  transition(':leave', [
    animate('160ms ease', style({ opacity: 0, transform: 'translateY(8px)' })),
  ]),
]);

export const fadeSwap = trigger('fadeSwap', [
  transition('* => *', [
    style({ opacity: 0, transform: 'translateY(8px)' }),
    animate('200ms ease-out', style({ opacity: 1, transform: 'none' })),
  ]),
]);
