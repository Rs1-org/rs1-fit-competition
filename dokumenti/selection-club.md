# COMP-36: Selection → Club

Selections are linked to clubs with `Selection.ClubId` (an internal GUID). API
create/update requests use **`clubPublicId`**, the club's 32-character public ID.
Both commands validate its format and active-club existence through FluentValidation
and `IPublicIdLookup`. Existing role restrictions remain: Admin/Moderator create
and edit; Admin deletes. Cancellation flows through MediatR to persistence.

Selection list, detail and write responses add nullable `clubPublicId`, `clubName`
and `clubSlug`. Internal `ClubId` is not exposed. A deleted club's identity is
omitted, while its selection remains readable and can be reassigned.

`GET /api/selection?clubPublicId=<public-id>&page=1&pageSize=10` filters in SQL
before counting/paging, combined with existing filters. Active selections are
ordered by name, then public ID. Filtering by an absent/deleted club returns an
empty page; a malformed public ID returns 400.

## UI

- `/manage-selections` and `/manage-selections/:slug` require an active club in
  the Reactive Form. The picker loads every server page, supports retry and uses
  public IDs. Save prevents duplicate submission and retains input on failure.
- `/selection/:slug` is a public lazy route showing a link to the assigned club.
  The selection list links to details and offers a club filter.
- `/clubs/:slug` lists its selections with server paging and links to details.
  Loading, empty and failed requests have distinct states.
- Legacy selections display “Club not assigned” until an authorized user assigns
  a club. Deleted clubs must be replaced on the next edit.

## Migration and compatibility

`20261018122224_LinkSelectionToClub` adds a nullable UUID column, index and
`Restrict` foreign key. Nullability preserves existing selections without inventing
club memberships; every create/update request requires a club. Older clients that
omit `clubPublicId` will receive validation errors on writes.

The Jira ticket originally requested COMP-117 → COMP-75 → COMP-36 migration
order. At implementation time COMP-117 and COMP-13 were in `main`, while COMP-75
was still absent. Following the owner's direction to choose the best approach,
this additive migration is based on that actual schema. Whichever PR integrates
second must reconcile the EF snapshot with COMP-75's `HomeStadiumId` changes and
verify the combined migrations. This PR does not implement COMP-75.

## Verification

Backend tests cover public-ID resolution, reassignment, club validation, soft
deletion, paging, combined filters, API role enforcement and DTO contracts.
Frontend tests cover picker validation, request payloads, lookup paging/retry,
duplicate saves, failed saves, navigation, filter resets and selection paging.

Relational verification uses a disposable PostgreSQL database: migrate through
`AddClubs`, insert a legacy selection, apply COMP-36, check the legacy row, FK and
restrict behavior, then query the actual API for paging, filters and soft deletion.
The diagram source and PNG include the relationship.

Browser inspection and a UI screenshot remain pending when no browser surface is
available. Unit tests and production builds do not establish visual correctness.
