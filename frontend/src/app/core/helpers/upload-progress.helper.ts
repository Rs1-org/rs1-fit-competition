import { HttpEvent, HttpEventType, HttpResponse } from '@angular/common/http';
import { filter, map, Observable, tap } from 'rxjs';

export function trackUpload<T>(
  events: Observable<HttpEvent<T>>,
  onProgress: (percent: number) => void,
): Observable<T> {
  return events.pipe(
    tap((event) => {
      if (event.type !== HttpEventType.UploadProgress) return;
      const total = event.total ?? 0;
      onProgress(total ? Math.min(100, Math.round((100 * event.loaded) / total)) : 0);
    }),
    filter((event): event is HttpResponse<T> => event.type === HttpEventType.Response),
    map((event) => event.body as T),
  );
}
