# CompetitionSolution - RS1 Project

FlashScore-like sports competition web application. A university course project for RS1 at FIT Sarajevo.

## Project Overview
- **Muhamed:** Country, City, Stadium, Stripe Integration
- **Dino:** Club, Selection, Match Events (Not yet implemented)
- **Edin:** Player, Referee, Manager, Dark/Light Theme, Angular Animations

## Essential Folder Paths

### Backend (.NET)
- **Controllers:** `backend/CompetitionSolution/Competition.Api/Controllers/`
- **MediatR Handlers:**
  - Auth: `backend/CompetitionSolution/Competition.Application/Auth/Handlers/`
  - Commands: `backend/CompetitionSolution/Competition.Application/Commands/[Entity]Commands/[Action]/`
  - Queries: `backend/CompetitionSolution/Competition.Application/Queries/[Entity]/[Action]/`
- **Validators:**
  - Auth: `backend/CompetitionSolution/Competition.Application/Auth/Validators/`
  - Commands: `backend/CompetitionSolution/Competition.Application/Commands/[Entity]Commands/Validators/`
- **EF Config:** `backend/CompetitionSolution/Competition.Infrastructure/Data/AppDbContext.cs` → `OnModelCreating`
- **Domain Entities:** `backend/CompetitionSolution/Competition.Domain/Entities/`
- **BaseEntity:** `backend/CompetitionSolution/Competition.Domain/Entities/BaseEntity.cs`
- **Interfaces:** `backend/CompetitionSolution/Competition.Application/Interfaces/`
- **Repositories:** `backend/CompetitionSolution/Competition.Infrastructure/Repositories/`
- **PagedResult:** `backend/CompetitionSolution/Competition.Application/Common/Models/PagedResult.cs`

### Frontend (Angular)
- **Feature Modules:** `frontend/src/app/features/`
- **Pages:** `frontend/src/app/pages/`
- **Shared Components:** `frontend/src/app/shared/components/`
- **Services:** `frontend/src/app/core/services/`
- **Guards:** `frontend/src/app/core/guards/`
- **Interceptors:** `frontend/src/app/core/interceptors/`
- **Models:** `frontend/src/app/core/models/`

## Naming Conventions
- **Commands:** `[Action][Entity]Command` (e.g. `CreateCityCommand`)
- **Handlers:** `[Command/QueryName]Handler` (e.g. `CreateCityCommandHandler`)
- **Queries:** `GetAll[Entities]Query` or `Get[Entity]ByIdQuery`
- **DTOs:** `[Entity][Type]Dto` (e.g. `CityListDto`, `CityDetailsDto`)
- **Note:** Some existing queries are incorrectly named `*Command` — see DECISIONS.md ADR-007. Do not flag as violations.

## RS1 Architecture Rules (non-negotiable)
1. **CQRS via MediatR** — controllers only call `_mediator.Send()`, zero direct service injection
   → `Competition.Api/Controllers/`
2. **Clean Architecture** — Domain → Application → Infrastructure → API, no reverse dependencies
3. **FluentValidation** — every Command has `AbstractValidator<T>` with real rules, not empty
   → `Competition.Application/Commands/[Entity]Commands/Validators/`
4. **CancellationToken** — every `Handle()` accepts it AND passes it through to repository calls
5. **EF Fluent API** — all config in `AppDbContext.OnModelCreating`, no DataAnnotations for DB config
   → `Competition.Infrastructure/Data/AppDbContext.cs`
6. **Soft delete** — all entities inherit `BaseEntity`, Delete sets `DeletedAt`, never hard deletes
   → `Competition.Domain/Entities/BaseEntity.cs`
7. **DTOs only** — handlers and controllers never return domain entities
8. **Auth guards** — write endpoints have `[Authorize]`, FE routes use `admin-moderator.guard.ts`
   → `frontend/src/app/core/guards/`
9. **Backend paging** — all list endpoints return `PagedResult<T>`, never raw `IEnumerable<T>`
   → `Competition.Application/Common/Models/PagedResult.cs`

## Workflow

```
/feat-plan "description"   →   .claude/previews/plan-<name>.md
                                step-by-step implementation plan
                                includes ENTITIES.md update as first step
    ↓
git checkout -b feat/<name>
    ↓
/feat-do "description"     →   reads plan-<name>.md
                                writes all source code in layer order
                                Domain → EF → DTOs → Commands → Queries
                                → Repository → Controller → Frontend → Tests
                                self-checks RS1 compliance before finishing
    ↓
/feat-check                →   .claude/previews/<branch-name>.md
                                RS1 compliance check
                                entity sync check (ENTITIES.md in sync?)
                                points earned
                                YES/NO ready for /feat-update
    ↓
if NO → fix issues, run /feat-check again
if YES → continue
    ↓
/feat-update               →   syncs ENTITIES.md (new/modified entities)
                                checks off items in features_<member>.md
                                adds points row to POINTS_TRACKER.md
    ↓
git add .
git commit -m "feat: <feature name>"
git add .claude/docs/
git commit -m "docs: update after <feature> — +Xb <member>"
git push
```

> Never run `/feat-update` if `/feat-check` says `Ready for /feat-update: NO`.
> Never skip `ENTITIES.md` update when adding a new entity — `/feat-check` will block you if you do.

## Slash Commands

| Command | Input | Output | Modifies |
|---|---|---|---|
| `/feat-plan "description"` | description string | `.claude/previews/plan-<name>.md` | nothing |
| `/feat-do "description"` | description string | source code | `Competition.*` + `frontend/` |
| `/feat-check` | current branch | `.claude/previews/<branch>.md` | nothing |
| `/feat-update` | current branch preview | synced docs | `ENTITIES.md`, `features_<member>.md`, `POINTS_TRACKER.md` |

## Documentation Index
- [RS1 Requirements](.claude/docs/RS1_REQUIREMENTS.md)
- [Entity Map](.claude/docs/ENTITIES.md)
- [Architecture Decisions](.claude/docs/DECISIONS.md)
- [Points Tracker](.claude/docs/POINTS_TRACKER.md)
- [Features: Muhamed](.claude/docs/features_muhamed.md)
- [Features: Dino](.claude/docs/features_dino.md)
- [Features: Edin](.claude/docs/features_edin.md)