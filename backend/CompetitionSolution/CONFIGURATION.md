# Configuration per environment

## Backend (`Competition.Api`)

`ASPNETCORE_ENVIRONMENT` picks the file that is layered on top of `appsettings.json`:

| Environment | File | Secrets come from |
|---|---|---|
| Development | `appsettings.Development.json` (local dev values) | `dotnet user-secrets` |
| Staging | `appsettings.Staging.json` (placeholders) | environment variables |
| Production | `appsettings.Production.json` (placeholders) | environment variables |

Sections: `ConnectionStrings`, `Jwt`, `Cors`, `AzureBlobStorage`, `Stripe`, `Seed`. They are bound to the classes in
`Competition.Infrastructure/Settings/`. The API refuses to start without a connection string, a JWT key (32+ chars) and at least one CORS origin.

Free public DEV deployment is documented in the root `DEPLOYMENT.md`. The optional `Storage__Provider`
selects `Supabase`; the default remains `Azure` for existing environments. Supabase requires
`SupabaseStorage__ProjectUrl`, `SupabaseStorage__ServiceRoleKey` (server secret only), and
`SupabaseStorage__BucketName` for a pre-created public DEV image bucket. No existing images are migrated.
The provider is selected at startup; switching it is intended for a fresh DEV database.

Render supplies `PORT`; the API binds to that port on all interfaces. Enable
`ASPNETCORE_FORWARDEDHEADERS_ENABLED` only behind Render's trusted HTTPS edge. `/health` checks liveness;
`/health/ready` checks database connectivity. Outside Development, startup disables the exact published
sample account password hashes from the historical seed migration; rotated passwords are preserved.

Never commit real secrets. Local setup for image upload (from `Competition.Api/`):

```
dotnet user-secrets set "AzureBlobStorage:ConnectionString" "<connection string>"
```

Staging/Production environment variables (`:` becomes `__`):

```
ConnectionStrings__DefaultConnection
Jwt__Key
AzureBlobStorage__ConnectionString
Stripe__SecretKey
Stripe__PublishableKey
Stripe__WebhookSecret
Cors__AllowedOrigins__0
```

Notes:
- The CORS origins in `appsettings.Staging.json` / `appsettings.Production.json` are placeholders. A real deployment must set `Cors__AllowedOrigins__0` (startup only checks that the list is not empty). Development allows `http(s)://localhost:4200` only.
- `Stripe` and `Seed` settings are bound but not used yet; COMP-54 (Stripe) and COMP-84 (seeded logins) will read them.

## Frontend

| Command | Environment file |
|---|---|
| `ng serve` / `ng build --configuration development` | `environment.development.ts` |
| `ng build --configuration staging` | `environment.staging.ts` |
| `ng build` / `--configuration production` | `environment.prod.ts` |

Only public values go in these files (`apiUrl`, Stripe publishable key); they end up in the browser bundle.
