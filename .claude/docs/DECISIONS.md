# Architecture Decision Log (ADR)
> Add a new entry every time a non-obvious technical decision is made.
> Never delete entries — mark superseded ones as [SUPERSEDED].
> This file is read by the agent during /feat-req to avoid flagging intentional choices as violations.

---

## [ADR-001] MediatR for CQRS
**Status:** Accepted
**Decision:** All controller actions delegate to MediatR Commands (writes) or Queries (reads) via `_mediator.Send()`.
**Reason:** RS1 mandatory requirement. Keeps controllers thin and business logic testable in isolation.
**Consequence:** Adds boilerplate for simple CRUD but required for RS1 compliance.
**Enforced in:** `Competition.Api/Controllers/`

---

## [ADR-002] Angular Signals for state (not NgRx)
**Status:** Accepted
**Decision:** Use Angular Signals for reactive state in components and services. NgRx is not used as the default.
**Reason:** RS1 requirement. Signals are the modern Angular 17+ approach with lower boilerplate.
**Consequence:** NgRx remains available as an optional advanced feature (8b) if points are needed later.
**Enforced in:** `frontend/src/app/`

---

## [ADR-003] FluentValidation in Application layer
**Status:** Accepted
**Decision:** Every Command has an `AbstractValidator<T>` registered in DI. Controllers do zero validation.
**Reason:** RS1 mandatory. Keeps domain entities clean and validation logic co-located with commands.
**Consequence:** Empty validators (like current `RegisterUserValidator`) do not satisfy RS1 — they must have real rules.
**Enforced in:** `Competition.Application/Commands/[Entity]Commands/Validators/`

---

## [ADR-004] EF Fluent API in AppDbContext.OnModelCreating (not separate config files)
**Status:** Accepted
**Decision:** All entity configuration lives in `OnModelCreating` inside `AppDbContext.cs`, not in separate `IEntityTypeConfiguration<T>` files.
**Reason:** Team chose centralized config. Both approaches satisfy RS1 — this is the one actually used.
**Consequence:** Agent must look in `AppDbContext.cs` for EF config, not in separate configuration classes.
**Enforced in:** `Competition.Infrastructure/Data/AppDbContext.cs`

---

## [ADR-005] Soft delete via BaseEntity.DeletedAt
**Status:** Accepted
**Decision:** All entities inherit `BaseEntity` which has a `DeletedAt` nullable DateTime. Null = active, set = soft deleted.
**Reason:** Preserves data integrity for historical records (match events, transfer history etc.).
**Consequence:** All `GetAll` queries must filter `WHERE DeletedAt IS NULL`. Delete handlers set `DeletedAt`, never hard delete.
**Enforced in:** `Competition.Domain/Entities/BaseEntity.cs`

---

## [ADR-006] Person as abstract base class for Player, Referee, Manager
**Status:** Accepted
**Decision:** `Player`, `Referee`, and `Manager` all inherit from a common `Person` base class stored in `Entities/Person/`.
**Reason:** Shared fields (FirstName, LastName, BirthDate, ProfileImage) avoid duplication.
**Consequence:** EF Table-Per-Hierarchy (TPH) or Table-Per-Type (TPT) — confirm which is configured in AppDbContext.
**Enforced in:** `Competition.Domain/Entities/Person/`

---

## [ADR-007] Naming inconsistency — some Queries named Command
**Status:** Known issue, not yet fixed
**Decision:** Some query handlers are incorrectly named `*Command` instead of `*Query` (e.g. `GetAllCitiesCommand`, `GetAllCitiesCommandHandler`).
**Reason:** Early naming mistake, not yet refactored.
**Consequence:** Agent should NOT flag these as CQRS violations — they still use MediatR correctly. Flag only if a controller bypasses `_mediator.Send()`.
**Affected files:** `GetAllCitiesCommand.cs`, `GetAllCitiesCommandHandler.cs`, similar in Country/Stadium

---

## [ADR-008] Stripe integration connected to business process (not standalone demo)
**Status:** Planned (Sprint 3)
**Decision:** Stripe payment will be triggered by a real business process in the app — not a standalone test form.
**Reason:** RS1 explicitly requires payment to connect to a business process and transaction to be saved to DB.
**Consequence:** Must define what the user pays for before implementing (ticket, subscription, match pass — TBD).
**Owner:** Muhamed

---

## [ADR-009] Stripe payment swapped from Edin to Muhamed
**Status:** Accepted
**Decision:** Muhamed owns Stripe. Edin received Dark/Light Theme (3b) and Angular Animations (3b) in exchange.
**Reason:** Workload rebalancing for fair point distribution across the team.
**Date:** Sprint 2

---

## [ADR-010] Role-based auth via admin-moderator.guard.ts
**Status:** Accepted
**Decision:** Frontend protected routes use `admin-moderator.guard.ts` in `frontend/src/app/core/guards/`.
**Reason:** RS1 requires role-based authorization checks on both FE and BE.
**Consequence:** Every write operation route on FE must use this guard. Every write endpoint on BE must have `[Authorize]`.