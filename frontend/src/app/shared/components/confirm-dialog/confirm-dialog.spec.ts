import { TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { ConfirmDialog } from './confirm-dialog';
import { ConfirmDialogService, deleteConfirm } from './confirm-dialog.service';

describe('ConfirmDialog', () => {
  beforeEach(() => TestBed.configureTestingModule({ providers: [provideNoopAnimations()] }));

  it('words every delete the same way', () => {
    expect(deleteConfirm('FK Sarajevo')).toEqual({
      title: 'Delete FK Sarajevo?',
      message: 'This cannot be undone.',
      confirmLabel: 'Delete',
      destructive: true,
    });
    expect(deleteConfirm('FK Sarajevo', 'This removes its 4 selections.').message).toBe('This removes its 4 selections. This cannot be undone.');
  });

  it('uses red only for destructive actions and resolves with the choice', async () => {
    const fixture = TestBed.createComponent(ConfirmDialog);
    const service = TestBed.inject(ConfirmDialogService);

    const save = service.confirm({ message: 'Save changes?' });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.btn-danger')).toBeNull();
    expect(fixture.nativeElement.querySelector('.btn-primary')).not.toBeNull();
    fixture.nativeElement.querySelector('.btn-primary').click();
    await expect(save).resolves.toBe(true);

    const remove = service.confirm(deleteConfirm('FK Sarajevo'));
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.confirm-icon')).not.toBeNull();
    fixture.nativeElement.querySelector('.btn-secondary').click();
    await expect(remove).resolves.toBe(false);
  });
});
