# Live match polls (COMP-47)

The live match timeline (`/live-matches/:publicId`, also the selected match in
`/match-events`) includes a reusable `MatchPolls` panel. Visitors see vote counts
and percentages; active signed-in users may submit one final vote per poll.
Admin/Moderator can open `/manage-polls/:matchPublicId` to create a question,
2–8 distinct options and an opening/closing schedule. The management route and
API both enforce those roles. Polls and votes persist in PostgreSQL.

## API and persistence

- `GET /api/polls/match/{matchPublicId}?page=1&pageSize=10` returns
  `PagedResult<PollDto>` (maximum page size 50). Match lookup rejects deleted or
  missing matches. Paging/counting happen in SQL; option counts are aggregated
  there, without loading individual votes.
- `POST /api/polls/match/{matchPublicId}` accepts `question`, `options` (string
  array), `opensAt` and `closesAt` (ISO timestamps with offsets).
- `POST /api/polls/{publicId}/vote` accepts only `optionPublicId`. User identity
  comes from the authenticated principal; supplied user/viewer IDs are ignored.
- DTOs expose public IDs, question/options, counts, percentages, timestamps,
  `isOpen` and the viewer's own option public ID. They expose no voter lists,
  account GUIDs or emails. Anonymous readers receive no personal vote.
- Questions are nonblank and at most 200 characters; option labels at most 100.
  Labels must be distinct after trimming and ignoring case. Closing time must be
  in the future and after opening time. Opening may be in the past to start now.
- The server uses `IClock`: opening is inclusive and closing is exclusive.
  Scheduled/closed polls reject votes. Votes are final; repeat submissions are
  rejected even when they name a different option. Invalid cross-poll options
  cannot be voted for.
- Migration `AddMatchPolls` adds `Poll`, `PollOption` and `PollVote`, all using
  `BaseEntity`. FKs restrict physical deletion. A composite FK binds a vote's
  option to the same poll; an active-row unique `(PollId, UserId)` index also
  prevents simultaneous duplicate votes. A database check enforces the time
  window. Normal duplicate requests return readable 400 errors; a database
  uniqueness race returns 409. No existing migration or database engine changes.
- Soft-deleted polls/matches/options hide their data through query filters.
  Vote records remain stored. No public poll editing/deletion or vote-change
  endpoint is introduced.

## Real-time and form behavior

After a successful create/vote save, the existing match SignalR group receives a
`MatchChanged` notice with `change: "polls"`. It contains no private choice or user
identity. The panel shares `MatchLiveService` with the score header and re-reads
counts and its own viewer state after notices or reconnect. Poll notices do not
refresh unrelated score/event queries. No new hub write method is exposed.

The voting form stays visible and keeps an unsent choice while other fans vote.
Repeated pending submissions are blocked; successful votes lock the user's choice
even while refreshed counts are arriving. Errors retain the selection. A different
signed-in identity clears private choices; refreshing the same identity does not.
The creation Reactive Form mirrors question/option/time rules, retains input after
errors and blocks duplicate saves. Date inputs are interpreted in browser local
time and saved as ISO instants. Counts, percentages and dates use regional pipes.

Poll responses include the server snapshot time. A timer re-reads visible polls at
their next opening/closing boundary relative to that time, avoiding dependence on
the browser's wall clock for status refresh. The API always rechecks its own clock
when accepting a vote. Percentages are rounded to one decimal place; rounding can
make displayed percentages sum to 99.9 or 100.1 for some distributions. Zero votes
produce zero percentages.

## Verification and integration limits

API tests cover roles, once-only voting, public/private viewer projections,
questions/options/windows, paging, wrong-poll choices and deleted matches. Boundary
and post-commit notification rules have isolated tests. Real
WebSocket test-server clients verify that creation/voting notify both spectators.
Local PostgreSQL checks exercise actual CQRS queries/counts and database rejection
of duplicate votes, mismatched options and invalid time windows, with rollback.

Angular tests cover payloads, guarded creation, option/window validation, retained
drafts during live updates, duplicate saves/votes, identity changes, reconnect and
server-relative boundary refresh. Production build validates strict templates.
Browser visual/mobile/theme/keyboard checks and screenshots are unavailable.

The shared match-detail implementation remains COMP-65's responsibility. This
feature is usable on the existing live timeline and its panel can be embedded
there later. No Jira status/estimate/comment changes, RS1 points, screenshots or
authorship claims are made. Free Render/Pages hosting is unchanged; no paid
service, new dependency or keep-awake/background job is introduced.
