import { Injectable, computed, signal } from '@angular/core';

export interface TourStep {
  /** Matches the `data-tour` attribute of the element the step points at. */
  target: string;
  title: string;
  text: string;
}

export const TOUR_STEPS: readonly TourStep[] = [
  { target: 'live', title: 'Live', text: 'Matches being played right now, with the score updating as it happens.' },
  { target: 'matches', title: 'Matches', text: 'All fixtures and results. Filter by status, competition, stadium or date.' },
  { target: 'competitions', title: 'Competitions', text: 'Leagues with their seasons, rounds and the league table.' },
  { target: 'statistics', title: 'Statistics', text: 'Team statistics, charts for the season and premium advanced statistics.' },
  { target: 'account', title: 'Your account', text: 'Log in here. Once you are in, this menu holds your profile, settings and log out.' },
];

const STATUS_KEY = 'tour_status';
const STEP_KEY = 'tour_step';
const DONE = 'done';

/** First-visit tour of the main sections. Progress lives in localStorage, so it survives a refresh and is shown once. */
@Injectable({ providedIn: 'root' })
export class TourService {
  readonly steps = TOUR_STEPS;
  readonly active = signal(false);
  readonly index = signal(0);
  readonly step = computed(() => this.steps[this.index()]);
  readonly isFirst = computed(() => this.index() === 0);
  readonly isLast = computed(() => this.index() === this.steps.length - 1);

  /** Starts the tour unless it was finished or skipped before; resumes the saved step after a refresh. */
  startIfFirstVisit(): void {
    if (read(STATUS_KEY) === DONE) return;
    const saved = Number(read(STEP_KEY));
    this.open(Number.isInteger(saved) && saved > 0 && saved < this.steps.length ? saved : 0);
  }

  /** Starts again from the first step, even if the tour was finished (Settings). */
  restart(): void {
    remove(STATUS_KEY);
    this.open(0);
  }

  next(): void {
    if (this.isLast()) {
      this.close();
      return;
    }
    this.open(this.index() + 1);
  }

  back(): void {
    if (!this.isFirst()) this.open(this.index() - 1);
  }

  /** Skipping counts as seen, so the tour does not come back. */
  skip(): void {
    this.close();
  }

  private open(index: number): void {
    this.index.set(index);
    this.active.set(true);
    write(STEP_KEY, String(index));
  }

  private close(): void {
    this.active.set(false);
    this.index.set(0);
    write(STATUS_KEY, DONE);
    remove(STEP_KEY);
  }
}

// Storage can be blocked (private mode, disabled site data); the tour then just runs without memory.
function read(key: string): string | null {
  try { return localStorage.getItem(key); } catch { return null; }
}

function write(key: string, value: string): void {
  try { localStorage.setItem(key, value); } catch { /* not persisted */ }
}

function remove(key: string): void {
  try { localStorage.removeItem(key); } catch { /* not persisted */ }
}
