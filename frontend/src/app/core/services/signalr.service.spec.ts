import { TestBed } from '@angular/core/testing';
import { Subscription } from 'rxjs';
import { REALTIME_CONNECTION_FACTORY, RealtimeConnection, RealtimeEvent, SignalRService } from './signalr.service';

describe('Reusable SignalR lifecycle', () => {
  const options = { hubPath: '/hubs/test', eventName: 'Changed', joinMethod: 'Join', args: ['public-id'] };
  let callbacks: { data?: (value: unknown) => void; reconnecting?: () => void; reconnected?: () => void; close?: () => void };
  let connection: { start: ReturnType<typeof vi.fn>; stop: ReturnType<typeof vi.fn>; invoke: ReturnType<typeof vi.fn>; off: ReturnType<typeof vi.fn> };
  let service: SignalRService, subscription: Subscription | undefined;
  let received: RealtimeEvent<{ score: number }>[];
  const settle = async () => { await Promise.resolve(); await Promise.resolve(); await Promise.resolve(); };
  beforeEach(() => {
    vi.useFakeTimers(); callbacks = {}; received = []; subscription = undefined;
    connection = { start: vi.fn().mockResolvedValue(undefined), stop: vi.fn().mockResolvedValue(undefined), invoke: vi.fn().mockResolvedValue(undefined), off: vi.fn() };
    const transport = { ...connection,
      on: (_: string, callback: (value: unknown) => void) => { callbacks.data = callback; },
      onreconnecting: (callback: () => void) => { callbacks.reconnecting = callback; },
      onreconnected: (callback: () => void) => { callbacks.reconnected = callback; },
      onclose: (callback: () => void) => { callbacks.close = callback; },
    } as unknown as RealtimeConnection;
    TestBed.configureTestingModule({ providers: [{ provide: REALTIME_CONNECTION_FACTORY, useValue: () => transport }] });
    service = TestBed.inject(SignalRService);
  });
  afterEach(() => { subscription?.unsubscribe(); vi.useRealTimers(); });
  it('joins after connecting and rejoins after a dropped connection', async () => {
    subscription = service.watch<{ score: number }>(options).subscribe(value => received.push(value)); await settle();
    expect(connection.invoke).toHaveBeenCalledExactlyOnceWith('Join', 'public-id');
    callbacks.data?.({ score: 2 }); expect(received.at(-1)).toEqual({ type: 'data', data: { score: 2 } });
    callbacks.reconnecting?.(); expect(received.at(-1)).toEqual({ type: 'state', state: 'reconnecting' });
    callbacks.reconnected?.(); await settle(); expect(connection.invoke).toHaveBeenCalledTimes(2);
    expect(received.at(-1)).toEqual({ type: 'state', state: 'connected' });
  });
  it('retries initial failures, which automatic reconnect does not cover', async () => {
    connection.start.mockRejectedValueOnce(new Error('Cold start'));
    subscription = service.watch<{ score: number }>(options).subscribe(value => received.push(value)); await settle();
    expect(received.at(-1)).toEqual({ type: 'state', state: 'reconnecting' });
    await vi.advanceTimersByTimeAsync(1000); await settle(); expect(connection.start).toHaveBeenCalledTimes(2);
    expect(received.at(-1)).toEqual({ type: 'state', state: 'connected' });
  });
  it('clears retry timers and closes transport when the consumer leaves', async () => {
    connection.start.mockRejectedValueOnce(new Error('Offline'));
    subscription = service.watch<{ score: number }>(options).subscribe(); await settle(); subscription.unsubscribe();
    await vi.advanceTimersByTimeAsync(60000); expect(connection.start).toHaveBeenCalledOnce();
    expect(connection.stop).toHaveBeenCalled(); expect(connection.off).toHaveBeenCalled();
  });
  it('closes a late initial connection after unsubscribe without joining an abandoned page', async () => {
    let resolve: () => void = () => undefined;
    connection.start.mockReturnValue(new Promise<void>(done => { resolve = done; }));
    subscription = service.watch<{ score: number }>(options).subscribe(); subscription.unsubscribe(); resolve(); await settle();
    expect(connection.invoke).not.toHaveBeenCalled(); expect(connection.stop).toHaveBeenCalled();
  });
  it('reports an invalid subscription without endlessly retrying rejected group membership', async () => {
    connection.invoke.mockRejectedValueOnce(new Error('Match not found'));
    subscription = service.watch<{ score: number }>(options).subscribe(value => received.push(value)); await settle();
    expect(received.at(-1)).toEqual({ type: 'state', state: 'unavailable' }); expect(subscription.closed).toBe(true);
    await vi.advanceTimersByTimeAsync(60000); expect(connection.start).toHaveBeenCalledOnce();
  });
});
