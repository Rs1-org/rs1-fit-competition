# Competition: Design Guide

How the **Competition** web app is designed, so any new page or feature (calendar, user admin, staff forms, settings, notifications…) can be built to look like it was always part of the app.

The reference is the current design canvas: Home, Live, Match list, Match detail, Season, Team, Player, Stadium, Charts, Event entry, Paywall, the two mobile screens, the Component library and the shared Navbar.

---

## 1. Design principles

1. **Everything revolves around the match.** Scores, minutes and match status are the most important things on any screen, and they always look the same.
2. **Numbers are the heroes.** Scores, minutes, stats and prices use a bold, condensed font with tabular figures.
3. **Data-dense but calm.** Lots of information in tidy cards, plus neutral greys. Colour is used only when it carries meaning.
4. **Team crests bring the colour.** The interface stays neutral; club colours appear only in crests and team areas.
5. **One app for every role.** Visitors, fans, moderators and admins use the same pages. Staff tools appear inline, only for the right role.
6. **Every screen has every state:** light and dark, loading, empty and error, plus every live status where a match is involved.

---

## 2. Typography

| Font | Used for | Sizes |
|---|---|---|
| **Rajdhani** (500/600/700) | Page titles, section titles, scores, minutes, numbers, badges | Page title 40px · section 20–24px · hero score 76px · card score 30px |
| **Barlow** (400–700) | Body text, labels, forms, reviews, descriptions | Body 15px · labels 13px · meta 12–13px |

- All numbers use tabular figures, so scores and minutes don't jump when they change.
- Small labels (badges, section labels, table headers) may be uppercase, at 12–13px with letter-spacing of 0.06–0.12em. Everything else is in sentence case.

Google Fonts link:
```html
<link href="https://fonts.googleapis.com/css2?family=Barlow:wght@400;500;600;700&family=Rajdhani:wght@500;600;700&display=swap" rel="stylesheet">
```

---

## 3. Colour tokens

Light and dark are both first-class. Every screen has a theme switch.

| Token | Light | Dark | Meaning |
|---|---|---|---|
| `--bg` | `#f4f7f8` | `#0c1218` | Page background |
| `--bg2` | `#ffffff` | `#141c26` | Secondary surface, table headers |
| `--card` | `#ffffff` | `#1a2430` | Cards |
| `--field` | `#f7faf9` | `#101820` | Inputs, stat tiles, row hover |
| `--tx` / `--tx2` / `--mut` | `#17202a` / `#3d4c5c` / `#5c6d7f` | `#f3f7fb` / `#c5d0dc` / `#8b9aab` | Primary / secondary / muted text |
| `--bd` | `#d8e0e6` | `rgba(255,255,255,.1)` | Borders |
| `--ac` | `#0f9f82` | `#3ee0b4` | Brand accent: positive, selected |
| `--actx` / `--btn` | `#0b7d66` | `#3ee0b4` | Accent for text and buttons (contrast-safe) |
| `--acs` | `rgba(15,159,130,.12)` | `rgba(62,224,180,.14)` | Accent tint backgrounds |
| `--live` / `--livetx` | `#e74c3c` / `#bf2b1f` | `#e74c3c` / `#ff7a6c` | **Live only**: pulsing dot, live minute |
| `--dg` | `#c81f43` | `#ff6b86` | Danger: losses, red cards, delete, errors |
| `--amb` | `#8f5f00` | `#f0b429` | Half-time, premium, warnings |
| `--yc` | `#f2c230` | `#f2c230` | Yellow card icon only |
| Card shadow | `0 8px 24px rgba(23,32,42,.06)` | `0 12px 32px rgba(0,0,0,.32)` | |

**Supporting tokens** (not part of the original design, added for states the table does not cover)

