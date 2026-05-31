# /feat-check — Branch Implementation Preview

> **Agent-agnostic command.** Works with Claude Code, Codex, Cursor, or any agent that reads `.claude/commands/`.

---

## Purpose

Scan the **current Git branch** and generate a single preview file showing what was implemented, whether it satisfies RS1 requirements, and how many points it earns.

This is a **read-only analysis command** — it never modifies source files.

Run this when you finish implementing a feature on a branch, before running `/feat-update`.

---

## Agent Instructions

### Step 1 — Identify context
- Read current branch: `git branch --show-current`
- Read current Git user: `git config user.name`
- Load relevant member's feature doc from `.claude/docs/features_<member>.md`
- Load `.claude/docs/RS1_REQUIREMENTS.md`
- Load `.claude/docs/ENTITIES.md`
- If a plan file exists at `.claude/previews/plan-<branch-name>.md`, load it — use it as the expected checklist

### Step 2 — Identify what changed on this branch
- Run `git diff main --name-only` (or `develop` if that's the base)
- Focus analysis only on changed files
- **Specifically flag any new files inside `Competition.Domain/Entities/`** — these are new entities

### Step 3 — Entity check (critical)
For every new `*.cs` file detected inside `Competition.Domain/Entities/`:
- Check if it exists in `.claude/docs/ENTITIES.md`
- If **missing from ENTITIES.md** → flag as a blocker: `ENTITIES.md not updated`
- This blocks `Ready for /feat-update: YES` — entity docs must be kept in sync

### Step 4 — Analyze changed files for RS1 compliance
1. **CQRS** — any controller injecting a service directly instead of `IMediator`? → critical violation
2. **FluentValidation** — every command/query has `AbstractValidator<T>` with real rules? Empty validators fail
3. **CancellationToken** — every `Handle()` signature includes it AND passes it to repository calls?
4. **EF Fluent API** — new entity configured in `AppDbContext.OnModelCreating`? Soft delete query filter added?
5. **DTOs** — domain entities never returned directly from handlers or controllers?
6. **Auth guard** — write endpoints have `[Authorize]`? FE routes have `admin-moderator.guard.ts`?
7. **Paging** — list handlers return `PagedResult<T>` not `IEnumerable<T>`?
8. **Tests** — any test files in the diff?
9. **BaseEntity** — new entities inherit `BaseEntity` for soft delete?

### Step 5 — Generate preview file

Write to `.claude/previews/<branch-name>.md`:

```markdown
# Preview — <branch name>
**Member:** <name>
**Branch:** <branch name>
**Date:** <today>
**Feature:** <feature name>
**Plan file:** <plan-<name>.md if exists, otherwise "no plan file">

---

## Entity Sync Check
| Entity | In Codebase | In ENTITIES.md | Status |
|---|---|---|---|
| <EntityName> | ✅ | ✅ / ❌ | OK / ⚠️ NEEDS UPDATE |

> ❌ Missing from ENTITIES.md blocks Ready for /feat-update.

---

## RS1 Compliance
| Check | Status | Location |
|---|---|---|
| CQRS via MediatR | ✅ / ❌ VIOLATION / ⚠️ Partial | `path/to/file.cs` |
| FluentValidation (real rules) | ✅ / ❌ Empty / ❌ Missing | `path/to/validator.cs` |
| CancellationToken in handler | ✅ / ❌ Missing | `path/to/handler.cs` |
| CancellationToken in repo calls | ✅ / ❌ Missing | `path/to/handler.cs` |
| EF Fluent API config | ✅ / ❌ Missing | `AppDbContext.cs` |
| Soft delete query filter | ✅ / ❌ Missing | `AppDbContext.cs` |
| BaseEntity inheritance | ✅ / ❌ Missing | `path/to/entity.cs` |
| DTO mapping (no domain entities) | ✅ / ❌ Missing | |
| Auth guard BE ([Authorize]) | ✅ / ❌ Missing | |
| Auth guard FE (route guard) | ✅ / ❌ / N/A | |
| Backend paging (PagedResult<T>) | ✅ / ❌ / N/A | |
| Tests | ✅ / ❌ Missing | |

> ❌ items block RS1 points. Fix before running /feat-update.

---

## Implementation Checklist
<if plan file exists: copy checklist from plan and mark each ✅ or ❌>
<if no plan file: derive checklist from RS1_REQUIREMENTS.md and feature doc>

---

## Points Earned This Branch
| Category | Item | Points |
|---|---|---|
| Basic / Advanced | <feature> | Xb |
| **Total this branch** | | **Xb** |

---

## Changed Files
**Domain**
- `...`
**Application**
- `...`
**Infrastructure**
- `...`
**API**
- `...`
**Frontend**
- `...`

---

## Issues Found
<list every RS1 violation, missing item, entity sync gap. Empty section = clean.>

---

## Ready for /feat-update?
YES — all RS1 checks pass and ENTITIES.md is in sync
NO — <list specific blockers>
```

### Step 6 — Print summary
```
Preview written to .claude/previews/<branch-name>.md
New entities detected: <list or "none">
ENTITIES.md in sync: YES / NO
Points this branch: Xb
Ready for /feat-update: YES / NO
```

---

## Usage

```
/feat-check
```
Always scopes to current branch automatically. No arguments needed.