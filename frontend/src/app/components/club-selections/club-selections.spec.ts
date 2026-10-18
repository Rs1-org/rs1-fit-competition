import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { registerLocaleData } from '@angular/common';
import bs from '@angular/common/locales/bs-Latn';
import { of, throwError } from 'rxjs';
import { ClubSelections } from './club-selections';
import { SelectionService } from '../../core/services/selection.service';
import { emptyPage } from '../../core/models/paged-result';

describe('ClubSelections', () => {
  const getAll = vi.fn();
  beforeEach(() => {
    registerLocaleData(bs);
    getAll.mockReset().mockReturnValue(
      of({
        ...emptyPage(),
        totalCount: 11,
        totalPages: 2,
        hasNextPage: true,
        items: [{ id: 'selection', name: 'Youth', abbreviation: 'YTH', slug: 'youth' }],
      }),
    );
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: SelectionService, useValue: { getAll } }],
    });
  });

  it('renders selection links and pages on the server using the club public ID', async () => {
    const fixture = TestBed.createComponent(ClubSelections);
    fixture.componentRef.setInput('clubPublicId', 'a'.repeat(32));
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('a').getAttribute('href')).toBe('/selection/youth');
    fixture.componentInstance.page(2);
    expect(getAll).toHaveBeenLastCalledWith(2, 10, { clubPublicId: 'a'.repeat(32) });
    fixture.componentRef.setInput('clubPublicId', 'b'.repeat(32));
    fixture.detectChanges();
    await fixture.whenStable();
    expect(getAll).toHaveBeenLastCalledWith(1, 10, { clubPublicId: 'b'.repeat(32) });
  });

  it('keeps a failed request distinct from an empty list and allows retry', async () => {
    getAll.mockReturnValueOnce(throwError(() => new Error('offline')));
    const fixture = TestBed.createComponent(ClubSelections);
    fixture.componentRef.setInput('clubPublicId', 'a'.repeat(32));
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alert"]')).toBeTruthy();
    expect(fixture.nativeElement.textContent).not.toContain('no selections yet');
    getAll.mockReturnValue(of(emptyPage()));
    fixture.componentInstance.retry();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('no selections yet');
  });
});
