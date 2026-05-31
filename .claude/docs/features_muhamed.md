# Feature Checklist: Muhamed
> Basic points capped at 15b. All new work targets advanced points only.

---

## Basic Features (15b / 15b — CAP REACHED)

### Country Management — 5b
- [x] Backend: `Country` entity in `Competition.Domain/Entities/`
- [x] Backend: `CreateCountryCommandHandler`, `UpdateCountryCommandHandler`, `DeleteCountryCommandsHandler`
- [x] Backend: `GetAllCountriesCommandHandler` (note: incorrectly named Command, see ADR-007)
- [x] Backend: `CountryController` using `_mediator.Send()`
- [x] Frontend: `country-list` component
- [ ] Backend: `CreateCountryCommand` validator has real rules (check — may be empty)
- [x] Backend: All Delete handlers pass CancellationToken through to repository
- [x] Backend: `GetAllCountriesCommandHandler` returns `PagedResult<T>` not raw list

### City Management — 5b
- [x] Backend: `City` entity in `Competition.Domain/Entities/`
- [x] Backend: `CreateCityCommandHandler`, `UpdateCityCommandHandler`, `DeleteCityCommandHandler`
- [x] Backend: `GetAllCitiesCommandHandler` (incorrectly named, see ADR-007)
- [x] Backend: `CityController` using `_mediator.Send()`
- [x] Frontend: `city-list` component with Country filter
- [x] Backend: `GetAllCitiesByCountryCommandHandler` uses CancellationToken correctly
- [ ] Backend: City validator has real rules (currently minimal)
- [x] Backend: Delete handler passes CancellationToken to repository

### Stadium Management — 5b
- [x] Backend: `Stadium` entity in `Competition.Domain/Entities/MatchEntities/Stadium.cs`
- [x] Backend: `CreateStadiumCommandHandler`, `UpdateStadiumCommandHandler`, `DeleteStadiumCommandHandler`
- [x] Backend: `GetAllStadiumsQueryHandler`
- [x] Backend: `StadiumController` using `_mediator.Send()`
- [x] Frontend: `stadium-list`, `stadium-detail` components
- [ ] Backend: Stadium validator has real rules
- [x] Backend: Capacity validation (> 0) enforced in FluentValidator

---

## Advanced Features (10b / 30b minimum needed)

### Testing Infrastructure — 10b (RS1 mandatory)
- [x] `Competition.Tests` xUnit project created with all packages (FluentAssertions, Moq, Mvc.Testing, EF InMemory)
- [x] References: Competition.Api, Application, Infrastructure added to test project
- [x] Added to `CompetitionSolution.sln`
- [x] `BaseUnitTest.cs` — shared `Mock<IMapper>` base class
- [x] `BaseIntegrationTest.cs` — `WebApplicationFactory<Program>` with InMemory DB swap
- [x] `Program.cs` — `public partial class Program {}` present; `IsRelational()` guard on migration
- [x] `SlugHelperTests.cs` — 6 Theory cases covering spaces, special chars, unicode (ČĆŽŠĐ), trim
- [x] `SlugHelper.cs` — rewritten with NFD normalization + Đ→d mapping + regex cleanup
- [x] `ValidationBehaviorTests.cs` — 3 Facts: no validators / valid / invalid
- [x] `CreateCityCommandHandlerTests.cs` — creation, mapping accuracy, CancellationToken propagation
- [x] `CityControllerTests.cs` — `GET /api/City/getCities` → 200 OK + `PagedResult<CityListDto>`
- [x] All 11 tests pass: `Passed! Failed: 0, Passed: 11`

