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
COMP-62 adds the schema without seed data or live data migration. The API is added separately by COMP-63 below.

## API (COMP-63)

Public read endpoints:

| Method and route | Result |
| --- | --- |
| `GET /api/statistic` | `PagedResult<StatisticDto>` |
| `GET /api/statistic/{publicId}` | Aggregate detail, 404 when absent/deleted |
| `GET /api/statistic/match/{matchPublicId}` | The match's active statistics, 404 when absent |

The list accepts `page` (1+), `pageSize` (1–100), `matchPublicId`,
`selectionPublicId`, `dateFrom`, `dateTo` (ISO timestamps with offset), and
`minTotalShots` (0+). Filters run in SQL before count/paging. Results sort by match
date descending, then public ID. All DTO identifiers are public IDs; internal GUIDs
and EF navigation graphs are not returned. Home/away contain selection names and
all seven values, or null if that team no longer has active statistics.

`POST /api/statistic` and `PUT /api/statistic/{publicId}` require Admin or Moderator.
Create takes `matchPublicId`, `home` and `away`; update takes `home` and `away`
and retains the original match. Both team objects use this shape:

```json
{"totalShots":12,"shotsOnTarget":5,"totalRedCards":0,"totalYellowCards":2,"corners":4,"bigChances":3,"possession":60}
```

Both objects are required; the possessions must sum to 100. Counters are
non-negative integers, shotsOnTarget <= totalShots, possession 0–100. The match
must have two distinct active selections. Existing statistics must be edited
rather than created again. Both teams are saved in one unit of work. If match
selections have changed, update soft-deletes obsolete rows and writes the current
two teams without modifying the match/selection records.

`DELETE /api/statistic/{publicId}` requires Admin and soft-deletes the aggregate
and its loaded team rows, retaining historical data. An unknown record returns 404;
validation failures use the shared 400 field-error contract. Every controller,
handler, lookup, repository query and unit-of-work save forwards request cancellation.

PostgreSQL verification is reproducible with
`Competition.Tests/Sql/StatisticsConstraints.sql` after applying migrations to an
isolated test database. Run it through `psql` with `ON_ERROR_STOP`; its synthetic
fixtures are wrapped in a transaction and rolled back. The xUnit persistence tests
separately exercise public IDs, timestamps and soft-delete query filters with EF InMemory.