| Token | Light | Dark | Meaning |
|---|---|---|---|
| `--btn-hover` | `#096a57` | `#2fcba1` | Hover on filled buttons |
| `--btn-tx` | `#ffffff` | `#06281f` | Text on filled buttons |
| `--dg-hover` / `--dg-tx` | `#a81938` / `#ffffff` | `#ff8aa0` / `#2a0710` | Hover and text on danger buttons |
| `--dgs` | `rgba(200,31,67,.1)` | `rgba(255,107,134,.12)` | Danger tint backgrounds |
| `--ambs` | `rgba(143,95,0,.12)` | `rgba(240,180,41,.14)` | Amber tint backgrounds |
| `--overlay` / `--overlay-strong` | `rgba(5,8,12,.6)` / `rgba(5,8,12,.9)` | same | Modal backdrops, image viewer |
| `--on-img` / `--on-img-bd` | `#f3f7fb` / `rgba(243,247,251,.4)` | same | Text and borders on photos and the hero band |
| `--hero` | `linear-gradient(135deg,#0c3b32,#0b7d66)` | same | Dark header band |
| `--route-a` / `--route-b` | `#dc2626` / `#2563eb` | same | Map marker / route line |
| `--shadow-lg` | `0 20px 60px rgba(0,0,0,.4)` | same | Modals |

Shape and fonts have tokens too: `--r-card`, `--r-ctl`, `--r-pill`, `--font-body`, `--font-title`, `--container`, `--page-pad`, `--section-gap`. All tokens live in `frontend/src/styles.css`.

**Colour rules**
- Teal is for brand, positive and selected states. Red means live or negative. Everything else is neutral grey.
- For text and filled buttons, use the darker `--actx` / `--btn`. Plain `#0f9f82` on white fails contrast.
- Colour is never the only signal: statuses carry text, results carry the letters W/D/L, and events carry an icon plus a label.

---

## 4. Shape, spacing, icons

- **Radius:** cards 8px, buttons and inputs 5px, badges and chips fully rounded (pill).
- **Cards:** 1px border plus a soft shadow.
- **Touch targets:** at least 44px. Toolbar buttons may be 40px.
- **Page container:** max width 1200px, 24px side padding, 20–28px gaps between sections.
- **Breakpoints:** desktop above 1180px, tablet `max-width: 1180px`, mobile `max-width: 768px`. Use no others.
- **Icons:** simple inline line icons (2px stroke, round caps), through the shared `app-icon` component. Never emoji.
- **Crests:** a shield-shaped placeholder in the club colour with its 3-letter code (SAR, ZEL…). No real logos.

**Event icon set**

| Event | Icon |
|---|---|
| Goal | Ball outline with a filled pentagon, in accent |
| Own goal | The same ball in danger red |
| Penalty scored / missed | Ball with a "P" chip / grey ball with a red "×" chip |
| Yellow / red card | Small rounded card, tilted 8° |
| Substitution | Up arrow in accent (player in) and down arrow in red (player out) |

---

## 5. Page anatomy

Every desktop page follows the same skeleton:

```
Navbar (shared component)
└─ Container (max 1200px)
   ├─ Breadcrumb (deep pages): Competitions › Premijer liga BiH › 2025/26 › Round 12
   ├─ Page header: title + subtitle, primary staff action on the right
   │  or an entity header card (crest/photo, name, facts, stat tiles, Follow/Edit)
   └─ Content: main column + side column (stacks on small screens)
```

- **Navbar:** logo, primary sections (Home, Live with count, Matches, Competitions, Teams, People, Statistics), search, theme toggle, favourites, notifications, a Manage button for staff, and an account menu (or Log in for visitors).
- **Main column:** the page's real content in section cards.
- **Side column:** context cards such as a mini league table, manager, fixtures, top scorers and quick facts.
- **Mobile (390 × 844):** compact top bar, scrolling content, bottom tab bar (Home, Live, Matches, Competitions, More). Long team names fall back to the 3-letter code.
- **Staff tablet tools (1180 × 820):** big touch targets, fast entry, and a live timeline beside the form.

---

## 6. Reusable patterns

When building something new, start from the closest existing pattern:

