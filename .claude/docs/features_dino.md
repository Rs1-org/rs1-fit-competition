# Feature Checklist: Dino
> Current status: 0 entities implemented. Everything below is not yet started.
> Sprint 3 priority: start with Club entity end-to-end before touching Selection or Match.

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
- [ ] Backend: `Club` entity in `Competition.Domain/Entities/`
  - Fields: `Id`, `Name`, `Logo` (URL), `StadiumId` (FK → Stadium)
  - Inherits `BaseEntity` (soft delete via `DeletedAt`)
  - EF config in `AppDbContext.OnModelCreating`
- [ ] Backend: `CreateClubCommand` + `CreateClubCommandHandler` + `CreateClubCommandValidator` (real rules)
- [ ] Backend: `UpdateClubCommand` + handler + validator
- [ ] Backend: `DeleteClubCommand` + handler (sets `DeletedAt`, does not hard delete)
- [ ] Backend: `GetAllClubsQuery` + handler returning `PagedResult<ClubListDto>`
- [ ] Backend: `GetClubByIdQuery` + handler returning `ClubDetailsDto`
- [ ] Backend: `ClubController` — all actions use `_mediator.Send()`, write actions have `[Authorize]`
- [ ] Backend: CancellationToken passed through in every handler and repository call
- [ ] Frontend: `club-list` component with filter (min 5 params: name, city, stadium, founded year, active)
- [ ] Frontend: Create/Edit dialog or form with FE validation
- [ ] Frontend: Route protected by `admin-moderator.guard.ts` for write operations

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