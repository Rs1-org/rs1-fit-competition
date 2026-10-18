# Live match updates (COMP-46)

`/live-matches/:publicId` opens the existing event timeline with a current score
summary and a SignalR subscription. Match-review and match-statistics detail pages
reuse the live summary. No full COMP-65 match management UI is implemented here.
Forms and review drafts stay mounted when the score changes. Event filters and
the current event page are retained when the timeline refreshes. If a deletion
empties that page, the list moves to the last valid page.

## Backend contract

- Public read-only hub: `/hubs/matches`.
- `WatchMatch(matchPublicId)` validates an active match's public ID and joins its
  group. Watching another match leaves the previous group; each connection watches
  at most one match. `UnwatchMatch()` leaves it. There is no client publish or write
  method. Public spectators need no account or token.
- Event create/update/delete and match update/delete handlers publish a MediatR
  `MatchChangedNotification` **after** the unit-of-work save succeeds.
- The API broadcasts `MatchChanged` with `{ matchPublicId, change, changedAt }`.
  `change` is `events`, `match` or `deleted`. Internal GUIDs are never broadcast.
- Notices invalidate client snapshots; REST remains the authoritative source of
  scores, status and paged event data. This also handles edits and deletions without
  trying to reconstruct a paged timeline from deltas.
- Socket delivery has a two-second cancellation budget and cannot fail an already
  committed write. Missed messages are recovered by re-reading after join/rejoin.
  There is no durable event queue or exactly-once delivery claim.
- Existing API write authorization remains in place: event writes require
  Admin/Moderator, match update retains its existing `[Authorize]`, match deletion
  requires Admin. This ticket introduces no hub write capability and does not
  change the separate match feature's role policy.
- Configured CORS origins are retained. Hub requests with a browser `Origin` are
  also explicitly checked, including WebSocket upgrades, because WebSockets are
  not governed by browser CORS. Non-browser clients may read without an Origin.
  Detailed hub errors are disabled and inbound messages are limited to 4 KiB.

## Reusable Angular transport

`SignalRService.watch<T>({ hubPath, eventName, joinMethod, args })` returns a typed
Observable of connection state and data. It registers listeners before starting,
retries initial network failures, reconnects with capped backoff, rejoins after
reconnect, and removes listeners/timers/stops the connection on unsubscribe. A
rejected group subscription becomes an explicit unavailable state.

`MatchLiveService.watch(publicId)` shares a match stream with ref-counted lifetime
across consumers and filters notices for other matches. `MatchLiveSummary` fetches
the initial REST snapshot and fetches again after connection/reconnection or a
notice. It exposes refresh reasons so the event page refreshes its SQL-filtered
list without recreating unrelated forms. Connection loss is visible and labels
the displayed score as potentially out of date. No JWTs or credentials are sent
to this public read-only hub.

The Microsoft client is pinned to `@microsoft/signalr` 10.0.11 in the lockfile.
The server uses the existing ASP.NET Core shared framework. No hosted SignalR,
Redis, persistent disk, paid service or new database migration is required.

## Free DEV deployment

The hub uses the existing `environment.apiUrl` and backend port, so Pages connects
to the HTTPS Render API and negotiates the matching WebSocket URL. Existing SPA
fallback covers the new route. Render supports inbound WebSockets but Free
services can sleep/restart; sockets are not durable state. An inactive or cold
service may take longer to connect. The client handles initial failures and
reconnection; persisted scores/events remain in PostgreSQL. No keep-awake cron
or additional paid resource is introduced. The setup currently targets the one
Render instance; adding multiple instances would require a separate scaling design.

Official references checked during implementation:

- [Microsoft JavaScript client, initial start and reconnect behavior](https://learn.microsoft.com/en-us/aspnet/core/signalr/javascript-client?view=aspnetcore-10.0).
- [Microsoft SignalR security and WebSocket origins](https://learn.microsoft.com/en-us/aspnet/core/signalr/security?view=aspnetcore-10.0).
- [Render WebSocket support](https://render.com/docs/websocket) and [Free service restrictions](https://render.com/docs/free).

## Verification

Integration tests use actual SignalR JSON/WebSocket framing through the ASP.NET
test server. They exercise event CRUD broadcasts, score updates, room replacement,
public-ID validation, absent write methods and allowed/rejected browser origins.
An isolated transport failure test confirms committed writes remain successful.
Angular tests exercise join/rejoin, initial retry, cleanup, late connection disposal,
rejected subscription handling, shared lifetime, latest-snapshot resync and retained
event filters/paging. Production compilation checks the real client dependency.

These checks do not constitute browser visual/mobile/theme/keyboard verification,
which is unavailable in this session. No screenshots, RS1 point totals, Jira status,
estimate or authorship evidence are fabricated.
