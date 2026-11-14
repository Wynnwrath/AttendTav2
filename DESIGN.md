---
name: AttendTa
description: Student-organization event registration and attendance, as a campus operations console.
---

<!-- SEED: established with the project owner on 2026-11-09, before implementation.
     This file replaces docs/design.md as the visual authority. docs/design.md is now an
     anti-reference: read it to know what this system is not.
     Direction: the category standard, taken as the standing exit and executed at full
     fidelity. No world, no metaphor. Tokens here are committed decisions, not extracted
     ones. Re-run /impeccable document once Styles/app.css and the shell exist, so the
     frontmatter and .impeccable/design.json carry the real extracted values. -->

# Design System: AttendTa

## Overview

**Creative North Star: "The Workspace Standard"**

AttendTa is a campus operations console, and it should look like the best one in its
category rather than like a designed object. The officer arrives to do a job: review a
request, fix a record, open the door, read the numbers. Every pattern they meet should be
one they already know — a left rail, a table, a search field, a status tag, a dialog. The
interface earns trust by being unsurprising and then by being exact.

Excellence here is not expression. It is alignment, density that respects the work, a type
scale with real hierarchy, complete empty and error states, and an accent that appears only
where an action is. There is no metaphor to decode, no texture to notice, and nothing
decorative to maintain.

This is a deliberate break from the superseded identity, which was itself a competent
category-standard shell. The difference is not a new world; it is a different ground, a
different ink, a different accent, a different typeface, a different shell, and a stricter
hand on depth. Those are listed as prohibitions at the end, because the old system is close
enough that drift would be invisible.

**Key Characteristics:**
- A left rail and top bar for management; a top header for members. Content is a full-width
  sheet, not a card grid.
- One cobalt accent, reserved for actions, focus, and the current destination.
- 1px borders do the structural work; shadows appear only on dialogs.
- Inter only. Figures use tabular numerals, not a mono family.
- 4px controls and tags, 8px panels; no 10px or 16px corners anywhere.
- Status is a tinted 4px tag with an uppercase label, never a rounded badge.
- Real data only: no tile, count, or action the service cannot produce.

## Colors

A neutral working surface with one action colour and a separate semantic set for state.

### Primary
- **Cobalt** (`#1D4ED8`): the only action accent. Primary buttons, links, the current
  destination in the rail, the focus ring, and the selected filter key. 6.7:1 on white.
- **Cobalt Deep** (`#1A45BE`): primary button hover and pressed states.
- **Cobalt Tint** (`#E8ECF9`): active rail item, selected row, and the quiet partner to the
  accent. Never a page background.

### Secondary
- **Confirmed Green** (`#0B6B45`): confirmed registrations, active members, issued
  certificates. Semantic only — never a button.
- **Pending Ochre** (`#8A5A00`): pending requests, waitlisted registrations, draft events.
- **Declined Red** (`#B42318`): declined, cancelled, and destructive actions.

### Tertiary
- **Neutral Grey** (`#6B6E74`): inactive members, closed check-in, archived records.

### Neutral
- **Ground** (`#F5F5F6`): the page background.
- **Surface** (`#FFFFFF`): panels, tables, rail, top bar, dialogs.
- **Surface Subtle** (`#FAFAFB`): table heads, hover rows, inset sections.
- **Border** (`#E4E5E7`): the 1px rule used everywhere.
- **Border Strong** (`#D2D4D7`): input borders and dividers that must read as structure.
- **Ink** (`#0E0F12`): primary text and headings.
- **Ink Secondary** (`#3A3D43`): table cells and supporting copy.
- **Ink Muted** (`#6B6E74`): metadata, helper text, inactive labels. 4.69:1 on ground.
- **Ink Faint** (`#9CA0A6`): decorative detail and disabled text only — 2.63:1 on white,
  never essential text.

### Named Rules

