---
name: AttendTa
description: Visual handoff for the existing event and attendance prototype.
status: final
updated: 2026-11-04
sources:
  - ../prd-attendta/prd-attendta.md
  - imports/README.md
colors:
  ink-primary: '#090D16'
  ink-secondary: '#334155'
  ink-muted: '#64748B'
  ink-faint: '#94A3B8'
  ink-inverse: '#FFFFFF'
  canvas-ground: '#F8FAFC'
  canvas-surface: '#FFFFFF'
  canvas-subtle: '#F1F5F9'
  canvas-dark: '#0B1120'
  amber: '#D97706'
  amber-deep: '#B45309'
  amber-subtle: '#FEF3C7'
  success: '#059669'
  success-text: '#047857'
  success-subtle: '#ECFDF5'
  error: '#DC2626'
  error-subtle: '#FEF2F2'
  neutral: '#475569'
  event-moss: '#4D7C0F'
  event-indigo: '#4F46E5'
  event-teal: '#0D9488'
  event-violet: '#7C3AED'
  event-crimson: '#BE123C'
  event-orange: '#EA580C'
  event-slate: '#3D5A80'
typography:
  body:
    fontFamily: Outfit
    fontSize: 15px
    lineHeight: '1.5'
  heading:
    fontFamily: Outfit
    lineHeight: '1.25'
  legacy:
    fontFamily: Plus Jakarta Sans
  values:
    fontFamily: JetBrains Mono
  certificate:
    fontFamily: Cinzel
rounded:
  sm: 6px
  md: 10px
  lg: 16px
  full: 9999px
spacing:
  '1': 4px
  '2': 8px
  '3': 12px
  '4': 16px
  '6': 24px
  '8': 32px
  '12': 48px
components:
  primary-button:
    background: '{colors.canvas-dark}'
    foreground: '{colors.ink-inverse}'
    radius: '{rounded.md}'
  surface-card:
    background: '{colors.canvas-surface}'
    radius: '{rounded.md}'
  input:
    background: '{colors.canvas-surface}'
    foreground: '{colors.ink-primary}'
    radius: '{rounded.md}'
  status-badge:
    foreground: '{colors.success-text}'
    background: '{colors.success-subtle}'
  scanner-window:
    background: '{colors.canvas-dark}'
    foreground: '{colors.ink-inverse}'
---

## Brand & Style

The existing AttendTa prototype is the visual reference for this student project. Its token register calls the direction “Institutional Precision”: a registrar ledger and operations console with light slate backgrounds, white panels, near-black text and restrained amber accents. Use the AttendTa logo and University of Cebu wording. Reproduce each page's visual layout, including existing variations; sample content and mock interactions do not establish product requirements. See the [reference bundle](imports/README.md), [login](imports/current-screens/current-login.png), [member events](imports/current-screens/current-member-events.png), [officer events](imports/current-screens/current-officer-events.png), [scanner](imports/current-screens/current-officer-scanner.png), [report](imports/current-screens/current-officer-report.png), and [admin](imports/current-screens/current-admin-users.png).

These UX documents take precedence over screenshot text and mock interactions. The PRD and system specification govern product behavior and scope. Pixel-identical output requires porting markup, styles, fonts and assets and comparing rendered screens; this document alone cannot guarantee it. Source prototype: frontend commit `7bd8cf7088601cf7efeee145b5237d1ae4cbb3f0`.

## Colors

Use {colors.canvas-ground} behind {colors.canvas-surface} panels, {colors.ink-primary} for primary text and {colors.ink-muted} for supporting text. {colors.ink-faint} is decorative only. Amber is the brand accent: {colors.amber} for icons and large markings, {colors.amber-deep} for small text and links, and {colors.amber-subtle} for highlights. Use {colors.success-text} for small success text and {colors.error} for errors; states also need words and icons. Event colors are restricted to event badges, card rules, passes, capacity bars and report marks. The admin prototype uses teal active navigation in some contexts; retain this observed page-specific variation when matching that screen.

For the full event color, contrast and glow register, use [DESIGN-TOKENS.md](imports/prototype-assets/DESIGN-TOKENS.md) and [tokens.css](imports/prototype-assets/tokens.css). Verify actual text/surface combinations; the register reports muted on ground at 4.55:1 and deep amber on white at 5.02:1. Do not use base amber for small text on white.

## Typography

