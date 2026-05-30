# CompetitionSolution - RS1 Project

FlashScore-like sports competition web application. A university course project for RS1 at FIT Sarajevo.

## Project Overview
- **Muhamed:** Country, City, Stadium, Stripe Integration
- **Dino:** Club, Selection, Match Events (Not yet implemented)
- **Edin:** Player, Referee, Manager, Dark/Light Theme, Angular Animations

## Essential Folder Paths
- **MediatR Handlers:**
  - Commands: `backend/CompetitionSolution/Competition.Application/Commands/[Entity]Commands/[Action]/`
  - Queries: `backend/CompetitionSolution/Competition.Application/Queries/[Entity]/[Action]/`
- **Validators:** `backend/CompetitionSolution/Competition.Application/Commands/[Entity]Commands/Validators/`
- **EF Fluent API:** `backend/CompetitionSolution/Competition.Infrastructure/Data/AppDbContext.cs` (in `OnModelCreating`)
- **Controllers:** `backend/CompetitionSolution/Competition.Api/Controllers/`
- **Angular Features:** `frontend/src/app/features/`
- **Angular Guards:** `frontend/src/app/core/guards/`

## Naming Conventions
- **C# Commands:** `[Action][Entity]Command` (e.g., `CreateCityCommand`) or `[Action][Entity]Commands` (e.g., `UpdateCityCommands`).
- **C# Queries:** `GetAll[Entities]Query` or `Get[Entity]ByIdQuery`. *Note: Some queries are incorrectly named as `Command` (e.g., `GetAllCitiesCommand`).*
- **C# Handlers:** `[Command/QueryName]Handler`.
- **C# DTOs:** `[Entity][Type]Dto` (e.g., `CityListDto`, `ManagerDto`).

## Current Implementation Status

### Muhamed
- **Implemented:**
  - **Entities:** `Country`, `City`, `GeoData`, `Stadium` (in `MatchEntities/Stadium.cs`)
  - **Controllers:** `CountryController`, `CityController`, `StadiumController`
  - **Handlers:** `CreateCityCommandHandler`, `UpdateCityCommandHandler`, `DeleteCityCommandHandler`, `GetAllCitiesCommandHandler`, `CreateCountryCommandHandler`, `UpdateCountryCommandHandler`, `DeleteCountryCommandsHandler`, `GetAllCountriesCommandHandler`, `CreateStadiumCommandHandler`, `UpdateStadiumCommandHandler`, `DeleteStadiumCommandHandler`, `GetAllStadiumsQueryHandler`
  - **Frontend:** `city-list`, `country-list`, `stadium-list`, `stadium-detail` components.
- **Missing / RS1 Compliance Issues:**
  - `GetAllCitiesByCountryCommandHandler` uses `CancellationToken` correctly, but some `Delete` handlers are missing `CancellationToken` in repository calls.
  - `RegisterUserValidator` is empty.
  - `Validator.cs` for City and Country are minimal or improperly named.

### Edin
- **Implemented:**
  - **Entities:** `Player`, `Referee`, `Manager` (in `Person/` folder)
  - **Controllers:** `PlayerController`, `RefereeController`, `ManagerController`
  - **Handlers:** `CreatePlayerCommandHandler`, `UpdatePlayerCommandHandler`, `DeletePlayerCommandHandler`, `GetAllPlayersQueryHandler`, `SearchPlayersQueryHandler`, etc.
  - **Frontend:** `manage-players`, `manage-referees`, `manage-managers`, `player-list`, `referee-list`, `manager-list`.
- **Missing / RS1 Compliance Issues:**
  - `ManagerController` actions (e.g., `GetAll`) do not accept or pass `CancellationToken`.
  - `GetAllManagersQueryHandler` returns `IEnumerable<ManagerDto>` instead of `PagedResult<ManagerDto>`.

### Dino
- **Implemented:** None found in `Competition.Domain/Entities`.
- **Missing:** Everything related to Club, Selection, and Match Events.

## RS1 Architecture Rules & Compliance
1. **CQRS via MediatR:** Every controller action must use `_mediator.Send()`.
   - *Status:* Mostly followed. Check `ManagerController.cs` and `CityController.cs`.
2. **FluentValidation:** Every Command must have a validator.
   - *Status:* **Failing.** Many validators (e.g., `RegisterUserValidator.cs`) are empty or missing logic.
3. **CancellationToken:** Every handler and controller action must pass `CancellationToken`.
   - *Status:* **Failing.** Missing in `ManagerController` and several repository calls.
4. **EF Fluent API:** Use `OnModelCreating` in `AppDbContext.cs`. No DataAnnotations.
   - *Status:* Followed in `AppDbContext.cs`.
5. **DTOs Only:** Handlers and Controllers must return DTOs, never Domain Entities.
   - *Status:* Mostly followed (e.g., `CityNameDto`, `ManagerDto`).
6. **Auth Guards:** Write operations must have `[Authorize]`.
   - *Status:* Followed in `CityController.cs` and `ManagerController.cs`.
7. **Backend Paging:** List endpoints must return `PagedResult<T>`.
   - *Status:* Followed for `City` but missing for `Manager` and `Referee`.
8. **Automated Tests:** Unit and Integration tests required.
   - *Status:* **Missing.** No test projects found in the solution.

## Agent Workflow
- **`/feat-req`**: Agent scans branch for RS1 compliance (CQRS, Validators, Tokens) and writes preview to `.claude/previews/`.
- **`/update`**: Agent updates `features_<member>.md` and `POINTS_TRACKER.md`.

## Documentation Index
- [RS1 Requirements](.claude/docs/RS1_REQUIREMENTS.md)
- [Entity Map](.claude/docs/ENTITIES.md)
- [Architecture Decisions](.claude/docs/DECISIONS.md)
- [Points Tracker](.claude/docs/POINTS_TRACKER.md)
- [Features: Muhamed](.claude/docs/features_muhamed.md)
- [Features: Dino](.claude/docs/features_dino.md)
- [Features: Edin](.claude/docs/features_edin.md)