**The One Accent Rule.** Cobalt is the only colour that means "act". It appears on the
primary button, links, the current destination, and the focus ring — and nowhere else. A
screen with two cobalt fills has two primary actions and is wrong.

**The Semantic Separation Rule.** Green, ochre, red, and grey describe state. They never
appear on a button, a link, or a selected control. Cobalt never describes state.

**The Contrast Floor Rule.** Muted ink is the lightest text allowed at normal size. Faint
ink is decorative only; if a reader needs it to complete the task, it is the wrong colour.

## Typography

**Display and body:** Inter (system-ui, -apple-system, Segoe UI, sans-serif)
**Figures:** Inter with `font-variant-numeric: tabular-nums`
**Mono:** none. The system has no monospace family.

**Character:** One workhorse neo-grotesque, used with discipline. Hierarchy comes from size,
weight, and colour — never from a second family. Student numbers, times, and counts align
because the figures are tabular, not because they are set in mono.

### Hierarchy
- **Display** (600, 22px/1.25, -0.015em): page titles. One per screen.
- **Headline** (600, 16px/1.35): panel and section headings.
- **Title** (600, 13.5px/1.4): row names, dialog titles, card headings.
- **Body** (400, 14px/1.5, 65–75ch): prose and help text.
- **Table** (400, 13px/1.45): roster cells and dense lists.
- **Label** (600, 10.5px, 0.06em, uppercase): column heads, field labels, status tags.
- **Micro** (600, 10px, 0.07em, uppercase): table heads and rail group labels.
- **Figure** (500, 13px, tabular): any number a reader compares down a column.

### Named Rules

**The Tabular Rule.** Every comparable number — student numbers, times, counts, rates — uses
tabular figures. Proportional figures in a column are a bug.

**The One Family Rule.** Inter for everything. A second typeface, a display serif, or a mono
family is drift, not a variation.

## Layout

A fixed left rail (256px) and a top bar (64px) frame a full-width content area with 24px
gutters. Management lists use the available width and align with the breadcrumb rather than
centering a capped sheet away from the sidebar. The content is a sheet of stacked sections, never a grid
of floating cards.

Management (administrator and officer) works from the rail shell: the rail carries the brand,
the grouped destinations and the account block at its bottom; the top bar carries the
breadcrumb and the role indicator. Members work from a top-header shell instead: a 64px bar
with the brand, the three member destinations and the account block, and no rail.

**Owner exception (2026-11-10):** the member Events feed (`MemberEvents/Index`) is a
cover-card grid. It is the only card grid in the system; the sheet rule above governs every
other screen.

A 4px rhythm with 8/12/16/24/32 steps. Roster rows are 48px; compact rows 40px. Section gaps
are 24px; in-section gaps 16px. Table cells use 14px horizontal padding.

The rail groups destinations under micro labels (Overview, Records, Output) and marks the
current one with a cobalt tint and cobalt text. Page heads carry the title, an optional
one-line description, and — only where the action exists — a single primary button on the
right.

Responsive: below 1024px the rail collapses into the existing disclosure menu and the top bar
keeps the breadcrumb and the role indicator; the member header collapses its destinations and
account block into the same disclosure menu. Below 768px gutters drop to 16px and page heads
stack. Wide tables keep their sticky first and last columns. At 200% zoom the rail collapses
before any content is clipped.

Account-request exception: the first column is applicant identity. Below 1024px this table
releases both sticky edges so they cannot obscure the intervening fields; horizontal scrolling
must expose every field. Other tables retain their existing column policy.

## Elevation & Depth

Flat. Every surface is a 1px border on the ground or on white. There are no panel shadows, no
hover lifts, no glows, and no gradients.

Depth is expressed by three paper values — ground, surface, surface-subtle — and by border
weight. A dialog is the only element that lifts, because it must read as physically above the
page.