The token system uses {typography.body.fontFamily} for display and body, {typography.values.fontFamily} for IDs, timestamps and counts, and {typography.certificate.fontFamily} for certificates. Some auth, officer/admin and event-detail pages explicitly use {typography.legacy.fontFamily}; preserve their actual type when matching those screens. Body is 15px comfortable or 14px compact; labels generally at least 12px. The prototype imports fonts externally: provide appropriate local font files and licenses for an intranet deployment and verify the appearance. Fallback fonts can alter layout.

## Layout & Spacing

Use the {spacing.1}–{spacing.12} scale. Public login has a 56px header and a centered white form up to 448px wide; registration is up to 576px. Member screens use a 64px header and 1152px centered content; event cards move from one column to two at 640px and three at 1280px. Officer/admin desktop screens use a 256px left rail at 1024px and above, a 64px top bar and up to 1280px content; event creation uses 896px. These dimensions are source-derived references; check individual pages. See [registration](imports/current-screens/current-registration.png), [mobile member events](imports/current-screens/current-mobile-member-events.png), and [mobile admin](imports/current-screens/current-mobile-admin.png).

## Elevation & Depth

Use thin slate hairlines and white surfaces for hierarchy. The token register allows one subtly elevated surface per view: `0 4px 12px -2px rgba(15,23,42,.05)`; dialogs use `0 20px 40px -15px rgba(15,23,42,.14)`. Some older pages use stronger Tailwind shadows; compare each screen to the current capture. An unloaded CDN render is not the intended visual reference.

## Shapes

Shared radius scale: {rounded.sm} for small controls, {rounded.md} for cards and controls, {rounded.lg} for larger panels, {rounded.full} for badges. Some legacy pages use different Tailwind radii; follow their observed composition. Shared buttons and inputs are around 40px high, large buttons 48px. Provide usable touch targets even if a capture shows smaller ones.

## Components

| Component | Visual handoff |
|---|---|
| Primary button | {components.primary-button.background} fill, {components.primary-button.foreground} label, {components.primary-button.radius}; secondary actions use light surfaces and borders. |
| Surface card | {components.surface-card.background}, hairline and {components.surface-card.radius}; event cards include cover crop, event badge, metadata and action. |
| Input | {components.input.background}, visible label, slate border, {components.input.radius}; field errors sit next to the control. |
| Status badge | Text plus functional color, using success, pending, error or neutral palette. |
| Navigation | Member top header/drawer; officer/admin desktop rail and top bar. The current narrow console lacks equivalent navigation: implementation needs an accessible mobile solution. |
| Ledger | White rows, subtle header, hairline separators, tabular values and pagination; keep columns readable on small screens. |
| Tabs and filters | Group adjacent tab or filter controls using subtle slate surfaces and a distinct selected treatment; use visible text and the active token treatment. |
| Data table | Use the ledger's white rows, subtle header and separators for tabular data; preserve column labels and legibility at narrow widths. |
| Dialog | Clear title, body, cancel and primary/confirm actions; preserve reference panel styling while keeping focus and errors accessible. |
| Pass | Event accent strip, event and registration details, and a 168px QR block in the shared component reference. |
| Scanner window | {components.scanner-window.background} preview area, camera action and result region; [desktop](imports/current-screens/current-officer-scanner.png) and [phone](imports/current-screens/current-mobile-scanner.png) references. |
| Certificate | Landscape A4 fixed template; [certificate reference](imports/reference-screens/SS-24_institutional_certificate_document.png) for composition only. Signatory details remain open. |

## Do's and Don'ts

- Preserve source layout and page-specific variations; compare rendered results at matching viewports.
- Use [prototype assets](imports/README.md) as visual references and bundle/compile required dependencies for local or intranet use.
- Use real application data, not screenshot text. Prototype test personas do not define the real login flow.
- Do not reintroduce student-number login, password recovery, in-system QR printing or bulk certificate issuance from older captures.
- Honor reduced motion; default transition tokens are 120/180/240ms. Show a visible 2px focus indicator with 2px offset.
- Aim for 44px touch targets; at minimum meet WCAG 2.2 target-size requirements of 24 by 24 CSS pixels or its spacing exceptions, including compact icon actions.
- Visual file index: [older screen references](imports/README.md#reference-screens) document additional prototype compositions; [prototype assets](imports/README.md#prototype-assets) index the logo, icons, covers, hero imagery, and per-page style sources. This index groups the imported material used for the visual handoff.
