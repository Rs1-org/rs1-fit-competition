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
  positionId?: string | null;
  subPositionId?: string | null;
  positionName?: string | null;
  subPositionName?: string | null;
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

export function pictureToDataUrl(
  picture: string | number[] | null | undefined,
): string | null {
  if (!picture) return null;

  if (typeof picture === 'string') {
    if (!picture.trim()) return null;
    if (picture.startsWith('data:')) return picture;
    return `data:image/jpeg;base64,${picture}`;
  }

  if (Array.isArray(picture) && picture.length > 0) {
    const binary = picture.map((byte) => String.fromCharCode(byte)).join('');
    return `data:image/jpeg;base64,${btoa(binary)}`;
  }

  return null;
}

export function formatPictureForView(picture: string | null | undefined): string {
  return picture ? 'Slika postavljena' : 'Nema slike';
}
