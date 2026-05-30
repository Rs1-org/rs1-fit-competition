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
- [ ] Backend: All Delete handlers pass CancellationToken through to repository
- [ ] Backend: `GetAllCountriesCommandHandler` returns `PagedResult<T>` not raw list

### City Management — 5b
- [x] Backend: `City` entity in `Competition.Domain/Entities/`
- [x] Backend: `CreateCityCommandHandler`, `UpdateCityCommandHandler`, `DeleteCityCommandHandler`
- [x] Backend: `GetAllCitiesCommandHandler` (incorrectly named, see ADR-007)
- [x] Backend: `CityController` using `_mediator.Send()`
- [x] Frontend: `city-list` component with Country filter
- [x] Backend: `GetAllCitiesByCountryCommandHandler` uses CancellationToken correctly
- [ ] Backend: City validator has real rules (currently minimal)
- [ ] Backend: Delete handler passes CancellationToken to repository

### Stadium Management — 5b
- [x] Backend: `Stadium` entity in `Competition.Domain/Entities/MatchEntities/Stadium.cs`
- [x] Backend: `CreateStadiumCommandHandler`, `UpdateStadiumCommandHandler`, `DeleteStadiumCommandHandler`
- [x] Backend: `GetAllStadiumsQueryHandler`
- [x] Backend: `StadiumController` using `_mediator.Send()`
- [x] Frontend: `stadium-list`, `stadium-detail` components
- [ ] Backend: Stadium validator has real rules
- [ ] Backend: Capacity validation (> 0) enforced in FluentValidator

---

## Advanced Features (0b / 30b minimum needed)

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
| Delete handlers missing CancellationToken in repo calls | `*DeleteCommandHandler.cs` | Medium |
| Some GetAll handlers may not return `PagedResult<T>` | `GetAllCountries*`, `GetAllCities*` | High |
| Some validators are empty or minimal | Country/City validators | High |