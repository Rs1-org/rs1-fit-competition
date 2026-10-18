# Free public DEV deployment

Prepared 2026-10-18. **Backend and frontend are deployed and public HTTP/authentication checks pass. GitHub deployment automation is supplied on a separate feature branch and requires activation.**
Public API: https://competition-dev-api.onrender.com
Public frontend: https://competition-dev-fejza.pages.dev
Current Pages deployment: https://7f578d63.competition-dev-fejza.pages.dev
Verified HTTP 200 for `/health`, `/health/ready` and the paginated selection endpoint.
The user's private image reference is `docker.io/dinofejzagic/competition-dev:dev-001`.
Pages assets have been rebuilt with the actual API origin; upload directory is `frontend/dist/pages/browser`.
Verified backend CORS origin is `https://competition-dev-fejza.pages.dev`, without a trailing slash.
The public smoke test passes API health/readiness, paged data, allowed/denied origins, bearer preflight,
SPA deep-route refresh, synthetic registration/login, refresh-token rotation, authenticated settings,
and rejection of revoked tokens. Evidence: ignored `out/deployment/public-smoke.log`.
The frontend root and deep Angular route both return HTTP 200, DEV noindex headers are active, and the
published API configuration bundle references the correct Render API.
Neon DEV database `competition-dev` is now initialized: 18 migrations applied, no pending migrations,
33 public tables including migration history, and all four application roles present. Both published sample
password hashes were disabled. Direct access was verified using full TLS certificate validation and the
ignored local `.env`; no credential values were copied into this document.
Work is on local branch `chore/free-dev-deployment`, based on the COMP-36 snapshot (`65d2f75`).
Publication is prepared separately on local branch `chore/dev-deploy-automation` from current `origin/main`,
so unrelated local guidance/Jira workflow changes remain outside the proposed deployment PR.
Existing unrelated workflow/guidance changes were preserved. This is a DEV setup, not a production certification.

## Team deployment through GitHub Actions

The `.github/workflows/dev-deploy.yml` checks PRs and deploys `main` after all backend/frontend
tests and builds pass. It publishes a private immutable Docker image, waits for the matching Render revision,
then uploads the verified Pages build and runs public HTTP smoke checks. Deployment stays disabled until the owner completes the activation steps.
Setup requires three repository secrets, a private-repository Actions billing limit, and the explicit
repository variable `DEV_CI_ENABLED=true`. Until enabled, jobs are skipped without allocating runners. Full team instructions,
secret names and first-deployment steps: [DEV_CICD.md](dokumenti/DEV_CICD.md).

## Repository inventory

| Area | Findings |
| --- | --- |
| Solution | `backend/CompetitionSolution/CompetitionSolution.sln` |
| Backend | Five .NET 10 projects: Api, Application, Domain, Infrastructure, Tests. ASP.NET Core, MediatR, FluentValidation, AutoMapper, Identity/JWT, NodaTime. |
| Database | PostgreSQL; Npgsql EF Core 10.0.3, EF tooling 10.0.12. 18 existing migrations, including COMP-13 Clubs and COMP-36 Selection.ClubId. No engine change or data migration to cloud performed. |
| Frontend | One Angular 21.2 application, TypeScript 5.9.2, RxJS/signals, Vitest/jsdom, static client rendering. No SSR server needed. |
| Containers | Root `docker-compose.yml`; legacy `backend/CompetitionSolution/compose.yaml`; API multi-stage Dockerfile and frontend nginx Dockerfile. |
| Local behavior | Native API HTTP 5244 / HTTPS 7208; Angular 4200. Root Compose API 8080, frontend 4200, PostgreSQL 5432. Existing nginx `/api` proxy and environment files are unchanged. |
| Public API | Controllers under `/api`; Swagger remains available at `/swagger` for DEV. Bearer JWT in Authorization; refresh tokens in PostgreSQL, browser tokens in localStorage. No cross-site auth cookies required. |
| Files | Existing Azure Blob Storage is still the default. An optional Supabase Free Storage adapter implements the same `IBlobStorageService` for new public DEV images. CSV/XLSX imports/exports use streams. No durable local upload store was introduced. |
| Other services | No registered background workers, queue, Redis, SMTP or active Stripe integration found. Role seeding runs at startup; configured DEV users are opt-in (COMP-84). |
| Browser dependencies | Google Fonts, cdnjs Font Awesome, unpkg Leaflet images and OpenStreetMap tiles; existing third-party availability/usage rules still apply. |
| Automation | Existing Jira workflow was not changed by this task; no cloud deployment workflow was added. |

