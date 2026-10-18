# Automatski DEV deploy nakon mergea

Workflow se isporučuje kroz deployment PR. Automatski deploy zahtijeva secrets i eksplicitno uključivanje opisano ispod.
Ne mijenja postojeći `.github/workflows/jira-ticket.yml`.
Provjere: 258 backend testova, 62 frontend testa, 6 testova deploy helpera i 2 testa Pages konfiguracije
prolaze; Docker i Pages build prolaze; actionlint potvrđuje workflow. Prethodne frontend greške u testnim
postavkama su ispravljene, bez preskakanja testova ili promjene ponašanja aplikacijskih ekrana.

## Kako tim radi

1. Developer napravi feature granu iz ažurnog `main`.
2. Push svoje grane i otvori pull request prema `main`.
3. Workflow **DEV checks and deployment** pokrene **Backend checks** i **Frontend checks**.
4. Drugi developer pregleda PR. Nakon prolaznih provjera, PR se mergea.
5. Promjena na `main` automatski pokrene provjere i posao **Deploy DEV**.
6. Actions objavi privatni Docker image, pokrene Render deploy i sačeka health potvrdu istog Git commita.
7. Objavi prethodno provjereni frontend build na postojeći Pages projekt i provjeri javnu integraciju.

Deploy se pokreće na svaki push na `main`, uključujući merge PR-a. Direktni push na `main` također bi ga
pokrenuo ako pravila repozitorija to dopuštaju. PR-ovi i druge grane ne dobijaju deployment tajne i ne
deployaju. Ručni **Run workflow** radi deploy samo kada je izabran `main`.

Svi developeri koriste isti DEV server:

- Frontend: https://competition-dev-fejza.pages.dev
- API: https://competition-dev-api.onrender.com
- Swagger: https://competition-dev-api.onrender.com/swagger

## Jednokratno podešavanje vlasnika repozitorija

Repozitorij: `Rs1-org/rs1-fit-competition`.
Otvori **Settings → Secrets and variables → Actions → New repository secret**.

Dodaj tačno ova tri secreta:

| Secret | Odakle uzeti vrijednost |
| --- | --- |
| `DOCKERHUB_TOKEN` | Docker račun `dinofejzagic` → Account settings → Personal access tokens. Novi CI token s Read & Write pravom za objavu u privatni `competition-dev` repozitorij. Render može zadržati zaseban Read token. |
| `RENDER_DEPLOY_HOOK_URL` | Render servis `competition-dev-api` → Settings → Deploy Hook. Kopirati cijeli URL direktno u GitHub secret. |
| `CLOUDFLARE_API_TOKEN` | Cloudflare profil → API Tokens → Create Token → Custom token. Account / Cloudflare Pages / Edit, ograničeno na račun koji sadrži `competition-dev-fejza`. |

Cloudflare Account ID, naziv Pages projekta, Docker Hub korisničko ime i javni URL-ovi već su u workflowu.
Nije potrebno davati drugim developerima Neon lozinku, Docker token ili Cloudflare token.
Ne kopirati lokalni Wrangler OAuth token u Actions; koristi zaseban ograničeni API token.
Neon connection string, JWT ključ i eventualni Supabase server ključ ostaju samo u Render environment
postavkama; nisu potrebni u GitHub Actions. Lokalni `.env` ostaje ignorisan.

Secrets moraju biti dostupni baš ovom repozitoriju. Organizacijski secret mora imati odobren pristup ovom
repozitoriju. Nedostajući secret prekida deployment s porukom koja navodi samo naziv, bez vrijednosti.

Nakon secrets i billing postavki, otvori **Settings → Secrets and variables → Actions → Variables** i dodaj
repository variable **`DEV_CI_ENABLED` = `true`**. To je jednokratno uključivanje cijelog workflowa. Dok ta
varijabla nije uključena, svi njegovi poslovi su **Skipped**, bez pokretanja runnera ili deploya. Ovo omogućava
objavu workflowa prije aktivacije, bez pretpostavke da je privatna Actions kvota/billing već podešen.

## Besplatna kvota i troškovi

Repozitorij je **privatan**. Standardni `ubuntu-latest` runner koristi uključene Actions minute organizacije;
to nije neograničeno besplatno. Prije aktivacije provjeri GitHub organizacija → **Settings → Billing & licensing
→ Budgets and alerts** i ograniči Actions potrošnju uz **Stop usage when budget limit is reached**, tako da nema
odobrene potrošnje izvan uključene besplatne kvote. Ako organizacijska postavka nije dostupna, vlasnik
organizacije treba provjeriti billing; ne pretpostavljati da su plaćeni overageovi isključeni.

Workflow koristi standardni Linux runner, bez većih/plaćenih runnera, bez plaćenih servisa i bez posebnog
GitHub deployment environmenta. Samo frontend build s `main` čuva se kao mali artifact, uz retention od jednog
dana; PR buildovi ne spremaju deployment artifacte. Docker build radi na runneru, ne na Docker Build Cloud trialu.
Render i Supabase planovi ostaju Free; ovaj workflow ne kreira cloud resurse i ne mijenja planove.
Ako se besplatna kvota potroši, sačekati reset; ne uključivati plaćeni plan kao automatsko rješenje.

