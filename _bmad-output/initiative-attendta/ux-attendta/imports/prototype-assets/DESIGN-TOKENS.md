# AttendTa v2 — Design Token Register

**Purpose:** the single, frozen value set for AttendTa v2 (static prototype) — the contract every builder references and no one re-invents. Design direction: *Institutional Precision* — registrar ledger × operations console. One brand accent: amber.

**Version:** 2.3 · **Date:** 2026-10-17
**Method:** All ratios computed with WCAG 2.2 relative luminance (sRGB) against the stated surface. Source of truth: `assets/tokens.css`.
**Changelog:** 2.3 (Event Moods final cut) — set reduced to Drift / Bloom (Lattice, Ember, Tempo retired); Tier 1 live takeover documented; 2.2 (Event Moods) — adds the Event Moods backdrop register (§ below); no token value changes; 2.1 (Phase 6 final) — adds `--navy-hover`; 2.0 (2026-10-15) — initial freeze after Wave D.

---

## Locked rules

- **ink-faint** is decorative only — hairlines, dividers, disabled glyph fills. **Never text.**
- **accent-amber** is for large text, icons and UI only. For small amber text and links use **accent-amber-deep**.
- **status-\*** colors are functional only (attendance, approval, error states). Never brand, never decoration.
- **event accents** apply ONLY to: type badges, event card hairline, dossier header rule, capacity gauge, LIVE band, pass strip, kiosk session chip, history ticks, report bars. **NEVER** nav, admin tables, buttons, status colors, or certificates.

---

## Ink

| Token | Value | Contrast vs | Ratio | WCAG | Use | Do Not Use For |
|---|---|---|---|---|---|---|
| `--ink-primary` | `#090D16` | canvas-surface | 19.43 | AAA | Headings, primary body copy, ledger values | — |
| `--ink-secondary` | `#334155` | canvas-surface | 10.35 | AAA | Secondary copy, labels, table cells | — |
| `--ink-muted` | `#64748B` | canvas-surface | 4.76 | AA | Meta, timestamps, helper text | Oversized display type |
| `--ink-muted` | `#64748B` | canvas-ground | 4.55 | AA | Meta on the page ground | — |
| `--ink-faint` | `#94A3B8` | canvas-surface | 2.56 | Decorative — never text | Hairlines, disabled fills, divider glyphs | Any text, any size |
| `--ink-inverse` | `#FFFFFF` | accent-amber-deep | 5.02 | AA | Text on deep amber / dark plates | Light surfaces |

## Canvas

| Token | Value | Contrast vs | Ratio | WCAG | Use | Do Not Use For |
|---|---|---|---|---|---|---|
| `--canvas-ground` | `#F8FAFC` | — | n/a | Not applicable | App background | Text directly (use ink-*) |
| `--canvas-surface` | `#FFFFFF` | — | n/a | Not applicable | Cards, sheets, table body | — |
| `--canvas-subtle` | `#F1F5F9` | — | n/a | Not applicable | Table headers, inset panels, kbd fill | — |
| `--canvas-elevated` | `#FFFFFF` | — | n/a | Not applicable | The single elevated surface per view | Stacking many elevated cards |
| `--canvas-dark` | `#0B1120` | ink-inverse | 18.83 | AAA | Kiosk / dark console plates | Anywhere else |
| `--navy-hover` | `#0B1120` | ink-inverse | 18.83 | AAA | Hover/active plate for `.btn--primary` | Anywhere else |

## Amber (single brand accent)

| Token | Value | Contrast vs | Ratio | WCAG | Use | Do Not Use For |
|---|---|---|---|---|---|---|
| `--accent-amber` | `#D97706` | canvas-surface | 3.19 | AA (large/UI only) | Icons, gauges, rules, active nav underline, large text | Small body text, long-form copy |
| `--accent-amber-deep` | `#B45309` | canvas-surface | 5.02 | AA | Small amber text, links, inline emphasis | — |
| `--accent-amber-subtle` | `#FEF3C7` | ink-primary | 17.45 | AAA | Selected-row / highlight wash behind ink-primary | Text color |
| `--accent-amber-glow` | `rgba(217,119,6,0.12)` | — | n/a | Decorative — never text | Focus halos, live pulse halo | Text, borders that carry meaning |
| `--ink-primary` on `--accent-amber` | `#090D16` / `#D97706` | accent-amber | 6.10 | AA | Amber button label | — |
| `--ink-inverse` on `--accent-amber-deep` | `#FFFFFF` / `#B45309` | accent-amber-deep | 5.02 | AA | Inverse label on deep amber plate | — |

