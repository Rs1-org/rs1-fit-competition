# /feat-plan — Feature Implementation Planner

> **Agent-agnostic command.** Works with Claude Code, Codex, Cursor, or any agent that reads `.claude/commands/`.

---

## Purpose

Generate a detailed, RS1-compliant implementation plan for a feature **before writing any code**.

The plan is written to `.claude/previews/plan-<feature-name>.md` — gitignored, local only.
Use it as your personal checklist while implementing. When done, run `/feat-check` → `/feat-update`.

---

## Usage

```
/feat-plan "Club entity with full CQRS stack"
/feat-plan "Stripe payment integration"
/feat-plan "Dark light theme toggle"
```

The description in quotes becomes the plan name and file name.

---

## Agent Instructions

### Step 0 — Clear previews
- Delete all files in `.claude/previews/` before generating the new plan
- This ensures stale plans from previous features don't cause confusion

### Step 1 — Load context
- Read `CLAUDE.md` — exact folder paths and naming conventions for this project
- Read `.claude/docs/RS1_REQUIREMENTS.md` — every mandatory RS1 rule
- Read `.claude/docs/ENTITIES.md` — existing entities and relationships
- Read `.claude/docs/DECISIONS.md` — architectural decisions (don't contradict these)
- Read the relevant member's `.claude/docs/features_<member>.md` — what's already done
- Identify which member is running this: `git config user.name`

### Step 2 — Determine feature type
- Is this a new entity? → Entity plan template
- Is this an advanced feature (Stripe, SignalR, theme etc.)? → Advanced feature plan template
- Is this a fix/refactor (paging, CancellationToken, validators)? → Fix plan template

### Step 3 — Generate the plan file

Write to `.claude/previews/plan-<feature-name>.md`:

```markdown
# Feature Plan — <feature name>
**Member:** <name>
**Date:** <today>
**Type:** New Entity | Advanced Feature | Fix/Refactor
**Estimated Points:** Xb (Basic) / Xb (Advanced)
**RS1 Items:** Item X — <name>

---

## ⚠️ Pre-flight Checklist
> Complete these BEFORE writing any code.

- [ ] ENTITIES.md updated with new entity fields and relationships (if applicable)
- [ ] Branch created: `feat/<feature-name>`
- [ ] Understand which layer each file belongs to (Domain / Application / Infrastructure / API)
- [ ] Read ADR-005 (soft delete) — entity must inherit BaseEntity
- [ ] Read ADR-004 (EF config in AppDbContext.OnModelCreating)

---

## Implementation Steps

### Step 1 — Domain Layer
`Competition.Domain/Entities/`

- [ ] Create `<Entity>.cs` inheriting `BaseEntity`
  ```csharp
  // Fields:
  // <list every field with type and constraints>
  // Navigation properties:
  // <list FKs and collections>
  ```
- [ ] Update `ENTITIES.md` — add entity with all fields, relationships, owner

---

### Step 2 — EF Configuration
`Competition.Infrastructure/Data/AppDbContext.cs` → `OnModelCreating`

- [ ] Add entity to `DbSet<Entity>` in AppDbContext
- [ ] Configure in `OnModelCreating`:
  - [ ] Primary key
  - [ ] Required fields
  - [ ] Max lengths
  - [ ] FK relationships (HasOne/WithMany, HasForeignKey)
  - [ ] Indexes (unique constraints if any)
  - [ ] Soft delete query filter: `.HasQueryFilter(e => e.DeletedAt == null)`

---

### Step 3 — DTOs
`Competition.Application/` (DTOs folder or co-located with commands)

- [ ] `<Entity>ListDto` — fields for list view (lightweight)
- [ ] `<Entity>DetailsDto` — fields for detail view (full)
- [ ] `Create<Entity>Dto` — fields accepted on create
- [ ] `Update<Entity>Dto` — fields accepted on update

---

### Step 4 — Commands + Validators
`Competition.Application/Commands/<Entity>Commands/`

- [ ] `Create<Entity>Command.cs` + `Create<Entity>CommandHandler.cs`
  - Handler signature: `Handle(Create<Entity>Command request, CancellationToken cancellationToken)`
  - Returns: `<Entity>DetailsDto`
- [ ] `Create<Entity>CommandValidator.cs`
  - Rules: <list specific validation rules for this entity>
- [ ] `Update<Entity>Command.cs` + handler + validator
- [ ] `Delete<Entity>Command.cs` + handler (sets `DeletedAt = DateTime.UtcNow`, never hard delete)
  - Validator: entity must exist and not already be deleted

---

### Step 5 — Queries
`Competition.Application/Queries/<Entity>/`

- [ ] `GetAll<Entities>Query.cs` + `GetAll<Entities>QueryHandler.cs`
  - Returns: `PagedResult<<Entity>ListDto>` — never raw `IEnumerable`
  - Accepts: `PageNumber`, `PageSize`, + filter params (min 5 for RS1)
  - Filter params: <list specific filter fields for this entity>
- [ ] `Get<Entity>ByIdQuery.cs` + handler
  - Returns: `<Entity>DetailsDto`

---

### Step 6 — Repository (if needed)
`Competition.Infrastructure/Repositories/`

- [ ] Add interface to `Competition.Application/Interfaces/I<Entity>Repository.cs`
- [ ] Implement in `Competition.Infrastructure/Repositories/<Entity>Repository.cs`
- [ ] All methods async, all accept and pass `CancellationToken`
- [ ] Register in DI

---

### Step 7 — Controller
`Competition.Api/Controllers/<Entity>Controller.cs`

- [ ] Inject only `IMediator` — no direct service injection
- [ ] `GET /api/<entities>` → `_mediator.Send(new GetAll<Entities>Query(...))`
- [ ] `GET /api/<entities>/{id}` → `_mediator.Send(new Get<Entity>ByIdQuery(id))`
- [ ] `POST /api/<entities>` → `[Authorize]` + `_mediator.Send(new Create<Entity>Command(...))`
- [ ] `PUT /api/<entities>/{id}` → `[Authorize]` + `_mediator.Send(new Update<Entity>Command(...))`
- [ ] `DELETE /api/<entities>/{id}` → `[Authorize]` + `_mediator.Send(new Delete<Entity>Command(id))`
- [ ] All actions accept `CancellationToken cancellationToken` and pass it to `_mediator.Send()`

---

### Step 8 — Frontend
`frontend/src/app/features/<entity>/`

- [ ] Feature module with lazy loading route
- [ ] `<entity>-list` component — table with min 5 filter params, pagination
- [ ] `<entity>-form` component — reactive form with FE validation
- [ ] `<entity>.service.ts` — calls API, uses Signals for state
- [ ] Route for write operations protected by `admin-moderator.guard.ts`
- [ ] FE validation mirrors BE validation rules

---

### Step 9 — Tests
`Competition.Tests/` (create project if it doesn't exist)

- [ ] Unit test: `Create<Entity>CommandHandlerTests.cs`
  - Test: valid command creates entity
  - Test: invalid command throws validation error
- [ ] Integration test: `<Entity>ControllerTests.cs`
  - Test: GET returns paged list
  - Test: POST creates entity and returns 201

---

## RS1 Final Checklist
> Every item below must be ✅ before running /feat-check

- [ ] New entity inherits `BaseEntity` (soft delete)
- [ ] ENTITIES.md updated
- [ ] EF config in `AppDbContext.OnModelCreating` (HasQueryFilter for soft delete)
- [ ] Every handler has `CancellationToken` in signature AND passed to repo calls
- [ ] Every command has a validator with real rules (not empty)
- [ ] All list handlers return `PagedResult<T>`
- [ ] Controllers only use `_mediator.Send()` — zero direct service calls
- [ ] Write endpoints have `[Authorize]`
- [ ] FE routes have `admin-moderator.guard.ts`
- [ ] DTOs used everywhere — no domain entities in responses
- [ ] At least one unit test and one integration test

---

## Points This Feature
| Category | Item | Points |
|---|---|---|
| Basic / Advanced | <feature> | Xb |
| **Total** | | **Xb** |

---

## Notes / Open Questions
> Use this space while planning — scratch pad, things to confirm with teammates, etc.
```

### Step 4 — Print summary
After writing the file, print:
```
Plan written to .claude/previews/plan-<feature-name>.md
Estimated points: Xb
Steps: X total
First step: Update ENTITIES.md (if new entity)
```

---

## What /feat-plan does NOT do
- Does not write any source code
- Does not modify any existing docs
- Does not create Jira tasks (do that manually from the plan)
- Does not commit anything