# Clubs (COMP-13 / COMP-14)

The club feature provides public browsing and role-protected management. Linking
selections to clubs is tracked separately in [COMP-36](https://rs1-2025-26-live-competiton.atlassian.net/browse/COMP-36).

## API

| Method | Path | Access |
| --- | --- | --- |
| GET | `/api/club` (also `/api/club/getAll`) | Public |
| GET | `/api/club/get/{slug}` | Public |
| POST | `/api/club/create` | Admin, Moderator |
| PUT | `/api/club/update/{slug}` | Admin, Moderator |
| DELETE | `/api/club/delete/{publicId}` | Admin |

The list returns the existing `PagedResult<ClubListDto>` contract. Filters are
applied before counting and paging; pages have a stable public-ID tie-breaker.

| Query parameter | Meaning |
| --- | --- |
| `page`, `pageSize` | Page starts at 1; page size is 1–100 (default 10) |
| `name` | Case-insensitive literal substring, at most 100 characters |
| `countryPublicId`, `cityPublicId` | Location filters using public identifiers |
| `foundedFrom`, `foundedTo` | Inclusive `yyyy-MM-dd` bounds; from must not exceed to |
| `hasStadium` | Omit for any; `true` or `false` filters active stadium assignment |
| `clubAge` | U7=0, U9=1, U11=2, U13=3, U16=4, U19=5, U21=6, Senior=7 |
| `sortBy`, `descending` | `name` (default), `foundingDate` or `clubAge`; ascending by default |

Create/update bodies contain `name`, nullable `foundingDate`, numeric `clubAge`,
nullable `cityPublicId` and nullable `stadiumPublicId`. Names are trimmed and
limited to 2–100 characters, with at least one character supported by the shared
slug generator. A supplied date must be valid and no later than the current UTC
date. A supplied reference must identify an active record. Duplicate active
slugs and invalid references return field validation errors.

DTOs expose public identifiers, names and slugs, never internal foreign-key
GUIDs. A successful delete returns `{ "publicId": "…" }`. Delete uses the shared
soft-delete mechanism; deleted clubs are hidden and their slugs may be reused.

## UI and persistence

- Public routes: `/clubs` and `/clubs/:slug`.
- Guarded forms: `/manage-clubs` and `/manage-clubs/:slug`, opened from the club list.
- Forms use Reactive Forms with inline client/server errors and duplicate-submit protection.
- Dates and totals use the shared regional pipes. Filters and pagination run on the server.
- The shared confirmation dialog supports a named modal, keyboard focus containment,
  Escape dismissal and focus return.
- Migration `20261018114239_AddClubs` adds only the `Clubs` table and its indexes/FKs.
  City/stadium references are optional and use `Restrict`; slug uniqueness applies
  only to active rows. EF configuration is in `AppDbContext.OnModelCreating`.
- The domain is reflected in [class-diagram.mmd](class-diagram.mmd).