## Selected hosting and cost limits

Only use free accounts/plans without payment methods, paid add-ons, trials or automatic upgrades. Check the account's current plan before creating anything. A free plan is quota-limited; it cannot guarantee permanent availability.

- **Render Free Web Service**, Docker, Frankfurt. Sleeps after 15 idle minutes; waking can take about a minute. 750 shared free instance-hours/month; disk is ephemeral and has no free persistent volume. No free one-off jobs or shell. Usage overages can be billed when a payment method exists; without one, services/builds can be suspended. Render may suspend excessive outbound traffic, including external DB traffic. Do not use keep-alive jobs. Render Free Postgres expires after 30 days, so it is unsuitable here. [Official free-service restrictions](https://render.com/docs/free).
- **Cloudflare Pages Free**, static assets only, Direct Upload. Git builds have 500 builds/month, one concurrent build, 20-minute timeout; free sites allow 20,000 files, 25 MiB/file. This setup uses no Functions, Workers, R2 or paid domain. [Limits](https://developers.cloudflare.com/pages/platform/limits/), [Direct Upload](https://developers.cloudflare.com/pages/get-started/direct-upload/).
- **Neon Free PostgreSQL**, preferably PostgreSQL 16 in the closest available EU region. Current published allowance: 1 GB/project, 100 CU-hours/project/month and 5 GB/project included public network transfer. Idle compute suspends after five minutes; storage persists. No time limit or credit card required. Keep the project on Free and use synthetic DEV data only. [Pricing and quotas](https://neon.com/pricing).
- **Private Docker Hub Personal repository** for the API runtime image when Git pushes are prohibited. Personal currently includes one private repository; builds run locally, without Build Cloud trials. Use an existing free account/available private repository slot. [Docker pricing](https://www.docker.com/pricing/).
- **Supabase Free Storage** for DEV images: 1 GB files, 5 GB egress plus 5 GB cached egress, up to two active free projects. Projects pause after one inactive week and must be resumed from the dashboard. Use a Free organization without paid add-ons or a payment method. Its unused database is not the application's database; existing EF data stays in Neon. [Supabase pricing](https://supabase.com/pricing/).

Render can run this API; Pages can serve the browser build. Neon uses the same PostgreSQL engine. The optional Supabase adapter provides a free image-storage path while retaining the existing architecture and Azure default. It streams uploads, returns public image URLs, and restricts deletes to its own bucket. No SDK/package was added. No existing images are migrated. Do not substitute Render disk. No Azure account or paid storage has been created.

## Configuration

Supply secrets through hosting secret settings or a process-local environment. Never put them in tracked files, command arguments, screenshots, logs, PRs or chat. Connection strings must never use browser environment files.

Required backend environment variable names:

| Name | Purpose |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Use Staging for public DEV, never Development. |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | Enable only behind Render's trusted TLS edge. |
| `ConnectionStrings__DefaultConnection` | Fresh DEV Neon database; Npgsql keyword-format connection string with certificate verification. |
| `Jwt__Key` | Independent cryptographically random signing secret, at least 32 characters. Render Blueprint can generate it. |
| `Cors__AllowedOrigins__0` | Exact HTTPS Pages origin, with no trailing slash/path or wildcard. Replaces the Staging placeholder. |

Optional backend variable names:

- `Cors__AllowedOrigins__1`, `Cors__AllowedOrigins__2`: approved localhost Angular origins only, if local-to-public API access is needed. Do not allow every Pages preview subdomain.
- `Jwt__Issuer`, `Jwt__Audience`: defaults come from Staging; retain consistent values across restarts.
- `AzureBlobStorage__ConnectionString`, `AzureBlobStorage__ContainerName`: only for an existing approved durable Azure account. The container must already exist, and uploaded URLs must have the intended read access. Optional for non-image endpoints.
- `Storage__Provider`: selects the optional Supabase provider for public DEV; omit it to retain existing Azure behavior.
- `SupabaseStorage__ProjectUrl`, `SupabaseStorage__ServiceRoleKey`, `SupabaseStorage__BucketName`: required when selecting Supabase. The legacy `service_role` JWT key is server-only and bypasses Storage RLS; never use the anonymous key or bundle this credential into Angular. Do not enable paid Azure storage for this zero-cost deployment.
- `AllowedHosts`: optionally restrict to the actual Render hostname.
- `PORT`: supplied by Render; API binds HTTP on all interfaces at that port. Existing launch profiles/Compose work without it.

Frontend build variable names:

- `PUBLIC_API_URL`: actual public HTTPS API **origin**, without `/api`, credentials, query or fragment. Public value bundled into JavaScript.
- `DEV_API_URL`, `DEV_FRONTEND_URL`: URLs used by the smoke-check script.
- `DEV_SMOKE_AUTH`: optional explicit opt-in to create a synthetic DEV account and exercise login/renewal; never use against production.

Account/tool authentication names, when using APIs instead of interactive login:

- `RENDER_API_KEY`
- `CLOUDFLARE_API_TOKEN`, `CLOUDFLARE_ACCOUNT_ID`
- `NEON_API_KEY`
- `SUPABASE_ACCESS_TOKEN`: project-management authorization for the optional Free image-storage project; distinct from the server's storage service-role key.

Cloudflare tokens need Account / Cloudflare Pages / Edit for the intended account. No zone/DNS permissions are needed for `pages.dev`. Provide registry credentials through Docker's credential helper and Render's registry credential settings.

### Proxy, HTTPS and CORS

Render terminates HTTPS; the container listens on HTTP. The ASP.NET forwarded-header middleware runs before application middleware. Its cloud opt-in trusts the hosting proxy, so enable it only behind an edge that overwrites forwarded headers. Do not turn this on for arbitrary direct internet hosting. No app-level HTTPS redirect was added, avoiding proxy redirect loops and preserving local HTTP.

The existing CORS policy uses exact `WithOrigins` matching, permits bearer/content-type preflight, and does not enable credential cookies. Allow the final stable Pages hostname, not an invented preview URL. If the Pages name is unavailable, use the hostname Cloudflare actually assigns and update CORS before testing.

## Build and verify locally

Use .NET 10 and a supported Node version (verified here with Node 24). PowerShell users can call `npm.cmd`; if needed, invoke the installed SDK at `$env:USERPROFILE/.dotnet/dotnet.exe`.

From the repository root:

```powershell
dotnet restore backend/CompetitionSolution/CompetitionSolution.sln
dotnet build backend/CompetitionSolution/CompetitionSolution.sln -c Release --no-restore
dotnet test backend/CompetitionSolution/Competition.Tests/Competition.Tests.csproj -c Release --no-build
docker build --platform linux/amd64 -t competition-dev:local -f backend/CompetitionSolution/Competition.Api/Dockerfile backend/CompetitionSolution
```

From `frontend/`:

```powershell
npm ci
npm run test:deployment
npm run build -- --configuration production
npm test -- --watch=false
# Set PUBLIC_API_URL in this process to the actual deployed API origin first.
npm run build:pages
```

Normal production output remains `frontend/dist/frontend/browser`. Pages output is **`frontend/dist/pages/browser`**, not the parent directory. The Pages script combines production optimizations/budgets with a dedicated environment replacement, deletes the generated source afterward and adds DEV `noindex` headers. Existing environments are untouched. A missing/non-HTTPS/local API origin fails the build. No localhost fallback is used for Pages.

Pages provides native SPA fallback when no top-level `404.html` exists; the build script checks this condition. Current Wrangler rejects a catch-all rewrite to `/index.html` as a loop, so no redundant `_redirects` file is emitted. Do not add a custom `404.html` without checking route refresh. [SPA behavior](https://developers.cloudflare.com/pages/configuration/serving-pages/).

## Database initialization and safe migrations

Create a **new, empty DEV-only** Neon database; do not import production data. Select PostgreSQL 16 if offered. The existing PostgreSQL migrations use standard tables/indexes and `gen_random_uuid()` supported by this version; no new extension or database-engine conversion is required.

Use Neon's **direct** connection endpoint for this single service, including startup DDL migrations. Convert the provided connection details to Npgsql keyword format in secret settings, with `SSL Mode=VerifyFull`, `Timeout=30`, `Command Timeout=60`, `Maximum Pool Size=10`, and `Minimum Pool Size=0`. Do not paste a `postgresql://` URL into this application's Npgsql configuration. Never disable certificate validation. No keepalive queries are needed.

The existing API applies EF migrations and creates roles **before listening**. This remains the deployment mechanism because Free Render has no one-off jobs. Use one instance and a dedicated database, and inspect migration logs before exposing the frontend. Existing migration history makes restarts idempotent. `/health` is liveness only; `/health/ready` explicitly checks DB connectivity. Configure Render probes to `/health`, avoiding continual Neon wakeups.

For future schema changes, review generated SQL before deployment. With compatible installed EF tooling:

```powershell
dotnet ef migrations script --idempotent --project backend/CompetitionSolution/Competition.Infrastructure --startup-project backend/CompetitionSolution/Competition.Api --output out/dev-migrations.sql
```

Back up any DEV data worth keeping with `pg_dump` before upgrades. Obtain connection credentials through a local PostgreSQL password file or environment, not command arguments. An image rollback does not undo database migrations. Never blindly run migration `Down` against a populated database.

### Public DEV accounts

An old migration inserts two privileged sample accounts with published passwords. Outside Development, startup now removes **only their exact original password hashes**, rotates their security stamps and removes their refresh tokens. Already rotated passwords and unrelated accounts remain unchanged. Local Development samples retain their existing behavior. No reusable public admin password was added. Never reuse a JWT key from a previously exposed environment.

Register your own synthetic DEV user through the application. In the DEV database's trusted SQL editor, assign Admin to that exact registered account after verifying its identity; replace the email placeholder before execution:

```sql
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT u."Id", r."Id"
FROM "AspNetUsers" u
JOIN "AspNetRoles" r ON r."NormalizedName" = 'ADMIN'
WHERE u."NormalizedEmail" = upper('<your-registered-dev-email>')
ON CONFLICT DO NOTHING;
```

Log out and log in again to obtain the role claim. This is a manual DEV bootstrap, not an anonymous elevation endpoint. Alternatively, initialize separate DEV/review logins using the opt-in [DataSeeder configuration](dokumenti/SEEDED_LOGINS.md). It does not change existing accounts or the manually provisioned DEV admin.

## Deployment without a source push

The core DEV services above are already deployed. The following manual procedure also supports redeployment without pushing application source; use the existing project names and skip resource creation.

1. Use free Render, Cloudflare and Neon accounts without payment methods. Authorize access to the intended accounts. The initial setup required account authorization; Cloudflare login and the manual Render/Neon setup have since completed. No connected Render/Cloudflare/Neon tool exists in this session.
2. Create the fresh Neon DEV project/database and store its connection details securely. Create a Cloudflare Pages Direct Upload project with a stable production branch/name; record its actual assigned hostname before setting backend CORS. For image features, create a Supabase Free project and a **public** `images-dev` bucket restricted to JPEG, PNG and WebP with a 6 MiB object limit. Do not add anonymous upload/delete policies: the backend uses its server-only service-role credential. Store only synthetic, non-private DEV images, because the bucket permits public reads. Set the Supabase variables above and select that provider. Keep the Azure default for existing local environments. Do not switch providers on a database containing real Azure images without a separately approved migration plan.
3. Build the local API image. Use an existing **private** Docker Hub Personal repository (one free private repository), login with `docker login`, tag and upload the compiled runtime image. Do not upload the repository or Docker build context. Verify private visibility before upload. These commands are intentionally separate from local builds and require account access:

```powershell
# Set IMAGE to the approved private registry path and a new immutable DEV tag.
docker tag competition-dev:local $env:IMAGE
docker push $env:IMAGE
```

4. In Render, New Web Service → Existing Image, select that private image, registry pull credentials, **Free**, Frankfurt. Set the backend variables above, the actual Pages origin, and `/health` as health-check path. No paid disk, database, pre-deploy job or worker. Record the real HTTPS API URL once healthy. [Prebuilt images and supported private registries](https://render.com/docs/deploying-an-image).
5. Set `PUBLIC_API_URL` to that actual API origin, run `npm run build:pages`, then deploy the **built assets only**:

```powershell
# From frontend/. Login opens an account authorization flow.
npx wrangler@4 login --device
npx wrangler@4 pages project create competition-dev --production-branch main --force
npx wrangler@4 pages deploy dist/pages/browser --project-name competition-dev --branch main --commit-dirty=true
```

Create the project only once (step 2); skip the create command on redeploy. Names shown are candidates, not existing resources. No Git integration is required. Direct Upload projects cannot later switch to Git integration without creating another project. The branch label selects the Pages production deployment; it does not push a Git branch.

For the actual project, use `--project-name competition-dev-fejza`. Wrangler 4.151.1 tried to delegate new
project creation to Workers; `--force` selected direct Pages creation. This was needed only for project
creation, not later deploys. An upload attempt hit `UND_ERR_CONNECT_TIMEOUT`; retrying uploaded all 88 assets
and produced the verified production deployment above. Device-code login succeeded after the normal OAuth
callback flow reported a CSRF mismatch.

6. Set `DEV_API_URL` and `DEV_FRONTEND_URL` to the real URLs and run:

```powershell
node scripts/smoke-deployment.mjs
```

This checks health/readiness, paged data, allowed/denied CORS, bearer preflight and SPA deep-route refresh. Set `DEV_SMOKE_AUTH` to true to additionally create one disposable synthetic user and verify registration/login, top-level refresh tokens, authenticated settings and rejection of a revoked refresh token. Credentials stay in memory and are never printed. Then inspect the browser Network panel and verify authenticated UI actions. Requests during a cold start may return the Render wakeup page; wait and retry user actions. No automatic write retries were introduced.

**Optional Git-based alternative:** `render.yaml` declares only a Free Docker service with manual deploys and required secret inputs. It is not applied and cannot pick up these local changes until a source push is explicitly authorized. Do not enable that path by silently pushing this branch. [Blueprint settings](https://render.com/docs/blueprint-spec).

## Redeploy, logs and troubleshooting

- Backend: repeat local tests/build, publish a new private immutable image tag, select it in Render and deploy. Keep the JWT key stable to preserve token validity. Recheck `/health/ready` and migration logs. Never select a paid plan to resolve a quota error.
- Frontend: set the actual API origin, build Pages assets and repeat the Wrangler deploy command. Rebuild after API hostname changes; changing a Cloudflare variable alone does not rewrite an already uploaded bundle.
- Render Dashboard → service → Logs/Events: check startup configuration, migration errors, listening port, memory and cold starts. Non-root runtime writes must stay temporary; application data belongs in PostgreSQL/object storage.
- Neon console: check project status, storage/compute/network quotas, database/role/hostname and TLS. A suspended compute may take time to wake. Do not poll readiness continuously.
- Cloudflare Pages → deployments: check the selected production branch, uploaded `browser` directory, absence of top-level `404.html`, and header file. Use `npx wrangler@4 pages deployment list --project-name competition-dev`. Static frontend request failures are inspected in browser DevTools, not backend deployment logs.
- CORS errors: compare the browser's exact Origin to the configured origin; check OPTIONS and Authorization headers. Do not use `*` or disable browser security.
- API returns HTML: check the built API URL and whether Render is waking; a Pages `/api` call means the wrong frontend build was uploaded.
- Image requests fail: verify the selected storage provider, public bucket, MIME/size limits, service-role credential and Supabase project status. Do not save uploads on Render disk or assume Neon storage implements Azure Blob APIs. Supabase redirects are deliberately rejected to avoid forwarding credentials to another host.

## Validation and outstanding issues

Evidence logs are in ignored `out/deployment/`; they contain no deployment credentials. Final checks/results are recorded there after completion.

- Backend restore/build and all **258 tests passed**, including anonymous health/revision, CORS preflight, sample-account protection, refresh-token contract/rotation and Supabase upload/delete/foreign-URL/error coverage. Storage HTTP contracts were tested with an in-process handler; no live Supabase upload was possible without account access.
- Linux Docker build passed. The non-root image ran under a 512 MiB memory limit on an assigned non-default port against a fresh, isolated PostgreSQL 16 verification database. All 18 migrations applied, both original sample password hashes were removed in Staging, and health/readiness, CORS, API paging, registration/login and authenticated settings passed. Container restart preserved the synthetic users and database schema. Existing local/main databases were not used for application verification.
- Wrangler's local Pages server served the Angular shell on a deep route. The combined local smoke script passed before and after API restart. This verifies static routing and API HTTP behavior separately; the validation bundle uses an unallocated example HTTPS API origin, so it does not establish browser-to-public-API communication.
- `npm ci` completed; the existing lockfile audit reports **39 vulnerabilities** (3 low, 12 moderate, 21 high, 3 critical). No unrelated package upgrades or blanket audit fixes were applied.
- Standard production and Pages builds passed with the existing font CSS budget and Leaflet CommonJS warnings. New deployment configuration tests: **2 passed**. Focused frontend renewal tests: **5 passed**.
- Current full Angular suite: **62 passed, 0 failed** across 33 files. The earlier 15 fixture failures were corrected as part of enabling strict CI: mock HTTP/router/animations, supply the dropdown's required control, register regional locale data, and update obsolete shell/format expectations. Application UI behavior was not changed by those fixture fixes.
- CI deploy orchestration: **6 tests passed**; the new Actions workflow passes actionlint. Docker build verifies the Git revision environment value. Cloud deployment through Actions requires three repository secrets and the explicit enable variable. Skipped jobs before activation do not constitute remote test validation.
- Deployment authentication validation exposed a refresh-contract defect, now corrected: the API returns top-level tokens as the frontend expects, expired requests replay with the renewed bearer token, concurrent callers share one rotation, and auth failures do not recursively trigger refresh. Revoked refresh tokens return 401. Regression tests cover these paths; existing roles and browser token storage remain unchanged.
- Final-image HTTP smoke checks passed with Supabase selected (using non-live configuration): registration/login, token renewal/rotation, authenticated settings and revoked-token rejection. No outgoing storage request was made with that non-live configuration. Runtime storage authentication still requires a real Free project.
- The public API and Pages deployment pass the combined HTTP smoke test, including the exact Pages-origin CORS and authentication/rotation checks. Frontend HTTPS, root/deep routes, noindex headers and the actual API bundle URL were verified. This is HTTP verification; rendered browser behavior and visual UI inspection remain unverified because computer-use inventory returned no browser/app sessions. A dedicated public cold-start test was not run. Wrangler is authenticated and Pages deployment succeeded.

Cloudflare deployment, Neon initialization, Render configuration and the public smoke test are complete. No additional login is needed to use the core DEV application. Supabase authorization or an existing Free project's storage settings are still needed to enable image features; live image upload has not been configured or verified. Store credentials through account authorization/secret settings or local environment, not in this document or chat.

The exact minimum account actions are:

1. Cloudflare login is complete. For a future login from `frontend/`, run `npx wrangler@4 login --device` and authorize the intended account, or securely supply a Pages Edit token and account ID.
2. In the intended Render Free workspace, create an API key and supply it securely using `RENDER_API_KEY`; confirm there is no payment method or paid upgrade.
3. Neon database initialization is completed. Use the existing ignored `.env` connection value in Render's secret settings; no further Neon authorization is required for this database.
4. Run `docker login` for an account with an available **private** Docker Hub Personal repository, and make read-only registry credentials available to Render.
5. For images, authorize a Supabase Free project or securely supply its project URL, public DEV bucket and server service-role key. This is optional only if image features can remain unavailable.

An ignored local `.env` is an alternative secure handoff location for these named credentials when a new shell's environment is not inherited by the agent. Do not commit it or paste its contents into chat. Test API/Pages processes created for verification are stopped after the checks; the isolated synthetic PostgreSQL database and local image remain for reproducibility.
