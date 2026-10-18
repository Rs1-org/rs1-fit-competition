import { inject, Injectable, InjectionToken } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export type RealtimeState = 'connecting' | 'connected' | 'reconnecting' | 'unavailable';
export type RealtimeEvent<T> = { type: 'state'; state: RealtimeState } | { type: 'data'; data: T };
export interface RealtimeOptions { hubPath: string; eventName: string; joinMethod: string; args: readonly unknown[]; }
export type RealtimeConnection = Pick<HubConnection, 'start' | 'stop' | 'invoke' | 'on' | 'off' | 'onreconnecting' | 'onreconnected' | 'onclose'>;
export const REALTIME_CONNECTION_FACTORY = new InjectionToken<(hubPath: string) => RealtimeConnection>('Realtime connection factory', {
  providedIn: 'root', factory: () => path => new HubConnectionBuilder()
    .withUrl(`${environment.apiUrl}${path}`, { withCredentials: false })
    .withAutomaticReconnect({ nextRetryDelayInMilliseconds: context => Math.min(30000, 1000 * 2 ** Math.min(context.previousRetryCount, 5)) })
    .configureLogging(LogLevel.None).build(),
});

/** Reusable transport for public read-only hubs; subscriptions own their connection and retry timers. */
@Injectable({ providedIn: 'root' })
export class SignalRService {
  private readonly factory = inject(REALTIME_CONNECTION_FACTORY);
  watch<T>(options: RealtimeOptions): Observable<RealtimeEvent<T>> {
    return new Observable(subscriber => {
      const connection = this.factory(options.hubPath);
      let disposed = false, attempts = 0;
      let retry: ReturnType<typeof setTimeout> | undefined;
      const state = (value: RealtimeState) => { if (!disposed) subscriber.next({ type: 'state', state: value }); };
      const data = (value: T) => { if (!disposed) subscriber.next({ type: 'data', data: value }); };
      const join = async () => {
        try {
          await connection.invoke(options.joinMethod, ...options.args);
          if (!disposed) { attempts = 0; state('connected'); }
        } catch {
          if (!disposed) { state('unavailable'); subscriber.complete(); }
        }
      };
      const schedule = () => {
        if (disposed || retry !== undefined) return;
        state('reconnecting');
        retry = setTimeout(() => { retry = undefined; void start(); }, Math.min(30000, 1000 * 2 ** Math.min(attempts++, 5)));
      };
      const start = async () => {
        if (disposed) return;
        try {
          await connection.start();
          if (disposed) { await connection.stop(); return; }
          await join();
        } catch { schedule(); }
      };
      // Register before starting so no initial server notice is lost.
      connection.on(options.eventName, data);
      connection.onreconnecting(() => state('reconnecting'));
      connection.onreconnected(() => { if (!disposed) void join(); });
      connection.onclose(schedule);
      state('connecting'); void start();
      return () => {
        disposed = true;
        if (retry !== undefined) clearTimeout(retry);
        connection.off(options.eventName, data);
        void connection.stop().catch(() => undefined);
      };
    });
  }
}
