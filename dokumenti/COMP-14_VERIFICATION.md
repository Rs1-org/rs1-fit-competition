# COMP-14 Club acceptance verification

The implementation described by COMP-14 was delivered with COMP-13 in merged
PR #79. This ticket verifies and strengthens its coverage rather than merging the
historical `origin/COMP-13` branch again.

| Requirement | Existing implementation / evidence |
| --- | --- |
| Club merged to main, additive migration | PR #79; `20261018114239_AddClubs` |
| Fluent persistence, active unique slug, restrictive city/stadium FKs | Infrastructure `Data/AppDbContext.cs`; Domain `Entities/Club.cs` has no DB DataAnnotations |
| Backend paging, name/location/date/stadium/age filters and supported sort | `Repositories/ClubRepository.cs`, Application `Queries/Clubs/` |
| Required/limited name, valid age/date, active supplied references | `Commands/ClubCommands/ClubValidators.cs`, `ClubCommandValues.ApplyAsync` |
| Admin/Moderator create/edit, Admin delete | `Api/Controllers/ClubController.cs`; actual HTTP role cases in `IntegrationTests/ClubControllerTests.cs` |
| Soft delete and DTO/public-ID responses | `ClubRulesTests.FiltersAndPaging_AreAppliedBeforeCountingAndDeletedRowsAreHidden`, `CreateAndUpdate_PersistReferencesAndReturnOnlyPublicIdentifiers` |
| Request cancellation and one unit-of-work save | `ClubRulesTests.Create_PropagatesCancellationAndSavesOnce` |
| Lazy club list/detail/guarded form routes | `frontend/src/app/app.routes.ts` |
| Five-parameter filter and server paging | `pages/club-list/club-list.ts`, `.html`, `.spec.ts`; additional age and sort controls |
| Reactive Forms, matching validators, inline backend errors | `components/club-form/club-form.ts`, `club-validation.ts` |
| Admin-only delete button and named confirmation | `pages/club-list/club-list.html`, `ClubList.onDelete` |
| Regional date display, English UI | Club list/detail templates use `appDate` and `appNumber` |

Additional HTTP tests in this ticket create and rename clubs as both privileged
roles, verify slug changes retain the public ID, and verify only Admin deletion
removes the club from public detail. Existing tests cover anonymous write rejection,
invalid filter/date input, reference validation and backend filter/paging semantics.

The complete migration chain was verified with local PostgreSQL during the sequential
deployment/statistics tasks. No new Club migration is necessary. No source branch
is re-merged and no domain/UI redesign is introduced here.

Browser inventory was empty, so responsive visual inspection and screenshots remain
manual checks. This document does not claim a Jira transition, a completed screenshot
requirement or any independently assessed RS1 points.
