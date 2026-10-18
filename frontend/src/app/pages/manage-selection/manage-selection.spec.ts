import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap } from '@angular/router';
import { BehaviorSubject, of, Subject, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { ManageSelection } from './manage-selection';
import { SelectionService } from '../../core/services/selection.service';
import { ClubService } from '../../core/services/club.service';
import { CountryService } from '../../core/services/country-service';
import { emptyPage } from '../../core/models/paged-result';

describe('ManageSelection club picker', () => {
  const clubId = 'a'.repeat(32);
  const create = vi.fn();
  const update = vi.fn();
  const getClubs = vi.fn();
  const navigate = vi.fn();
  const getBySlug = vi.fn();
  let params$ = new BehaviorSubject(convertToParamMap({}));
  function setup(slug: string | null = null) {
    params$.next(convertToParamMap({ slug }));
    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: { navigate } },
        { provide: ActivatedRoute, useValue: { paramMap: params$ } },
        {
          provide: SelectionService,
          useValue: {
            create,
            update,
            getBySlug,
          },
        },
        { provide: ClubService, useValue: { getAll: getClubs } },
        { provide: CountryService, useValue: { getAll: () => of(emptyPage()) } },
      ],
    });
    const component = TestBed.runInInjectionContext(() => new ManageSelection());
    component.ngOnInit();
    return component;
  }
  beforeEach(() => {
    vi.clearAllMocks();
    params$ = new BehaviorSubject(convertToParamMap({}));
    getBySlug.mockReset().mockReturnValue(
      of({
        name: 'Youth',
        abbreviation: 'YTH',
        followersCount: 0,
        countryPublicId: 'b'.repeat(32),
        clubPublicId: clubId,
      }),
    );
    getClubs.mockReturnValue(of({ ...emptyPage(), items: [{ publicId: clubId, name: 'Club' }] }));
  });

  it('cancels stale selection reads when navigating to another edit route', () => {
    const stale = new Subject();
    getBySlug.mockReturnValueOnce(stale);
    const component = setup('old');
    expect(stale.observed).toBe(true);
    params$.next(convertToParamMap({ slug: 'youth' }));
    expect(stale.observed).toBe(false);
    expect(component.form.controls.clubPublicId.value).toBe(clubId);
    expect(component.loading).toBe(false);
  });

  it('shows server club validation at the picker and allows choosing another club', () => {
    update.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 400,
            error: {
              errors: [{ propertyName: 'ClubPublicId', errorMessage: 'Club no longer exists.' }],
            },
          }),
      ),
    );
    const component = setup('youth');
    component.save();
    expect(component.fieldError('clubPublicId')).toBe('Club no longer exists.');
    component.form.controls.clubPublicId.setValue('c'.repeat(32));
    expect(component.form.controls.clubPublicId.valid).toBe(true);
  });

  it('requires a club and submits its public ID while blocking duplicate saves', () => {
    const pending = new Subject();
    create.mockReturnValue(pending);
    const component = setup();
    component.form.patchValue({
      name: 'Youth',
      abbreviation: 'YTH',
      countryPublicId: 'b'.repeat(32),
    });
    component.save();
    expect(create).not.toHaveBeenCalled();
    expect(component.fieldError('clubPublicId')).toBeTruthy();
    component.form.controls.clubPublicId.setValue(clubId);
    component.save();
    component.save();
    expect(create).toHaveBeenCalledTimes(1);
    expect(create).toHaveBeenCalledWith(expect.objectContaining({ clubPublicId: clubId }));
    pending.next({});
    pending.complete();
    expect(navigate).toHaveBeenCalledWith(['/selection-list']);
  });

  it('loads the saved club in edit mode and retains values after an API failure', () => {
    update.mockReturnValue(throwError(() => new Error('offline')));
    const component = setup('youth');
    expect(component.form.controls.clubPublicId.value).toBe(clubId);
    component.save();
    expect(update).toHaveBeenCalledWith('youth', expect.objectContaining({ clubPublicId: clubId }));
    expect(component.form.controls.name.value).toBe('Youth');
    expect(component.error).toBeTruthy();
    expect(component.saving).toBe(false);
  });

  it('loads all club pages and can retry a failed lookup', () => {
    getClubs.mockReturnValueOnce(throwError(() => new Error('offline')));
    const component = setup();
    expect(component.clubsError).toBeTruthy();
    getClubs.mockReturnValueOnce(
      of({
        ...emptyPage(),
        page: 1,
        hasNextPage: true,
        items: [{ publicId: clubId, name: 'First' }],
      }),
    );
    getClubs.mockReturnValueOnce(
      of({ ...emptyPage(), page: 2, items: [{ publicId: 'c'.repeat(32), name: 'Last' }] }),
    );
    component.loadClubs();
    expect(component.clubOptions).toHaveLength(2);
    expect(getClubs).toHaveBeenLastCalledWith(2, 100);
    expect(component.clubsError).toBe('');
  });
});
