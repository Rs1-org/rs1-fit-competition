# Feature Checklist: Edin
> Basic points blocked until paging violations are fixed.
> Fix `GetAllManagersQueryHandler` and `GetAllRefereesQueryHandler` first.

---

## Basic Features (0b / 15b — BLOCKED by RS1 violations)

### Player Management — 5b
- [x] Backend: `Player` entity in `Competition.Domain/Entities/Person/Player.cs`
- [x] Backend: `CreatePlayerCommandHandler`, `UpdatePlayerCommandHandler`, `DeletePlayerCommandHandler`
- [x] Backend: `GetAllPlayersQueryHandler`, `SearchPlayersQueryHandler`
- [x] Backend: `PlayerController` using `_mediator.Send()`
- [x] Frontend: `player-list`, `manage-players` components, filter by position
- [ ] Backend: `GetAllPlayersQueryHandler` returns `PagedResult<T>` — **confirm this**
- [ ] Backend: All handlers pass CancellationToken through to repository
- [ ] Backend: Player FluentValidator has real rules

### Referee Management — 5b
- [x] Backend: `Referee` entity in `Competition.Domain/Entities/Person/Referee.cs`
- [x] Backend: CRUD handlers implemented
- [x] Backend: `RefereeController` using `_mediator.Send()`
- [x] Frontend: `referee-list`, `manage-referees` components
- [ ] Backend: `GetAllRefereesQueryHandler` returns `PagedResult<T>` — **likely missing, check**
- [ ] Backend: CancellationToken passed through in all handlers
- [ ] Backend: Referee FluentValidator has real rules

### Manager Management — 5b
- [x] Backend: `Manager` entity in `Competition.Domain/Entities/Person/Manager.cs`
- [x] Backend: CRUD handlers implemented
- [x] Backend: `ManagerController` using `_mediator.Send()`
- [x] Frontend: `manager-list`, `manage-managers` components
- [ ] Backend: `GetAllManagersQueryHandler` returns `PagedResult<ManagerDto>` — **CONFIRMED MISSING**
- [ ] Backend: `ManagerController.GetAll` passes CancellationToken — **CONFIRMED MISSING**
- [ ] Backend: Manager FluentValidator has real rules

> ✅ Once all three `GetAll*` handlers return `PagedResult<T>` and CancellationTokens are fixed,
> run `/feat-req` → `/update` to unlock **15b basic points**.

---

## Advanced Features (0b / 30b minimum needed)

### Dark/Light Theme — 3b (RS1 Item 85)
- [ ] Angular theme toggle service
- [ ] Theme persisted (Signal + localStorage)
- [ ] Applied via Angular Material theming (`@use '@angular/material' as mat`)
- [ ] Both themes fully styled — not just background color change
- [ ] Toggle accessible from navbar or settings

### Angular Animations — 3b (RS1 Item 41)
- [ ] Route transition animations (fade or slide between pages)
- [ ] At least one component-level animation (list enter/leave, card, dialog)
- [ ] Uses `@angular/animations` — `trigger()`, `state()`, `transition()`, `animate()`
- [ ] Animations smooth — no jank on slower connections

### Remaining Advanced Points Needed: 24b+
> Suggested options to reach 30b advanced:
> - Custom async Angular validators — 3b — RS1 Item 38
> - Custom Angular pipes — 3b — RS1 Item 45
> - Custom Angular directives — 4b — RS1 Item 46
> - Responsive design with Angular Material — 5b — RS1 Item 40
> - Autocomplete — 2b — RS1 Item 31
> - PDF reports — 3b each — RS1 Item 12
> - JS charts — 4b — RS1 Item 22

---

## Open RS1 Violations (fix before running /update)

| Issue | File | Priority |
|---|---|---|
| `GetAllManagersQueryHandler` returns `IEnumerable` not `PagedResult` | `GetAllManagersQueryHandler.cs` | Critical |
| `ManagerController.GetAll` missing CancellationToken | `ManagerController.cs` | High |
| `GetAllRefereesQueryHandler` paging — confirm | `GetAllRefereesQueryHandler.cs` | High |
| FluentValidators for Player/Referee/Manager — confirm rules exist | `*Validators/` | High |