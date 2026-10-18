# dokumenti

Project documentation required by RS1 (diagrams, later also the ER diagram).

| File | What |
|---|---|
| `class-diagram.mmd` | Class diagram source (Mermaid). Edit this one. |
| `class-diagram.png` | Rendered diagram, exported from the `.mmd`. |
| [SEEDED_LOGINS.md](SEEDED_LOGINS.md) | Role login names and private DEV/review seeding configuration. |
| [USER_MANAGEMENT.md](USER_MANAGEMENT.md) | Admin account/role management, public identifiers and session revocation. |

## Colours

| Colour | Meaning |
|---|---|
| Grey | Entities from Sprints 1–3 |
| Green | New in Sprint 4 |
| Purple | ASP.NET Identity framework base classes |

Each sprint, recolour the previous sprint's entities grey and give the new ones the highlight colour (RS1: new entities in a different colour).

## Re-render the PNG

Uses the Mermaid CLI with the locally installed Edge, so Puppeteer does not need to download Chromium:

```bash
echo '{"executablePath":"C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe"}' > puppeteer.json
PUPPETEER_SKIP_DOWNLOAD=1 npx -y @mermaid-js/mermaid-cli@11.4.2 \
  -i dokumenti/class-diagram.mmd -o dokumenti/class-diagram.png \
  -p puppeteer.json -b white -s 2 -w 2400
```

The `.mmd` also previews directly on GitHub and in VS Code with a Mermaid extension.
