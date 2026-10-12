# Feature Checklist: Dino
> COMP-13-club-crud: Club implemented and verified in a draft PR; merge/review pending. Selection → Club remains COMP-36.
> Club CRUD is eligible for 5 basic points after merge; no points awarded by this draft.

---

## ⚠️ Pre-requisite: Understand the architecture first
Before writing any code, read:
- `CLAUDE.md` — folder paths and naming conventions
- `.claude/docs/RS1_REQUIREMENTS.md` — what every handler must have
- `.claude/docs/ENTITIES.md` — how Club connects to Stadium, City, Player
- `.claude/docs/DECISIONS.md` — ADR-004 (EF in AppDbContext), ADR-005 (soft delete), ADR-007 (naming)

Every handler you write must have: MediatR, FluentValidation with real rules, CancellationToken, PagedResult on lists, DTO return types. No exceptions.

---

## Basic Features (0b / 12b)

### Club Management — 5b
- [x] Backend: `Club` entity in `Competition.Domain/Entities/`
  - Fields: internal `Id`, `PublicId`, `Name`, `Slug`, `FoundingDate`, `ClubAge`, `CityId`, `StadiumId`
  - Inherits `BaseEntity` (soft delete via `DeletedAt`)
  - EF config in `AppDbContext.OnModelCreating`
- [x] Backend: `CreateClubCommand` + `CreateClubCommandHandler` + `CreateClubCommandValidator` (real rules)
- [x] Backend: `UpdateClubCommand` + handler + validator
- [x] Backend: `DeleteClubCommand` + handler (sets `DeletedAt`, does not hard delete)
- [x] Backend: `GetAllClubsQuery` + handler returning `PagedResult<ClubResponseDto>`
- [x] Backend: `GetClubBySlugQuery` + handler returning `ClubResponseDto`
- [x] Backend: `ClubController` — all actions use MediatR, write actions have `[Authorize]`
- [x] Backend: CancellationToken passed through in every handler, repository and UnitOfWork call
- [x] Frontend: `club-list` with five filters (name, city, stadium, founding year, age group) and backend pagination
- [x] Frontend: Create/Edit Reactive Forms with FE validation matching BE rules
- [x] Frontend: Lazy-loaded write routes protected by `admin-moderator.guard.ts`
- [x] Tests: Club handler, validators, repository filters/soft delete, HTTP authorization/CRUD and frontend behavior
- [ ] Merge/review and COMP-36 selection relationship before closing the overall story

### Selection (National Team) Management — 5b
- [ ] Backend: `Selection` entity in `Competition.Domain/Entities/`
  - Fields: `Id`, `Name`, `CountryId` (FK → Country), `Confederation`, `FifaRanking`
  - Inherits `BaseEntity`
  - EF config in `AppDbContext.OnModelCreating`
- [ ] Backend: Full CQRS — Create/Update/Delete commands + GetAll/GetById queries
- [ ] Backend: All validators with real rules (FifaRanking range 1–211, Name required)
- [ ] Backend: `SelectionController` — all actions via `_mediator.Send()`
- [ ] Backend: `GetAllSelectionsQuery` returns `PagedResult<SelectionListDto>`
- [ ] Frontend: `selection-list` component with filter
- [ ] Frontend: Create/Edit form with FE validation

### Login / Logout — 2b
- [ ] Frontend: Login form connected to JWT auth endpoint
- [ ] Frontend: Logout clears token + refresh token
- [ ] Frontend: Auth interceptor injects Bearer token on requests
- [ ] This may already exist — check `frontend/src/app/core/interceptors/` and auth pages

---

## Advanced Features (0b / 30b minimum needed)

### Match Events System — ~10b
- [ ] Backend: `Match` entity
  - Fields: `HomeTeamId`, `AwayTeamId`, `StadiumId`, `RefereeId`, `ScheduledAt`, `Status`, `HomeScore`, `AwayScore`
  - A match is Club vs Club OR Selection vs Selection — enforce in validator
- [ ] Backend: `MatchEvent` entity
  - Fields: `MatchId`, `PlayerId`, `EventType` (Goal/Card/Substitution), `Minute` (0–120)
- [ ] Backend: Full CQRS for Match: Create/Update/GetAll/GetById
- [ ] Backend: Full CQRS for MatchEvent: AddEvent/GetEventsByMatch
- [ ] Backend: Validator: player must belong to one of the two teams in the match
- [ ] Frontend: Match list/detail view
- [ ] Frontend: Event timeline display per match

### Remaining Advanced Points Needed: 20b+
> Suggested options to reach 30b advanced:
> - SignalR real-time match event updates — 8b — RS1 Item 21
> - JS charts (match stats, goals per minute) — 4b — RS1 Item 22
> - PDF match report — 3b — RS1 Item 12
> - Column sorting — 3b — RS1 Item 13
> - Autocomplete (search clubs/players) — 2b — RS1 Item 31

---

## Recommended Implementation Order
1. `Club` entity + EF config + CQRS handlers + controller + frontend list
2. `Selection` entity + same stack
3. Login/Logout frontend (if not already done)
4. `Match` + `MatchEvent` entities + handlers
5. Advanced features to reach 30b

Run `/feat-req` after completing each branch, then `/update` if clean.
