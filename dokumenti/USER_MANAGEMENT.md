# Admin user management (COMP-107)

Open **Manage → Users** or `/admin/users` as Admin. The route is lazy-loaded and
Admin-guarded; the API independently requires Admin on every endpoint.

The page provides server-side paging and five filters: name/email search, role,
active status, registered from, registered to. Dates use local calendar-day
boundaries in the browser and ISO offset timestamps at the API. Regional pipes
format the displayed dates/numbers. Loading, empty, error and retry are separate states.

The role selector assigns Admin, Moderator or User as requested by the ticket.
Existing Manager roles remain visible/filterable; they are not silently removed
or treated as Admin. Every role/status change requires confirmation. The current
account is labelled "Your account" and has no mutation controls; the handlers
also reject self changes. Failed saves retain the selected role and show API errors.

| Endpoint | Request / response |
| --- | --- |
| `GET /api/admin/users` | `PagedResult<UserListDto>` |
| `PUT /api/admin/users/{publicId}/role` | `{ "role": "Moderator" }` → `true` |
| `PUT /api/admin/users/{publicId}/active` | `{ "isActive": false }` → `true` |

List parameters: `page`, `pageSize` (1–100), `search` (max 256), `role`,
`isActive`, `registeredFrom`, `registeredTo`. Filters run in the database before
count/paging; role names for the page are loaded in one batch. DTOs contain public
IDs, names/email, roles, active status, nullable registration time and `isSelf`.
They do not expose password hashes, security stamps or internal user GUIDs.

Role/status changes stage updates and persist once through the unit of work.
Accounts are retained on deactivation; inactive users cannot log in or refresh.
Role changes replace the previous role membership, and both mutations revoke
refresh sessions and rotate the Identity security stamp. Newly issued JWTs carry
a security-stamp claim checked against the active account on authentication, so
those sessions lose access immediately after mutation. Tokens issued before this
change retain their existing expiry (at most the existing one-hour lifetime).
An already matching single role/status is an idempotent no-op.

The additive `AddUserManagementIdentifiers` migration creates a random unique
32-character public ID for each existing account. Existing auth response IDs and
JWT subjects stay unchanged. New users get `RegisteredAt` from the injected clock;
historical users retain null because their original creation date was not recorded.
The UI displays "Not recorded", and date filters exclude those unknown timestamps.
No passwords, email addresses or role assignments are rewritten by the migration.

Verification covers authorization, self protection, role/session changes,
deactivate/reactivate login, field validation, paging/filtering and unknown dates.
Angular tests cover filters, confirmation/cancellation, duplicate submission,
error/retry, payloads, route guard and hidden self controls. No browser was available
for a responsive visual review or screenshot; this remains a manual UI check.
