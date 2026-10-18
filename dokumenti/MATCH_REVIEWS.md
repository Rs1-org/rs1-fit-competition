# Match reviews and reactions (COMP-52)

Open **Match reviews** from the public navbar. `/match-reviews` pages existing
matches from the Match API; `/match-reviews/:publicId` shows the match context,
average rating and paged reviews. The separate `MatchReviewPanel` can also be
embedded by COMP-65 with `[matchPublicId]`; this change does not implement the
full match management feature. Routes are lazy-loaded and the panel uses the
existing auth modal, confirm dialog, paging and regional formatting.

An active logged-in user can post one rating (integer 1–5) and nonblank comment
(1–2000 characters) per match. Only the author can edit or soft-delete it. Editing
recomputes the average; deletion excludes the review and its reactions from public
queries while keeping history. No reviews means a null average, displayed honestly.
Public author names are first name and last initial; emails and internal user IDs
are not included. Comments are rendered as text, not HTML.

Like/dislike is one active reaction per user/review. Selecting the same value again
removes it; selecting the other value switches it. Users react only to others'
reviews. Counts and viewer state come from SQL aggregation; the client reloads
after writes instead of inventing optimistic totals. Unsaved edits remain during
pagination/reaction reloads and failed saves; write controls prevent double submission.

| Endpoint | Access / body |
| --- | --- |
| `GET /api/reviews/match/{matchPublicId}?page=1&pageSize=10` | Public; average, count, paged reviews and caller's own review |
| `POST /api/reviews/match/{matchPublicId}` | Authenticated; `{ rating, comment }` |
| `PUT /api/reviews/{publicId}` | Owner; `{ rating, comment }` |
| `DELETE /api/reviews/{publicId}` | Owner |
| `PUT /api/reviews/{publicId}/reaction` | Authenticated other user; `{ value: 1 }` or `{ value: -1 }` |

The server obtains owner/viewer identity from the JWT, never a bound user ID.
Unknown/deleted matches and reviews return 404, invalid fields/duplicate reviews
400, missing/inactive identities 401 and ownership/self-reaction failures 403.
Filtered unique indexes enforce one active review/reaction; a concurrent uniqueness
conflict returns 409 with a reload instruction. All commands and queries propagate
CancellationToken and writes save once through the unit of work.

`AddMatchReviewsAndReactions` is additive, with BaseEntity public IDs/timestamps,
soft-delete filters, restrictive FKs and database checks for rating/reaction bounds.
It contains no demo reviews or production data. No paid dependency/service was added.
Browser inspection/screenshots remain unavailable in this session; build and
behavior tests do not establish responsive visual quality.
