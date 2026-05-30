# Points Tracker
> Updated by `/update` after each completed branch.
> Basic cap: **15b per member**. Advanced minimum: **30b**. Defense threshold: **40b total**.

---

## Scoreboard

| Member | Basic | Advanced | Total | Defense Ready |
|---|---|---|---|---|
| Muhamed | 15b | 0b | 15b | 🔴 Need 25b more advanced |
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