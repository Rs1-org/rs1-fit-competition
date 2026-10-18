# Statistics frontend (COMP-66)

Visitors can open `/statistics` and `/match-statistics/:publicId`. The directory
uses COMP-63's paged API, with five server filters: match, selection, match date
from/to and minimum shots for either team. The existing match picker is reused
with paging; team choices come from the selected match's existing detail endpoint.
Date inputs use browser local time and are converted to ISO instants for the API,
consistent with existing match-date display behavior.

`MatchStatisticsPanel` loads recorded statistics for a match and displays both
teams through `StatisticComparison`. It is embedded in the existing match-review
detail page and the dedicated match-statistics page. Numbers use `appNumber`;
dates use `appDateTime`. A recorded zero remains zero; missing team data is
labelled as missing. Only a statistics 404 becomes a no-records state. Other API
failures remain visible with a retry action.

Admin/Moderator can use `/manage-statistics/:matchPublicId`, protected by the
existing management guard. It loads the match and any existing statistics,
creates if none exist and updates by statistic public ID otherwise. Update
payloads contain only `home`/`away` values; create also includes `matchPublicId`.
The backend continues to enforce authorization and saves both teams atomically.

The Reactive Form requires all seven integer values for each team, with no
fabricated initial statistics. Counters are non-negative Int32 values, possession
is 0–100, shots on target cannot exceed total shots, and possession totals 100.
Group validators enforce the last two rules. Server field errors appear beside
their fields; errors retain input, and duplicate submissions are blocked. A failed
statistics request does not enable a new-record save. Matches need two distinct
active selections; the backend rechecks these references when saving.

No new endpoint, database migration, package or paid resource is introduced.
The existing Match detail DTO already exposes selection public IDs; its TypeScript
detail type now reflects those fields. Existing review/match API routes are kept.

## Integration and evidence

COMP-65's complete match-detail/manage page remains owned by its assigned developer
and is absent from this checkout. The existing review detail now shows statistics,
and the reusable panel can be embedded in COMP-65 once that page exists. This
change does not claim completion of COMP-65, COMP-98 or report tickets. No RS1
points, authorship, Jira status/estimate or screenshot evidence is fabricated.

Behavior tests cover required numbers, negative/fractional/out-of-range values,
cross-field rules, atomic create/update payloads, backend field errors, retained
drafts, duplicate saves, filters/paging, failed-load retry, missing-data display,
regional number rendering and guarded access. Production compilation checks strict
templates. Browser surfaces are unavailable in this session: mobile, theme,
keyboard and zoom checks and screenshots have not been performed.
