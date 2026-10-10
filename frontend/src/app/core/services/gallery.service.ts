import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface GalleryItem {
  id: string;
  name: string;
  kind: string;
  hasPicture: boolean;
  pictureUrl: string | null;
}

export interface GallerySection {
  kind: string;
  title: string;
  withPicture: GalleryItem[];
  withoutPicture: GalleryItem[];
}

@Injectable({ providedIn: 'root' })
export class GalleryService {
  private readonly apiUrl = `${environment.apiUrl}/api/gallery`;
  private http = inject(HttpClient);

  list(): Observable<GallerySection[]> {
    return this.http.get<GallerySection[]>(this.apiUrl);
  }

  image(kind: string, id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${kind}/${id}/image`, { responseType: 'blob' });
  }

  upload(kind: string, id: string, file: File): Observable<HttpEvent<GalleryItem>> {
    const body = new FormData();
    body.append('picture', file);
    return this.http.post<GalleryItem>(`${this.apiUrl}/${kind}/${id}`, body, {
      reportProgress: true,
      observe: 'events',
    });
  }

  remove(kind: string, id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${kind}/${id}`);
  }
}