## Status (functional only)

| Token | Value | Contrast vs | Ratio | WCAG | Use | Do Not Use For |
|---|---|---|---|---|---|---|
| `--status-emerald` | `#059669` | canvas-surface | 3.77 | AA (large/UI only) | Present ticks, success icons, gauges | Small text |
| `--status-emerald-text` | `#047857` | canvas-surface | 5.48 | AA | "Present" / success label text | — |
| `--status-emerald-subtle` | `#ECFDF5` | — | n/a | Not applicable | Success chip wash | Text color |
| `--status-amber` | `#D97706` | canvas-surface | 3.19 | AA (large/UI only) | Late / pending icons and bars | Small text |
| `--status-amber-subtle` | `#FFFBEB` | — | n/a | Not applicable | Pending chip wash | Text color |
| `--status-rose` | `#DC2626` | canvas-surface | 4.83 | AA | Absent / error text and icons | Brand, decoration |
| `--status-rose-subtle` | `#FEF2F2` | — | n/a | Not applicable | Error chip wash | Text color |
| `--status-neutral` | `#475569` | canvas-surface | 7.58 | AAA | Neutral / excused labels | — |
| `--status-neutral-subtle` | `#F1F5F9` | — | n/a | Not applicable | Neutral chip wash | Text color |

## Event accents

Each family: `--event-{family}` (base, marks/gauges/hairlines), `--event-{family}-deep` (small text on light), `--event-{family}-subtle` (chip wash), `--event-{family}-glow` (halo).

### moss

| Token | Value | Contrast vs | Ratio | WCAG | Use | Do Not Use For |
|---|---|---|---|---|---|---|
| `--event-moss` | `#4D7C0F` | canvas-surface | 4.99 | AA | Badge fill, hairline, gauge | — |
| `--event-moss-deep` | `#3F6212` | canvas-surface | 7.08 | AAA | Small text on light | — |
| `--event-moss-deep` | `#3F6212` | event-moss-subtle | 6.84 | AA | Text on moss chip | — |
| `--event-moss-subtle` | `#F7FEE7` | — | n/a | Not applicable | Chip wash | Text color |
| `--event-moss-glow` | `rgba(77,124,15,0.12)` | — | n/a | Decorative — never text | Halo | Text |

### indigo

| Token | Value | Contrast vs | Ratio | WCAG | Use | Do Not Use For |
|---|---|---|---|---|---|---|
| `--event-indigo` | `#4F46E5` | canvas-surface | 6.29 | AA | Badge fill, hairline, gauge | — |
| `--event-indigo-deep` | `#4338CA` | canvas-surface | 7.90 | AAA | Small text on light | — |
| `--event-indigo-deep` | `#4338CA` | event-indigo-subtle | 7.07 | AAA | Text on indigo chip | — |
| `--event-indigo-subtle` | `#EEF2FF` | — | n/a | Not applicable | Chip wash | Text color |
| `--event-indigo-glow` | `rgba(79,70,229,0.12)` | — | n/a | Decorative — never text | Halo | Text |

### teal

| Token | Value | Contrast vs | Ratio | WCAG | Use | Do Not Use For |
|---|---|---|---|---|---|---|
| `--event-teal` | `#0D9488` | canvas-surface | 3.74 | AA (large/UI only) | Badge fill, hairline, gauge | Small text |
| `--event-teal-deep` | `#0F766E` | canvas-surface | 5.47 | AA | Small text on light | — |
| `--event-teal-deep` | `#0F766E` | event-teal-subtle | 5.25 | AA | Text on teal chip | — |
| `--event-teal-subtle` | `#F0FDFA` | — | n/a | Not applicable | Chip wash | Text color |
| `--event-teal-glow` | `rgba(13,148,136,0.12)` | — | n/a | Decorative — never text | Halo | Text |

