import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { registerLocaleData } from '@angular/common';
import bs from '@angular/common/locales/bs-Latn';
import { of } from 'rxjs';
import { SelectionDetail } from './selection-detail';
import { SelectionService } from '../../core/services/selection.service';
import { UserStateService } from '../../core/services/user-state.service';

describe('SelectionDetail club navigation', () => {
  it('links to the club and hides edit for an anonymous viewer', () => {
    registerLocaleData(bs);
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { paramMap: of(convertToParamMap({ slug: 'youth' })) },
        },
        {
          provide: SelectionService,
          useValue: {
            getBySlug: () =>
              of({
                name: 'Youth',
                abbreviation: 'YTH',
                followersCount: 1234,
                clubName: 'Club',
                clubSlug: 'club',
                stadiumName: 'Kosevo',
                stadiumSlug: 'kosevo',
              }),
          },
        },
        { provide: UserStateService, useValue: { canManagePersons: () => false } },
      ],
    });
    const fixture = TestBed.createComponent(SelectionDetail);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('a[href="/clubs/club"]').textContent).toContain(
      'Club',
    );
    expect(
      fixture.nativeElement.querySelector('a[href="/stadium/kosevo"]').textContent,
    ).toContain('Kosevo');
    expect(fixture.nativeElement.textContent).not.toContain('Edit selection');
  });
});
