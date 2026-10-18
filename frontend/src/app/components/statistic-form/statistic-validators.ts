import { AbstractControl, FormBuilder, ValidatorFn, Validators } from '@angular/forms';
import { TeamStatisticValues } from '../../core/models/statistic';

export const shotsWithinTotal: ValidatorFn = (control: AbstractControl) => {
  const total: unknown = control.get('totalShots')?.value, target: unknown = control.get('shotsOnTarget')?.value;
  return typeof total === 'number' && typeof target === 'number' && target > total ? { shotsWithinTotal: true } : null;
};
export const possessionTotal: ValidatorFn = (control: AbstractControl) => {
  const home: unknown = control.get('home.possession')?.value, away: unknown = control.get('away.possession')?.value;
  return typeof home === 'number' && typeof away === 'number' && home + away !== 100 ? { possessionTotal: true } : null;
};
export function statisticTeamForm(fb: FormBuilder) {
  const number = (max = 2147483647) => fb.control<number | null>(null, [Validators.required, Validators.min(0), Validators.max(max), Validators.pattern(/^\d+$/)]);
  return fb.group({ totalShots: number(), shotsOnTarget: number(), totalRedCards: number(), totalYellowCards: number(),
    corners: number(), bigChances: number(), possession: number(100) }, { validators: shotsWithinTotal });
}
export function completeTeam(values: { [K in keyof TeamStatisticValues]: number | null }): values is TeamStatisticValues {
  return Object.values(values).every(value => typeof value === 'number' && Number.isInteger(value));
}