### violet

| Token | Value | Contrast vs | Ratio | WCAG | Use | Do Not Use For |
|---|---|---|---|---|---|---|
| `--event-violet` | `#7C3AED` | canvas-surface | 5.70 | AA | Badge fill, hairline, gauge | — |
| `--event-violet-deep` | `#6D28D9` | canvas-surface | 7.10 | AAA | Small text on light | — |
| `--event-violet-deep` | `#6D28D9` | event-violet-subtle | 6.48 | AA | Text on violet chip | — |
| `--event-violet-subtle` | `#F5F3FF` | — | n/a | Not applicable | Chip wash | Text color |
| `--event-violet-glow` | `rgba(124,58,237,0.12)` | — | n/a | Decorative — never text | Halo | Text |

### crimson

| Token | Value | Contrast vs | Ratio | WCAG | Use | Do Not Use For |
|---|---|---|---|---|---|---|
| `--event-crimson` | `#BE123C` | canvas-surface | 6.29 | AA | Badge fill, hairline, gauge | Status/error semantics |
| `--event-crimson-deep` | `#9F1239` | canvas-surface | 8.02 | AAA | Small text on light | — |
| `--event-crimson-deep` | `#9F1239` | event-crimson-subtle | 7.30 | AAA | Text on crimson chip | — |
| `--event-crimson-subtle` | `#FFF1F2` | — | n/a | Not applicable | Chip wash | Text color |
| `--event-crimson-glow` | `rgba(190,18,60,0.12)` | — | n/a | Decorative — never text | Halo | Text |

### orange

| Token | Value | Contrast vs | Ratio | WCAG | Use | Do Not Use For |
|---|---|---|---|---|---|---|
| `--event-orange` | `#EA580C` | canvas-surface | 3.56 | AA (large/UI only) | Badge fill, hairline, gauge | Small text |
| `--event-orange-deep` | `#9A3412` | canvas-surface | 7.31 | AAA | Small text on light | — |
| `--event-orange-deep` | `#9A3412` | event-orange-subtle | 6.88 | AA | Text on orange chip | — |
| `--event-orange-subtle` | `#FFF7ED` | — | n/a | Not applicable | Chip wash | Text color |
| `--event-orange-glow` | `rgba(234,88,12,0.12)` | — | n/a | Decorative — never text | Halo | Text |

### slate

| Token | Value | Contrast vs | Ratio | WCAG | Use | Do Not Use For |
|---|---|---|---|---|---|---|
| `--event-slate` | `#3D5A80` | canvas-surface | 7.06 | AAA | Badge fill, hairline, gauge | — |
| `--event-slate-deep` | `#2E4560` | canvas-surface | 9.83 | AAA | Small text on light | — |
| `--event-slate-deep` | `#2E4560` | event-slate-subtle | 8.88 | AAA | Text on slate chip | — |
| `--event-slate-subtle` | `#EFF4F9` | — | n/a | Not applicable | Chip wash | Text color |
| `--event-slate-glow` | `rgba(61,90,128,0.12)` | — | n/a | Decorative — never text | Halo | Text |

### amber

| Token | Value | Contrast vs | Ratio | WCAG | Use | Do Not Use For |
|---|---|---|---|---|---|---|
| `--event-amber` | `#D97706` | canvas-surface | 3.19 | AA (large/UI only) | Badge fill, hairline, gauge | Small text |
| `--event-amber-deep` | `#B45309` | canvas-surface | 5.02 | AA | Small text on light | — |
| `--event-amber-deep` | `#B45309` | event-amber-subtle | 4.51 | AA | Text on amber chip (tightest pair — do not lighten) | — |
| `--event-amber-subtle` | `#FEF3C7` | — | n/a | Not applicable | Chip wash | Text color |
| `--event-amber-glow` | `rgba(217,119,6,0.12)` | — | n/a | Decorative — never text | Halo | Text |

