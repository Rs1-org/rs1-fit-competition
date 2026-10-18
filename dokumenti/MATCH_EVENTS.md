# Match events (COMP-41)

Public `/match-events` lists recorded events with server paging. Match, event type,
team, player and base-minute range are frontend filters that drive the API query.
Choose a match to load its team/player choices; the match picker itself is paged.
`/match-events/:publicId` provides match context and a timeline grouped by match
part, ordered by base minute, added time and stable identifiers. Pagination applies
to events, so a group's remaining events can continue on the next page.

Admin and Moderator can use `/manage-events/new` or `/manage-events/:id`. The API
enforces those roles on **all three** write operations, including deletion, as the
ticket explicitly requires moderator deletion. Anonymous users can read events.
The form retains drafts after validation/save errors and blocks duplicate saves.

## Contract and rules

- `GET /api/events`: `PagedResult<EventDto>`; optional `matchPublicId`,
  `eventTypePublicId`, `selectionPublicId`, `playerPublicId`, `minuteFrom`,
  `minuteTo`; `page` defaults to 1 and `pageSize` to 20 (maximum 100).
- `GET /api/events/{publicId}`, `GET /api/events/lookups`,
  `GET /api/events/match/{matchPublicId}/context`.
- `POST /api/events`, `PUT /api/events/{publicId}`, `DELETE /api/events/{publicId}`.
- Bodies use `matchPublicId`, `eventTypePublicId`, `matchPartPublicId`,
  `selectionPublicId`, nullable `playerPublicId` and `relatedPlayerPublicId`,
  integer `minute` (0–130), integer `addedMinute` (0–30), optional `note` (500 chars).
  Omitted players must be null, not empty identifiers. All external IDs are public
  32-character lowercase hex IDs, never internal foreign-key GUIDs.
- An event cannot move between matches on edit. Its team must be active and be
  one of the match's selections. Supplied players must be active and have an active
  membership record for that selection. The existing history model has no validity
  dates, so this establishes recorded membership, **not** a historical lineup.
- A substitution requires two different players: `playerPublicId` is player off,
  `relatedPlayerPublicId` is player on. Other event types allow an unknown player
  and reject a second player. Own-goal team is the actor's team; this feature does
  not infer or update match scores or statistics from events.
- Match parts determine grouping. Minute validation uses the broad match bounds
  above, without inventing a competition-specific clock rule for penalties or
  extra time. Range filters use the base minute, excluding added time.
- CQRS validation and business rules precede one unit-of-work save; cancellation
  propagates through EF. Deletion preserves rows. Hidden matches, selections,
  types and parts hide their events. FKs restrict physical deletion.

## Reference data and migration

`DataSeeder.SeedEventLookupsAsync` runs independently of optional demo accounts.
It inserts missing active codes without changing existing names or public IDs.
Seven types: Goal, Own goal, Yellow card, Red card, Substitution, Penalty scored,
Penalty missed. Four parts: 1st half, 2nd half, Extra time, Penalties.

`AddMatchEvents` creates three new tables, their indexes, restrictive FKs and
minute/different-player checks. Existing Match/Selection schemas and data remain
unchanged. Apply using the established startup migration process or `dotnet ef
database update --project Competition.Infrastructure --startup-project Competition.Api`
against the intended DEV database; keep its connection string in private configuration.

## Integration and verification limits

COMP-75 still owns MatchStatus seed/lookup. COMP-65's full match-detail page is
not present in this checkout. Events work with existing matches and expose a
dedicated match timeline linked from match reviews. Reusable
`MatchEventTimeline` accepts event data and permission flags for later embedding
in COMP-65. This does not complete those other tickets or remove their dependency.
Fresh database match creation still needs COMP-75's status seed/lookup flow.

Tests cover API role boundaries, CRUD, all filters/paging, substitutions, malformed
references, minutes, membership, soft deletion and idempotent lookup seeding.
Angular behavior tests cover payload/paging, range validation, form errors and
duplicate saves. Relational migration/query checks use isolated local PostgreSQL,
never Neon or production data. The diagram source is updated; no rendered PNG,
browser screenshots or visual verification are claimed. Jira status, estimate,
comments and screenshot attachments have not been changed.
