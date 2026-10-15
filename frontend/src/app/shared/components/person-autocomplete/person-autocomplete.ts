import { Component, DestroyRef, inject, input, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { catchError, debounceTime, distinctUntilChanged, Observable, of, switchMap } from 'rxjs';
import { AutofocusDirective } from '../../directives/autofocus.directive';

export interface PersonSuggestion {
  id: string;
  label: string;
}

@Component({
  selector: 'app-person-autocomplete',
  imports: [ReactiveFormsModule, AutofocusDirective],
  templateUrl: './person-autocomplete.html',
  styleUrl: './person-autocomplete.css',
})
export class PersonAutocomplete implements OnInit {
  label = input('Name');
  inputId = input('person-search');
  control = input.required<FormControl<string>>();
  search = input.required<(term: string) => Observable<PersonSuggestion[]>>();
  private destroyRef = inject(DestroyRef);

  suggestions = signal<PersonSuggestion[]>([]);
  open = signal(false);
  private skipNext = false;

  ngOnInit(): void {
    this.control().valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap((term) => {
        if (this.skipNext) {
          this.skipNext = false;
          return of([]);
        }
        const value = (term ?? '').trim();
        if (value.length < 2) return of([]);
        return this.search()(value).pipe(catchError(() => of([])));
      }),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe((items) => {
      this.suggestions.set(items);
      this.open.set(items.length > 0);
    });
  }

  pick(item: PersonSuggestion): void {
    this.skipNext = true;
    this.control().setValue(item.label);
    this.suggestions.set([]);
    this.open.set(false);
  }

  close(): void {
    this.open.set(false);
  }
}