---

## Event Moods (student backdrop register)

Officer-selected per event; **shape & motion only** — color always follows the event's type accent (no free-form color, no new values — the Event accents above are reused verbatim). Closed set; pure CSS/SVG layers, no assets.

| Mood | Character | Technique | Ship durations | Type default |
|---|---|---|---|---|
| Plain | none (today's look) | — | — | fallback |
| Drift | layered wave fields | 2 masked wave layers, `translateX` loop | 55s / 80s | Seminar, Workshop, Assembly, Orientation |
| Bloom | floating bubble field | 9 radial-gradient circles (44–120px), slow float | 38s / 52s | Outreach, Sports, Recognition, Cultural Night |

**Final cut (2026-10-17):** Lattice, Ember and Tempo were built, previewed, then retired — one texture mood (Bloom) and one motion mood (Drift), plus Plain.

**Budgets (locked):** ≤ 2 span layers + container; `transform`/`opacity` only — no blur, no canvas, no JS loops. Washes and fills are built only from `*-glow` passes (flat wash ~12% alpha; bubble fills double-painted, ≤ ~23% effective). Frozen composition must read as intentional. Below 768px only the calmest layer animates. `prefers-reduced-motion` **or** `prefs.reducedMotion` (`.mood-frozen`) → full freeze-frame; `@media print` → never printed.

**Placement (locked):** `student/events.html` — Live-Now hero + full backdrop; `student/details.html` — quieter backdrop (`--quiet`, 55%). While an event is live the page root also carries `data-accent` — **Tier 1 takeover**: the backdrop paints the accent ground wash and the student header gains a 2px inset accent rule (same verified accents, no chrome recolor). Officer picker previews in `events/create.html` use amber-family tints only (color still follows the type). **Never** on officer / admin / kiosk / passes / public index.

**Guardrails (checker F9 / P13):** backdrop is `aria-hidden`, no inline styles, sits under a `.mood-content` wrapper above content; mood whitelist enforced in markup; reduced-motion + print kill-blocks required in `components.css`.

---

## Typography

| Token | Family | Role |
|---|---|---|
| `--font-display` | Outfit | Headings, section titles, nav, buttons |
| `--font-body` | Outfit | All body copy, controls, tables |
| `--font-mono` | JetBrains Mono | Values only — IDs, codes, counts, timestamps, tokens |
| `--font-ceremonial` | Cinzel | Certificates and print only |

**Scale floors**
- Body: 15px comfortable (`--density-text` 0.9375rem); 14px compact via `[data-density="compact"]`.
- Labels and meta: ≥ 12px.
- Absolute floor: 11px (`0.6875rem`) for kbd / mono chips only.
- Line-height: 1.5 body; 1.25 headings.

## Spacing

Unit set: **4 / 8 / 12 / 16 / 24 / 32 / 48**. Every margin and padding is one of these.

**Density modes** (`[data-density]`): comfortable = text `0.9375rem`, row pad `0.75rem`, section gap `1.5rem`; compact = text `0.875rem`, row pad `0.5rem`, section gap `1rem`.

## Radii

**6 / 10 / 16 / pill only** (`--radius-sm` / `--radius-md` / `--radius-lg` / `--radius-pill`).

## Elevation

Maximum **one** elevated surface per view (`--shadow-subtle`). Everywhere else: hairline borders (`--hairline-light` / `--hairline-medium` / `--hairline-dark` / `--hairline-amber`). No stacked card shadows.

## Motion

`--motion-fast` 120ms · `--motion-base` 180ms · `--motion-slow` 240ms · `--ease-standard` `cubic-bezier(0.2, 0, 0, 1)`. Reduced-motion honored globally via `prefers-reduced-motion`. Press feedback: `.btn-tactile` scales to exactly `0.96`.

## Focus

2px amber ring (`--focus-ring`) at 2px offset (`--focus-offset`) via `:focus-visible` — `outline: var(--focus-ring) solid var(--accent-amber)`.

## Z-scale

`--z-nav` 30 · `--z-sticky` 40 · `--z-drawer` 50 · `--z-modal` 60.
