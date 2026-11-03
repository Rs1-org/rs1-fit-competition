import { Component, EventEmitter, inject, Input, OnChanges, OnInit, Output, signal, SimpleChanges } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputBox } from '../../shared/components/input-box/input-box';
import { Modal } from '../../shared/components/modal/modal';
import { Button } from '../../shared/components/button/button';
import { Dropdown } from '../../shared/components/dropdown/dropdown';
import { CountryService } from '../../core/services/country-service';
import { CompetitionService } from '../../core/services/competition.service';
import { ToastService } from '../../core/services/toast.service';
import { CompetitionDetailsDto, SaveCompetitionDto } from '../../core/models/competition';
import { showServerErrors } from '../../core/utils/api-error';

@Component({
  selector: 'app-competition-form',
  imports: [InputBox, Modal, ReactiveFormsModule, Button, Dropdown],
  templateUrl: './competition-form.html',
  styleUrl: './competition-form.css',
})
export class CompetitionForm implements OnInit, OnChanges {
  @Input() isOpen = false;
  @Input() editCompetition: CompetitionDetailsDto | null = null;
  @Output() saved = new EventEmitter<void>();
  @Output() closed = new EventEmitter<void>();

  private fb = inject(NonNullableFormBuilder);
  private countryService = inject(CountryService);
  private competitionService = inject(CompetitionService);
  private toast = inject(ToastService);

  countries = signal<{ publicId: string; name: string }[]>([]);
  errorMessage = signal('');
  saving = signal(false);

  form = this.fb.group({
    name: this.fb.control('', { validators: [Validators.required, Validators.minLength(2), Validators.maxLength(100)] }),
    shortName: this.fb.control('', { validators: Validators.maxLength(20) }),
    countryPublicId: this.fb.control(''),
    description: this.fb.control('', { validators: Validators.maxLength(1000) }),
  });

  get isEditMode(): boolean {
    return this.editCompetition !== null;
  }

  ngOnInit(): void {
    this.countryService.getAllForDropdown().subscribe({
      next: (countries) => this.countries.set([{ publicId: '', name: 'No country (international)' }, ...countries]),
      error: () => this.errorMessage.set('Countries could not be loaded. Close the form and try again.'),
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['editCompetition'] || changes['isOpen']) {
      this.errorMessage.set('');
      if (this.editCompetition) {
        this.form.reset({
          name: this.editCompetition.name,
          shortName: this.editCompetition.shortName ?? '',
          countryPublicId: this.editCompetition.countryPublicId ?? '',
          description: this.editCompetition.description ?? '',
        });
      } else {
        this.form.reset();
      }
    }
  }

  onClose(): void {
    this.form.reset();
    this.closed.emit();
  }

  onSubmit(): void {
    if (this.saving()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    const payload: SaveCompetitionDto = {
      name: value.name.trim(),
      shortName: value.shortName.trim() || null,
      description: value.description.trim() || null,
      countryPublicId: value.countryPublicId || null,
    };
    const editing = this.isEditMode;
    const request$ = editing
      ? this.competitionService.update(this.editCompetition!.slug, payload)
      : this.competitionService.create(payload);

    this.saving.set(true);
    this.errorMessage.set('');
    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success(editing ? 'Competition updated' : 'Competition added');
        this.form.reset();
        this.saved.emit();
      },
      error: (err) => {
        this.saving.set(false);
        const message = showServerErrors(this.form, err, 'The competition could not be saved.');
        this.errorMessage.set(message);
        if (message) this.toast.error(message);
      },
    });
  }
}