### Match Entity — Domain + EF Migration (mandatory — entity count)
> Sprint scope: Steps 1–3 only. Steps 4–10 deferred to next sprint.
- [x] `Match.cs` — inherits `BaseEntity`; `Date (Instant)`, `FollowersCount`, `HomeScore?`, `AwayScore?`, `HalfHomeScore?`, `HalfAwayScore?`, `IsPostponed`, `Slug`; FKs to `MatchStatus` + `Stadium`
- [x] `MatchStatus.cs` — inherits `BaseEntity`; `Name (required, max 50)`, `Slug`; `ICollection<Match>`
- [x] EF config in `AppDbContext.OnModelCreating` — `HasQueryFilter`, `HasIndex(Slug).IsUnique`, `HasOne→MatchStatus (Restrict)`, `HasOne→Stadium (Restrict)`
- [x] Migration `20260531083023_AddMatchAndMatchStatus` generated and reviewed
- [x] `ENTITIES.md` updated — entity count 13 → 15 (1 still needed for 16-entity minimum)
- [x] DTOs (`MatchListDto`, `MatchDetailsDto`, `MatchStatusListDto`)
- [x] Commands + Validators (Create, Update, Delete) — see Match Commands section below
- [ ] Queries (GetAll with filters + paging, GetBySlug)
- [x] Repository (`IMatch`, `MatchRepository`) + DI registration
- [ ] Controller (`MatchController`) with `[Authorize]` on writes
- [ ] Frontend (`match-list`, `match-form`, `match.service.ts`)
- [ ] Tests (`CreateMatchCommandHandlerTests`, `MatchControllerTests`)
- [ ] Connect to advanced feature for points (SignalR +8b / charts +4b / PDF +3b)

### Match Commands — CQRS Write Side (mandatory prerequisite)
- [x] `NodaTimeMappingHelper` — `FormatInstant(Instant)` + `ParseInstant(string?)` added
- [x] `MappingProfile` — `CreateMatchCommand→Match`, `Match→MatchDetailsDto`, `Match→MatchListDto`, `MatchStatus→MatchStatusListDto`
- [x] `IMatch` interface — `GetBySlugAsync(string, CancellationToken)` + inherits `IRepository<Match>`
- [x] `MatchRepository` — `GetBySlugAsync` with `.Include(Status).Include(Stadium)`; registered in DI
- [x] `CreateMatchCommand` + `CreateMatchCommandHandler` — maps, sets `Instant.FromDateTimeUtc`, saves, reloads, returns `MatchDetailsDto`
- [x] `UpdateMatchCommand` (`[JsonIgnore] Slug`) + `UpdateMatchCommandHandler` — loads by slug, updates all fields, reloads, returns `MatchDetailsDto`
- [x] `DeleteMatchCommand` + `DeleteMatchCommandHandler` — loads by ID, soft-deletes, returns `Guid`
- [x] `MatchValidators.cs` — `CreateMatchCommandValidator` (date in future, slug max 200, paired FK fields); `UpdateMatchCommandValidator` (paired scores, half ≤ full, scores ≥ 0); `DeleteMatchCommandValidator` (Id not empty)
- [x] CancellationToken in all 3 `Handle()` signatures + every repo/UoW call
- [x] Build clean, 11/11 tests passing

### Stripe Payment Integration — 10b (RS1 Item 3)
- [ ] Define business process (what does user pay for? ticket / match pass / subscription)
- [ ] Install `Stripe.net` NuGet package
- [ ] Add Stripe keys to `appsettings.Development.json` — never hardcoded
- [ ] Backend: `Payment` entity + EF config in `AppDbContext.OnModelCreating`
- [ ] Backend: `CreatePaymentCommand` + handler + FluentValidator
- [ ] Backend: Payment intent endpoint in `PaymentsController`
- [ ] Backend: Transaction saved to DB on successful payment
- [ ] Backend: Error handling (declined card, insufficient funds)
- [ ] Frontend: Stripe Elements payment form
- [ ] Frontend: Success / Cancel redirect pages
- [ ] RS1 check: transaction must be connected to real business process — not a standalone demo

### Remaining Advanced Points Needed: 20b+
> Suggested options to reach 30b advanced:
> - PDF Reports — 3b each (max 6b) — RS1 Item 12
> - Column sorting in tables — 3b — RS1 Item 13
> - File upload (backend) — 3b — RS1 Item 6
> - JS charts — 4b — RS1 Item 22
> - Autocomplete — 2b — RS1 Item 31
> - Notifications read/unread — 3b — RS1 Item 26

---

## Open RS1 Violations (fix before running /update)

| Issue | File | Priority |
|---|---|---|
| `RegisterUserValidator` is empty | `Auth/Validators/RegisterUserValidator.cs` | High |
| ~~Delete handlers missing CancellationToken in repo calls~~ | ~~`*DeleteCommandHandler.cs`~~ | ~~Medium~~ ✅ Fixed |
| ~~Some GetAll handlers not returning `PagedResult<T>`~~ | ~~`GetAllCountries*`, `GetAllCities*`~~ | ~~High~~ ✅ Fixed |
| Some validators are empty or minimal | Country/City validators | High |