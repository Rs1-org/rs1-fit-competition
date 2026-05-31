# RS1 Course Requirements
> Source: RS1_upute_za_izradu_seminarskog_rada_2025_26 + RS1_tabela_za_bodovanje_2025_26
> This is the authoritative reference. When in doubt, this file wins.

---

## Mandatory Architecture (blocks ALL points if violated)

| Rule | Requirement | Where enforced |
|---|---|---|
| CQRS via MediatR | Every controller action calls `_mediator.Send()` only — no direct service injection | `Competition.Api/Controllers/` |
| Clean Architecture | Domain → Application → Infrastructure → API, no reverse dependencies | `Competition.*` projects |
| FluentValidation | Every Command has an `AbstractValidator<T>` with real rules — empty validators don't count | `Competition.Application/Commands/[Entity]Commands/Validators/` |
| CancellationToken | Every handler `Handle()` method accepts and passes through `CancellationToken` | All `*Handler.cs` files |
| EF Fluent API | All entity config in `OnModelCreating` in `AppDbContext.cs` — zero DataAnnotations for DB config | `Competition.Infrastructure/Data/AppDbContext.cs` |
| DTOs only | Controllers and handlers never return domain entities — always map to DTOs | All `*Handler.cs` return types |
| Auth guards | Write endpoints have `[Authorize]`, FE routes have `admin-moderator.guard.ts` | Controllers + `frontend/src/app/core/guards/` |
| Backend paging | All list endpoints return `PagedResult<T>` — no raw `IEnumerable<T>` returns | All `GetAll*` handlers |
| Async/await + CancellationToken | All repository calls are async and pass through the token | `Competition.Infrastructure/Repositories/` |
| Min 16 entities | 3-member group requires at least 16 entities (excludes simple lookup tables) | `Competition.Domain/Entities/` |
| Data seeder | DB must be pre-populated with login credentials per role for defense | `Competition.Infrastructure/Data/` |
| Lazy loading | EF navigation properties use lazy loading | `AppDbContext.cs` + entity configs |
| Unit + Integration tests | At least one test per feature area — currently MISSING entirely | `Competition.Tests/` (not yet created) |
| English only | All code, comments, variable names in English | Entire codebase |

---

## Point Structure

### Basic Functionality — hard cap 15b per member
| Points | Item |
|---|---|
| 3b | One CRUD form with 5-param FE filter + FE & BE validation |
| 2b | Backend paging + backend filter (addon per CRUD form) |
| 2b | Login + logout form |
| 3b | Custom landing page design |
| 3b | Wizard / stepper form |

> ⚠️ Basic points are CAPPED at 15b per member regardless of how many CRUD forms are built.
> Do NOT invest more time in basic features once you hit 15b.

### Advanced Functionality — minimum 30b per member
| RS1 Item | Feature | Points |
|---|---|---|
| Item 3 | Stripe payment (must connect to business process, transaction saved to DB) | 10b |
| Item 6 | File upload/download to filesystem | 3b |
| Item 6+ | Image handling addon | 1b |
| Item 12 | PDF report with parameters (max 6b per member) | 3b each |
| Item 13 | Column sorting in table view | 3b |
| Item 14 | AI/LLM integration (ChatGPT etc.) | 8b |
| Item 21 | SignalR real-time messaging | 8b |
| Item 22 | JS charts (D3.js, Chart.js) | 4b |
| Item 26 | Notifications with read/unread status | 3b |
| Item 31 | Autocomplete on input fields | 2b |
| Item 38 | Custom async Angular validators | 3b |
| Item 40 | Responsive design with Angular Material | 5b |
| Item 41 | Angular animations for route/component transitions | 3b |
| Item 45 | Custom Angular pipes | 3b |
| Item 46 | Custom Angular directives | 4b |
| Item 85 | Dark/Light theme toggle (persisted) | 3b |

### Defense Threshold
- Minimum **40b total per member** to request defense review
- Minimum **10b basic + 30b advanced**
- Both practical exam modules must be passed before defense
- Each member defends independently — no need to wait for teammates

---

## Known RS1 Violations in Current Codebase (as of CLAUDE.md generation)

| Member | Violation | File | Severity |
|---|---|---|---|
| Muhamed | `RegisterUserValidator` is empty | `Auth/Validators/RegisterUserValidator.cs` | High |
| Muhamed | Some Delete handlers missing CancellationToken in repo calls | `*DeleteCommandHandler.cs` | Medium |
| Muhamed | Some queries incorrectly named `Command` | `GetAllCitiesCommand` etc. | Low |
| Edin | `ManagerController.GetAll` missing CancellationToken | `ManagerController.cs` | Medium |
| Edin | `GetAllManagersQueryHandler` returns `IEnumerable` not `PagedResult` | `GetAllManagersQueryHandler.cs` | High |
| Edin | `GetAllRefereesQueryHandler` missing paging | TBC | High |
| Dino | Nothing implemented | — | Critical |
| All | No test projects exist | — | High |