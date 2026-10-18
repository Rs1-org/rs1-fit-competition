# DEV and review logins (COMP-84)

`Admin` is the existing highest-privilege role. It represents the "Super Admin"
wording in COMP-22; no new role or permission inheritance is introduced. Backend
authorization attributes and frontend role checks retain their existing names.
COMP-22's Jira wording still needs editorial alignment; this change does not edit Jira.

`DataSeeder` creates missing roles on every startup after migrations. Account
seeding is disabled by default, including in Development. Enable it only for an
intended DEV/review database. Historical migration accounts are separate; published
historical passwords remain disabled outside Development.

| Role | Login | Password configuration |
| --- | --- | --- |
| Admin | admin@competition.ba | `Seed:AdminPassword` |
| Moderator | moderator@competition.ba | `Seed:ModeratorPassword` |
| Manager | manager@competition.ba | `Seed:ManagerPassword` |
| User | user@competition.ba | `Seed:UserPassword` |

Set passwords privately with `dotnet user-secrets` from
`backend/CompetitionSolution/Competition.Api`:

```powershell
dotnet user-secrets set "Seed:AdminPassword" "<private strong password>"
dotnet user-secrets set "Seed:ModeratorPassword" "<private strong password>"
dotnet user-secrets set "Seed:ManagerPassword" "<private strong password>"
dotnet user-secrets set "Seed:UserPassword" "<private strong password>"
dotnet user-secrets set "Seed:Enabled" "true"
dotnet run --launch-profile http
```

Use distinct passwords meeting ASP.NET Identity's configured password policy.
On Render use `Seed__Enabled`, `Seed__AdminPassword`, `Seed__ModeratorPassword`,
`Seed__ManagerPassword`, `Seed__UserPassword`. Do not put credentials in tracked
appsettings files, frontend bundles, Actions logs, screenshots or Jira. Share actual
review credentials privately with the review staff; the table above records names only.
Disable `Seed__Enabled` after initialization. Password changes use Identity's normal
reset flow, never repeated seeding. `Seed:SuperAdminPassword` is a legacy unused setting.

Running twice does not duplicate users/roles or overwrite passwords, active flags
or existing role membership. A seed email already belonging to an account without
the expected role stops seeding with a configuration error; resolve that collision
manually rather than silently elevating the existing user. Missing/invalid passwords
fail before any seed users are created. Domain demo data belongs to COMP-44/COMP-112;
new domain seed methods can extend `DataSeeder.SeedAsync` in foreign-key order.

No database migration is required: this uses the existing Identity schema.
