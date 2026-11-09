# Database backup

`competition-2026-11-09.zip` contains `competition.sql`, a plain SQL dump (`pg_dump --no-owner --no-privileges`) of a
PostgreSQL 16 database. It was taken from `main` at commit `68566f8` and includes all EF Core migrations up to then.

What is in it:

- Demo data from the startup seeder: countries, cities, stadiums with map coordinates, the Premijer liga BiH
  competition with seasons and rounds, demo teams with squads, formations and history, and their matches.
- One finished match with the full matchday data, for the flow and the PDF report: FK Sarajevo vs FK Tuzla City
  (19.07.2025, 3:0). It has a referee, both lineups with formations (4-3-3 and 4-4-2), goals, cards, a substitution
  and team statistics. Other matches have only their result.
- One login per role (see below). Passwords are stored only as ASP.NET Identity hashes.
- `user@competition.ba` has an active premium subscription until 2027-11-08, so the advanced statistics
  (team form, head-to-head) are unlocked for that account. Premium was granted directly in the database for the
  review, not through a Stripe payment, so `PaymentTransactions` is empty.
- No refresh tokens, no Stripe keys, no storage keys.

## Logins

| Role | Email | Premium |
| --- | --- | --- |
| Admin | admin@competition.ba | no (sees the paywall) |
| Moderator | moderator@competition.ba | no |
| Manager | manager@competition.ba | no |
| User | user@competition.ba | yes, until 2027-11-08 |

The passwords are not stored in the repository. They are sent privately with the review form (section 2).
The two legacy sample accounts from migration `SeedTestAdminAndModerator` (`admin@fit.local`,
`moderator@fit.local`) have no password in this backup and cannot log in.

## Restore with Docker (recommended)

From the repository root, after unzipping `competition-2026-11-09.zip` into `db-backups/`:

```
docker compose up -d db
docker compose exec db psql -U postgres -c "DROP DATABASE IF EXISTS competition" -c "CREATE DATABASE competition"
docker compose cp db-backups/competition.sql db:/tmp/competition.sql
docker compose exec db psql -U postgres -d competition -v ON_ERROR_STOP=1 -f /tmp/competition.sql
docker compose up -d backend frontend
```

The app is then available at http://localhost:4200 (API at http://localhost:8080).

## Restore into a local PostgreSQL

```
createdb -U postgres competition
psql -U postgres -d competition -v ON_ERROR_STOP=1 -f db-backups/competition.sql
```

`appsettings.Development.json` already points to `Host=localhost;Database=competition;Username=postgres`, so the
API started with `dotnet run --launch-profile http` (from `backend/CompetitionSolution/Competition.Api`) uses it
without extra configuration. Leave `Seed:Enabled` off; the accounts are already in the backup.

On startup the API applies any migrations newer than the backup, so the backup keeps working after later merges.

## Not configured in the backup

Stripe payments and image uploads need keys that are not in the repository (see
`backend/CompetitionSolution/CONFIGURATION.md`). Without them the app runs normally: the Stripe checkout returns
"Payments are not configured." and image uploads fail. Records can still be created and edited without a picture
(the seeded data has none).