| Need | Reference screen | Pattern |
|---|---|---|
| Match row | Home, Match list | Status (time / live minute / HT / FT / PPD) · home name + crest · score or "vs" · away crest + name · follow heart. The winner is bold |
| Live card | Home, Live | Competition label, `LIVE 67'` pill with pulsing dot, two team rows with big scores, last event and HT score |
| Status badge | Component library | Scheduled = teal tint · Live = red tint + pulse · Half-time = amber · FT = grey · Postponed / Cancelled = red tint |
| List page | Match list | Title + staff action → filter card (up to 5 filters, active chips, Reset/Apply) → quick tabs + import/export for staff → results → pagination ("Showing 11–20 of 134") |
| Data table | Season, Player | Light header row with small uppercase labels, hairline row separators, numbers right-aligned in Rajdhani |
| Tabs | Match detail | Underline tabs with a 3px accent bar; optional count chip or lock icon |
| Two-sided timeline | Match detail | Grouped by match part; home events on the left, away on the right, minute pill in the middle |
| Stat comparison | Match detail | Values on both ends (leader bold), label centred, bars filling outward from the middle |
| Poll | Match detail | Options as bars that fill with the vote %, a check on your vote, countdown, login prompt for visitors |
| Reviews | Match detail | Average + distribution, write form with stars, review cards with like/dislike |
| Charts | Charts, Player | Simple SVG charts with light gridlines, Rajdhani labels and the legend below |
| Map | Stadium | Map panel with a pin; staff picker mode adds latitude/longitude fields |
| Fast staff entry | Event entry | Big quick-action tiles, toggles, steppers, player grid, live-updating timeline, toast on save |
| Premium lock | Paywall | Blurred content behind a centred card: PREMIUM pill, 3 benefits, price, call-to-action button |
| Feedback | Component library | Login modal, confirm dialog, toasts, notifications dropdown |

If a new feature needs something that isn't listed, build it from the same parts (cards, tokens, Rajdhani numbers, pill badges, 44px controls) and add it to the Component library.

---

## 7. Roles

| Role | Sees |
|---|---|
| Visitor | Everything public; sign-up prompts instead of personal features |
| Fan | + voting, reviews, favourites, notifications, "Your teams" |
| Moderator | + inline staff actions: edit, add event, enter statistics, manage polls, import/export |
| Admin | + delete, reports, user management |

Rules:
- Staff actions live **inside** the normal pages, never on a separate admin copy.
- Visitors never see disabled admin buttons; the buttons are simply absent.
- Destructive actions always go through a confirmation dialog: "Delete FK Sarajevo? This cannot be undone."

---

## 8. States every screen needs

- **Loading:** skeleton rows shaped like the real rows, gently shimmering. No spinners on their own.
- **Empty:** icon, short title, one helpful sentence and an action (staff only where relevant), e.g. "No matches in this round yet. Schedule one."
- **Error:** message plus a Retry button.
- **Success:** a toast after saving or deleting.
- **Forms:** inline field errors in red under the field, and a disabled "Saving…" button while saving.
- **Match statuses:** Scheduled (countdown), Live (pulsing + minute), Half-time, Finished, Postponed, Cancelled.
- **Themes:** light and dark.

---

## 9. Motion

Motion only where something changed or is live:
- Pulsing red dot on live matches.
- "Just updated" flash on a score change, and a goal banner or animation.
- Poll bars grow when votes arrive.
- A short drop animation on the map pin, and toasts sliding in.

---

## 10. Content and formats

- Demo data uses the Bosnian league: Premijer liga BiH 2025/26, FK Sarajevo, FK Željezničar, Stadion Asim Ferhatović Hase (Koševo). Players and managers are invented; reuse the same names across pages so they stay consistent.
- Dates: `29.10.2026 18:00`, or relative ("Today", "in 2 h").
- Numbers: thousands with dots (`34.500`). Money: `4,99 KM` or `€ 1,2 M`.
- Minutes: `67'`, `45+2'`. Scores: `2 – 1`; `vs` before kick-off.
- UI text in English. Buttons are short verbs ("Schedule match", "Post review").
- No lorem ipsum. Missing images or facts become labelled placeholders such as `[PLAYER PHOTO]`.

---

## 11. Accessibility

- Use real buttons, links, form labels and tables with header cells.
- Icon-only buttons have a text label for screen readers.
- Text contrast is at least 4.5:1 in both themes.
- Visible focus ring: 2px accent outline.
- Live score updates are announced politely to screen readers.
- Charts have a text description of their data.

---

## 12. Checklist for a new page

- [ ] Uses the shared navbar, the 1200px container and a breadcrumb if it's a deep page
- [ ] Rajdhani for titles and numbers, Barlow for text; colours only from the token table
- [ ] Built from existing patterns (section 6) where possible
- [ ] Light and dark versions
- [ ] Role variants: staff actions inline and hidden from visitors and fans
- [ ] Loading, empty and error states (plus match statuses if relevant)
- [ ] Desktop and mobile versions
- [ ] Demo data and formats follow section 10
- [ ] Accessibility rules from section 11
