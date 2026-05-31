# Points Tracker
> Updated by `/update` after each completed branch.
> Basic cap: **15b per member**. Advanced minimum: **30b**. Defense threshold: **40b total**.

---

## Scoreboard

| Member | Basic | Advanced | Total | Defense Ready |
|---|---|---|---|---|
| Muhamed | 15b | 10b | 25b | 🔴 Need 15b more advanced |
| Dino | 0b | 0b | 0b | 🔴 Need 40b |
| Edin | 0b | 0b | 0b | 🔴 Need 40b |

> 🟢 40b+ | 🟡 35b+ | 🔴 Not ready
>
> ⚠️ Edin's Player/Referee/Manager basic points are NOT counted yet —
> `GetAllManagersQueryHandler` returns `IEnumerable` not `PagedResult` (RS1 violation).
> Fix paging on Manager + Referee first, then those points unlock.
>
> ⚠️ Dino has 0 implemented entities — all points are blocked.

---

## Muhamed — Log

| Date | Branch | Feature | Points | Total |
|---|---|---|---|---|
| 2026-03-04 | feat-stadiums | Country + City + Stadium CRUD + filter + paging | +15b | 15b |
| 2026-05-31 | feature/comp-37/xUnit-test | Testing infrastructure (unit + integration tests) | +10b | 25b |
| 2026-05-31 | feature/comp-59/match-domain-entity-EF-migration | Match + MatchStatus domain entities + EF migration (mandatory, entity count 13→15) | +0b | 25b |
| 2026-05-31 | feature/comp-60/Match—CQRS-create/update/delete-fluent-validation | Match CQRS commands (Create/Update/Delete) + validators + IMatch repo + DTOs (prerequisite for advanced) | +0b | 25b |

---

## Dino — Log

| Date | Branch | Feature | Points | Total |
|---|---|---|---|---|
| — | — | Nothing implemented yet | — | 0b |

---

## Edin — Log

| Date | Branch | Feature | Points | Total |
|---|---|---|---|---|
| — | — | Player/Referee/Manager implemented but paging violation blocks points | — | 0b |

> Once `GetAllManagersQueryHandler` and `GetAllRefereesQueryHandler` return `PagedResult<T>`,
> run `/feat-req` on those branches and `/update` to log the points.