### Shadow Vocabulary
- **Dialog lift** (`box-shadow: 0 20px 48px -16px rgba(14, 15, 18, 0.24)`): modal and record
  dialogs only.

### Named Rules

**The Border-Not-Shadow Rule.** If a panel needs a shadow to separate from the page, the
border or the ground value is wrong. Fix that instead.

## Shapes

Corners are tight and consistent: 4px on buttons, inputs, status tags, and small controls;
8px on panels, tables, dialogs, and KPI cards; 999px only on the account avatar. There are no
10px or 16px corners, and no pill-shaped badges.

Borders are 1px and always present on interactive controls. A borderless input, a borderless
button, or a card with no outline is not part of this system.

## Components

### Buttons
- **Shape:** 4px radius, 36px height (44px on touch surfaces), 14px horizontal padding.
- **Primary:** Cobalt fill, white text, 600 weight at 12.5px. Exactly one per screen.
- **Secondary:** white fill, 1px Border Strong, Ink Secondary text.
- **Destructive:** Declined Red fill, white text. Requires a confirm dialog.
- **Hover / Focus:** primary darkens to Cobalt Deep; focus is a 2px Cobalt outline at 2px
  offset, and a 2px white outline inside the rail.
- **Placement:** page heads, table rows and dialogs use bordered buttons. Row controls use a
  compact 32px height; touch surfaces use at least 44px. Ordinary inline navigation remains a link.

### Status Tags
- **Style:** 4px radius, tinted background, 10.5px 600 uppercase at 0.06em, 22px height, 9px
  horizontal padding. No dot, no pill, no border.
- **Confirmed / Active:** Confirmed Green on `#E7F2EC`.
- **Pending / Waitlisted:** Pending Ochre on `#F7EEDD`.
- **Declined / Cancelled:** Declined Red on `#FBEAE8`.
- **Inactive / Draft:** Neutral Grey on `#F1F2F4`.
- **Rule:** the label is always present. Colour is never the only signal.

### Tables / Rosters
- **Shape:** 8px panel, 1px Border, no shadow.
- **Head:** Surface Subtle ground, Micro type, 1px Border below, 38px height.
- **Rows:** 48px, 1px Border between, hover Surface Subtle, no zebra striping.
- **Cells:** 13px, Ink Secondary; the name cell is 600 Ink with a 26px initials tile.
- **Numbers:** tabular figures, Ink, right-aligned where the column is numeric.
- **Actions:** compact bordered buttons with 8px gaps. Use an overflow menu for secondary actions.
- **Density:** keep identity and decision facts visible; combine related fields and put
  secondary information in a shared Show details dialog. No internal table scroll region.
  Below 640px, rows become labelled blocks with natural page scrolling.
- **Search and filters:** each list declares appropriate fields through the shared Razor
  toolbar. An applied search shows Clear; changing its text restores Search. Preserve
  unrelated query criteria across filters, paging and native decisions.
- **Count and pager:** one full-width footer directly below the table shows the actual
  range and filtered total, Auto size, page number, and visible disabled boundaries.
  Zero results show a count without a fabricated page. Auto-fit targets five records,
  reduces the page for short windows using measured rows/cards, and holds that size while
  paging until filters or viewport dimensions change. Never clip content to force a fit.
- **List spacing:** page heads end with 16px of space. Filter toolbars keep 16px of
  bottom padding; lists with loading feedback reserve one compact 16px live line before
  the table, without an additional blank paragraph margin.

### Event dossier
- **Fact sheet:** schedule and signup deadline as labelled values on the left; confirmed
  capacity as a bordered block on the right with the 26px tabular figure and the remaining-seat
  line. The two stack below 768px.
- **Check-in:** a row in the fact sheet carrying the state tag and a secondary control; the tag
  names the row. Complete event is the only primary, and only once the event has started;
  before that it is a disabled secondary.
- **Roster:** status keys, compact table and consistent pagination/count footer. Repeated timezone
  captions are omitted; UTC storage and Manila display conversion remain unchanged.

