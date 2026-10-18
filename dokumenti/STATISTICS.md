# Match statistics (COMP-62)

`Statistic` is the aggregate for one match's statistics. `TeamStatistic` stores
the counters for a selection within that aggregate. Both inherit BaseEntity,
including generated public IDs, timestamps and soft deletion.

There is at most one active Statistic per Match and one active TeamStatistic
per (Statistic, Selection), enforced by filtered unique indexes. Old soft-deleted
rows stay available for internal history. All foreign keys use Restrict.
Queries hide deleted aggregates, matches, team rows and selections.

Team counters are non-negative integers: TotalShots, ShotsOnTarget, TotalRedCards,
TotalYellowCards, Corners and BigChances. Possession is a whole percentage (0–100).
PostgreSQL checks also enforce shots on target <= total shots. ShotsOnTarget is
included because the linked COMP-66 form requires it.

COMP-63 owns the CQRS/API aggregate writes: require the match's two active distinct
home/away selections, save both teams in one unit of work, and require their
possession percentages to sum to 100. Those cross-row/domain rules are not expressible
as the row-level checks introduced here. COMP-66 owns the frontend comparison/form.
This change adds no endpoints, DTOs, seed data or live data migration.

PostgreSQL verification is reproducible with
`Competition.Tests/Sql/StatisticsConstraints.sql` after applying migrations to an
isolated test database. Run it through `psql` with `ON_ERROR_STOP`; its synthetic
fixtures are wrapped in a transaction and rolled back. The xUnit persistence tests
separately exercise public IDs, timestamps and soft-delete query filters with EF InMemory.