Službeni izvori: [Actions billing](https://docs.github.com/en/billing/concepts/product-billing/github-actions),
[budgets](https://docs.github.com/en/billing/how-tos/set-up-budgets),
[Render deploy hooks](https://render.com/docs/deploy-hooks),
[Pages CI upload](https://developers.cloudflare.com/pages/how-to/use-direct-upload-with-continuous-integration/).

## Prva objava workflowa

Pripremljeni workflow zavisi od lokalnih deployment izmjena: Docker PORT/revision podrške, health endpointa,
Pages build skripte, smoke skripte i popravljenih testnih postavki. Sve te task-owned datoteke trebaju biti u
istom deployment PR-u. COMP-36 je već mergean u `main`.

Za aktivaciju nakon objave deployment PR-a:

1. Postavi tri secreta, provjeri ograničenje billing potrošnje i tek zatim uključi `DEV_CI_ENABLED=true`.
2. Pregledaj deployment PR i mergeaj ga u `main`.
3. Otvori **Actions → DEV checks and deployment** i prati **Deploy DEV**.
4. Prvi CI run objavit će tagove `sha-<git-sha>` i `dev` u privatni Docker Hub repozitorij.
5. Tek kada `dev` tag postoji, u Render servisu promijeni podrazumijevani Image URL na:

   ```text
   docker.io/dinofejzagic/competition-dev:dev
   ```

   Sačuvaj postavku i provjeri deploy. Zadrži postojeće privatne registry credentials i Free plan.

Render hook za svaki CI deploy koristi tačan `sha-...` tag. Podrazumijevani `dev` tag je potreban da kasniji
ručni restart ili promjena environment varijabli ne vrati stari ručno objavljeni `dev-001` image. Nemoj
postavljati `:dev` prije nego što ga prvi workflow objavi. Image repository/namespace na Renderu mora ostati
isti kao u workflowu; Render dopušta override taga/digesta, ne proizvoljnog drugog repozitorija.

## Šta se provjerava

- .NET 10 restore, Release build i cijeli backend test suite. Testovi ne dobijaju cloud database credentials.
- Node 24, `npm ci`, deployment skripte, cijeli Angular test suite i optimizovani Pages build.
- Nema `continue-on-error` za testove: pad zaustavlja deploy.
- Backend image dobija `APP_REVISION=<git-sha>`; `/health` vraća `revision`.
- Render mora vratiti isti SHA i uspješan `/health/ready` prije objave frontenda.
- Javni smoke test provjerava health/readiness, paginaciju, dozvoljeni/nedozvoljeni CORS, bearer preflight
  i osvježavanje Angular rute. Ne kreira korisničke račune pri svakom mergeu.
- Automatske migracije ostaju dio backend startupa na jednoj DEV instanci. GitHub ne spaja direktno na Neon.

Main deployevi su serijalizovani. Aktivni deploy se ne prekida zbog novog mergea; GitHub može zamijeniti
čekajući run novijim. U konačnici se objavljuje novija promjena nakon završenog aktivnog runa. Backend/frontend
objava nije atomska transakcija: tokom prelaska stari frontend može kratko raditi s novim API-jem. Promjene API
ugovora planirati kompatibilno.

## Kako dijagnosticirati grešku

**Actions → run → neuspjeli job/step** sadrži build/test poruku. Tajne se ne ispisuju.

- **Missing repository secret:** dodati odgovarajući secret pa ponoviti run.
- **Docker unauthorized/denied:** token mora imati Write i pristup repozitoriju računa `dinofejzagic`.
- **Render 401:** provjeriti/regenerisati deploy hook i ažurirati secret.
- **Render 400:** provjeriti da Image URL na servisu koristi isti `docker.io/dinofejzagic/competition-dev`.
- **Render revision timeout:** provjeriti Render Logs/Events, registry pull credentials i migracije. Stari
  zdravi server ne smatra se uspješnim novim deployem. Ne uklanjati provjeru revizije da bi job postao zelen.
- **Cloudflare unauthorized:** Pages Edit token mora biti za tačan račun; bez DNS/Workers/R2 dozvola.
- **Upload timeout:** ponoviti neuspjeli deploy; prethodno uploadane datoteke se mogu ponovo iskoristiti.
- **Artifact expired:** pokrenuti cijeli workflow ponovo na `main`, jer artifact traje jedan dan.
- **CORS fail:** Render `Cors__AllowedOrigins__0` mora biti tačno `https://competition-dev-fejza.pages.dev`, bez `/` na kraju.

Može se koristiti **Run workflow → main** za novi deploy trenutnog `main`. Ponovno izvršavanje starog runa
deploya njegov stari SHA; to koristiti samo kada je namjeran rollback i shema baze je kompatibilna.
Rollback imagea ne vraća EF migracije niti podatke. Ne brisati korištene image tagove dok su potrebni za rollback.

## Pravila za merge

Dogovor tima: feature grane, PR, nezavisni pregled i prolazni **Backend checks** / **Frontend checks** prije
mergea. Ako trenutni GitHub plan podržava tražena branch pravila za ovaj privatni repo, vlasnik može te
provjere postaviti kao required checks na `main`. Ovaj zadatak ne mijenja branch protections, identitete,
plan organizacije ni ovlasti developera.

Repozitorijsku administraciju i secrets podešava vlasnik. Ostalim developerima je dovoljan odgovarajući
repo pristup za grane/PR i dogovorena merge prava; nisu im potrebni pojedinačni hosting računi za svaki deploy.
