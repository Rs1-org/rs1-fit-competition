import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';

import { ImportExportBar } from './import-export-bar';
import { ImportResult } from '../../../core/models/import-export';

describe('ImportExportBar', () => {
  let fixture: ComponentFixture<ImportExportBar>;
  let component: ImportExportBar;

  const failed: ImportResult = {
    imported: 0,
    totalRows: 3,
    success: false,
    errors: [
      { row: 3, column: 'Country', message: "Country 'Croatia' does not exist." },
      { row: 4, column: 'Population', message: "'many' is not a whole number." },
    ],
  };

  async function setup(importResult: ImportResult | HttpErrorResponse): Promise<void> {
    await TestBed.configureTestingModule({ imports: [ImportExportBar] }).compileComponents();
    fixture = TestBed.createComponent(ImportExportBar);
    component = fixture.componentInstance;
    component.fileBaseName = 'cities';
    component.exportFn = () => of(new Blob(['Name\r\n']));
    component.importFn = () => (importResult instanceof HttpErrorResponse ? throwError(() => importResult) : of(importResult));
    component.canImport = true;
    fixture.detectChanges();
  }

  function selectFile(): void {
    const input = fixture.nativeElement.querySelector('input[type=file]') as HTMLInputElement;
    Object.defineProperty(input, 'files', { value: [new File(['x'], 'cities.csv')] });
    input.dispatchEvent(new Event('change'));
    fixture.detectChanges();
  }

  it('shows the import button only when import is allowed', async () => {
    await setup(failed);
    expect(fixture.nativeElement.textContent).toContain('Import');

    component.canImport = false;
    fixture.componentRef.changeDetectorRef.markForCheck();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('input[type=file]')).toBeNull();
  });

  it('lists every row error and does not report an import when the file is rejected', async () => {
    await setup(failed);
    let emitted: number | undefined;
    component.imported.subscribe((n) => (emitted = n));

    selectFile();

    const rows = fixture.nativeElement.querySelectorAll('.io-errors tbody tr');
    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('3');
    expect(rows[0].textContent).toContain('Country');
    expect(fixture.nativeElement.textContent).toContain('Nothing was imported.');
    expect(emitted).toBeUndefined();
  });

  it('reports the count and notifies the list after a successful import', async () => {
    await setup({ imported: 2, totalRows: 2, success: true, errors: [] });
    let emitted: number | undefined;
    component.imported.subscribe((n) => (emitted = n));

    selectFile();

    expect(fixture.nativeElement.textContent).toContain('Imported 2 of 2 rows.');
    expect(emitted).toBe(2);
  });

  it('shows the validator message when the server rejects the file', async () => {
    await setup(new HttpErrorResponse({
      status: 400,
      error: { message: 'Validation failed', errors: [{ propertyName: 'File', errorMessage: 'Only .csv and .xlsx files can be imported.' }] },
    }));

    selectFile();

    expect(fixture.nativeElement.textContent).toContain('Only .csv and .xlsx files can be imported.');
  });
});
