import { FormControl, FormGroup } from '@angular/forms';
import {
  clubDateRangeValidator,
  clubNameValidator,
  foundingDateValidator,
} from './club-validation';

describe('Club validation', () => {
  it('rejects whitespace, punctuation and future or impossible founding dates', () => {
    expect(clubNameValidator(new FormControl('  '))).toBeTruthy();
    expect(clubNameValidator(new FormControl('!!'))).toBeTruthy();
    expect(clubNameValidator(new FormControl('Željezničar'))).toBeNull();
    expect(foundingDateValidator(new FormControl('2999-01-01'))).toBeTruthy();
    expect(foundingDateValidator(new FormControl('2025-02-30'))).toBeTruthy();
    expect(foundingDateValidator(new FormControl('0000-01-01'))).toBeTruthy();
    expect(foundingDateValidator(new FormControl('1921-09-19'))).toBeNull();
    expect(foundingDateValidator(new FormControl(''))).toBeNull();
  });

  it('rejects a reversed founding-date range and accepts open ranges', () => {
    const group = new FormGroup({
      foundedFrom: new FormControl('2000-01-01'),
      foundedTo: new FormControl('1990-01-01'),
    });
    expect(clubDateRangeValidator(group)).toEqual({ dateRange: true });
    group.controls.foundedTo.setValue('');
    expect(clubDateRangeValidator(group)).toBeNull();
  });
});
