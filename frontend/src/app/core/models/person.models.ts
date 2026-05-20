export interface PersonFields {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  picture: string | null;
}

export interface Player extends PersonFields {
  id?: string;
  height: number;
  shirtNumber: number;
  transferValue: number;
}

export interface Referee extends PersonFields {
  id?: string;
  careerStart: string | null;
}

export interface Manager extends PersonFields {
  id?: string;
  careerStart: string | null;
}

export function emptyPersonFields(): PersonFields {
  return {
    firstName: '',
    lastName: '',
    dateOfBirth: '',
    picture: null,
  };
}

export function formatPictureForView(picture: string | null | undefined): string {
  return picture ? 'Slika postavljena' : 'Nema slike';
}