### Inputs / Fields
- **Style:** white ground, 1px Border Strong, 4px radius, 36px height, 13.5px.
- **Focus:** 2px Cobalt outline at 2px offset; the border does not change.
- **Error:** 1px Declined Red border plus a red message line; the message is text, not colour.
- **Disabled:** Surface Subtle ground, Ink Faint text, border unchanged.

### Navigation
- **Rail:** 256px white, 1px Border on the right, grouped under micro labels. Items are 34px,
  6px radius, 13px. Current item: Cobalt Tint ground, Cobalt text, 600 weight. The account
  block sits at the bottom of the rail, above a 1px Border, and stays pinned there when the
  rail scrolls.
- **Top bar:** 64px white, 1px Border below, breadcrumb left, role indicator right. The
  account block lives in the rail, not the top bar.
- **Member header:** 64px white, 1px Border below, brand left, Events / My Events / My Passes
  after the brand, account block right. A long account name truncates on one line.
- **Account block:** a 32px neutral initials avatar, the name and the role tag in a 44px
  trigger with a chevron. The trigger is a rail row with a hover ground and a focus ring, not a
  boxed button; the member header reuses it at content width. It opens a small popover menu
  holding Profile (members only) and Sign out. Enter or Space opens, Escape closes, and focus
  returns to the trigger. Management has no Profile destination, so its menu carries Sign out
  alone.
- **Role indicator:** the real role is a neutral outlined text tag in the account block, the
  management top bar and the mobile menu. Administrator and Officer remain explicit words, not
  different themes.
- **Mobile:** the existing disclosure menu, restyled to the rail's vocabulary; 44px targets.
  It carries the destinations, the name, the role tag and Sign out.

### KPI Card (restricted)
White panel, 8px radius, 1px Border, 14px/16px padding, 11.5px label, 26px 600 tabular
figure, optional 11.5px cobalt delta. **Only on screens with real aggregates** — Reports,
Event details capacity, Check-in counts. Never on a list whose service returns only a total.
The dossier's confirmed-capacity block is the Event details instance.

### Dialog
8px radius, white, 1px Border, the single dialog shadow, 24px padding. Title at 16px 600,
actions right-aligned with the primary last. Focus is contained, Escape closes, and the
backdrop is `rgba(14, 15, 18, 0.36)`.

Edited dialogs ask before discarding unsaved input. The discard prompt stays inside the existing
dialog, with Keep editing and Discard changes actions; dialogs are never stacked for confirmation.
Unchanged and read-only dialogs dismiss immediately. A busy save cannot be dismissed.

## Do's and Don'ts

### Do:
- **Do** keep one cobalt primary action per screen and put it at the right of the page head.
- **Do** use 1px borders and the three paper values instead of shadows.
- **Do** set every comparable number in tabular figures.
- **Do** give every status a text label alongside its tint.
- **Do** show the real total from `PagedResult.TotalCount` and nothing more.
- **Do** keep the AttendTa logo tile and the "University of Cebu" line exactly as supplied.
- **Do** keep 44px targets on touch surfaces and a visible focus ring everywhere.

### Don't:
- **Don't** reuse the superseded palette: slate `#F8FAFC`, navy `#0B1120`/`#090D16`, amber
  `#D97706`, emerald `#059669`, rose `#DC2626`.
- **Don't** use Outfit, JetBrains Mono, or Cinzel, or add any mono family for figures.
- **Don't** use 10px or 16px corners, pill badges, or filled rounded status chips.
- **Don't** restore the old shell: a horizontal nav row under a white header with a dark
  identity strip.
- **Don't** add panel shadows, hover lifts, gradients, glass, or texture.
- **Don't** invent a create action, a filter panel, or a count the service cannot produce —
  the members list has none of the three.
- **Don't** turn a roster into a card grid, or a detail page into a dashboard.
