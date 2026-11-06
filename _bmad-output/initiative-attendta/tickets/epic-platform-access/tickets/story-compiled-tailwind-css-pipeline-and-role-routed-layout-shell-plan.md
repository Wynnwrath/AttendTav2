---
title: 'Compiled Tailwind and role-routed navigation shell'
type: 'feature'
ticket: '9'
created: '2026-11-05'
status: 'built'
baseline_revision: 'e50e833541f99f9daf3605c156177607d0743c90'
route: 'full'
route_source: 'auto'
risk: 'medium'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 1
context:
  - '_bmad-output/initiative-attendta/ux-attendta/DESIGN.md'
  - '_bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md'
  - '_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The shared layout still depends on Bootstrap and internet-hosted assets, and its navigation does not reflect the authenticated role. Existing views depend on Bootstrap styling, so removing it without a replacement would degrade them.

**Approach:** Build Tailwind locally from the visual tokens, replace the shared shell with accessible role-aware layouts, and retain usable existing screens, feedback, and antiforgery behavior.

## Boundaries & Constraints

**Always:** Story 1.1 / issue #11 must be merged and its actual authentication, role, logout, and login contracts inspected before implementation; update this plan with the human if its assumptions no longer hold. Follow AD-8 and AD-14 and the visual handoff. Use the actual authenticated principal and server authorization; visible links are not an authorization boundary. Render only available, permitted destinations; do not invent routes. Keep UI usable on the offline hotspot after installation/build, with locally served licensed fonts and dependencies. Preserve existing master-data behavior and accessible validation.

**Never:** Implement 1.1, weaken authorization or antiforgery, expose unavailable routes as functioning features, change master-data business rules, use runtime CDNs, or remove Bootstrap before its dependent screens have usable replacements.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|----------------------------|----------------|
| Public | Anonymous request | Public layout has no authenticated navigation | No protected destinations exposed |
| Role navigation | Authenticated admin, officer, or member | Correct available links and shell for each role on desktop and mobile | Server policies still reject forbidden requests |
| Feedback | Success or error TempData | One readable, accessible message with safely encoded text | Empty messages produce no notification |
| Mutations | Authenticated page and logout/AJAX POST | Layout exposes antiforgery token; logout uses 1.1's POST contract | Missing or invalid token is rejected by the server |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.WebApp/Views/Shared/{_Layout,_Header}.cshtml` and `wwwroot/js/site.js` — legacy shell, remote assets and click-only dropdown; retain POST logout.
- `AuthConstants.cs`, `Startup.Auth.cs`, `AccountController.cs` — roles, policies and authentication. Configure the AJAX antiforgery header.
- `Views/{Account,Member,EventType,Venue}/`, `wwwroot/{css,fonts,lib}/` — Bootstrap forms and remote fonts; local jQuery and validation exist. Avoid duplicate bundles.
- `ux-attendta/{DESIGN,EXPERIENCE}.md` — typography, colors, shell, focus and reduced motion; fonts are not bundled in the prototype.
- `.github/workflows/ci.yml`, `tests/ASI.Basecode.IntegrationTests/{AttendTaWebApplicationFactory,AuthenticationFlowTests}.cs` — CI and cookie/policy/logout tests; no browser suite.

## Tasks & Acceptance

**Execution:**
- [x] Inspected 1.1 auth and tests, merged in PR #87.
- [x] `ASI.Basecode.WebApp/package.json`, `package-lock.json`, `Styles/`, `wwwroot/css/app.css`, `.gitignore` — pin Tailwind and `@tailwindcss/cli` 4.3.4, compile from DESIGN tokens, scan Razor/JS sources, explicitly include dynamic utilities, commit reproducible CSS, and ignore `node_modules`.
- [x] `.github/workflows/ci.yml` — add Node setup, `npm ci` and CSS compilation before .NET build; fail when regenerated committed CSS differs.
- [x] `Views/Shared/`, `Startup.Auth.cs` and `wwwroot/{css,js,fonts,lib}/` — provide the design's 256px desktop management rail and 64px top bar; make rail contents vertically scrollable at short desktop viewports. Keep role-aware navigation, local assets, feedback, POST logout and antiforgery header.
- [x] `Views/{Account,Member,EventType,Venue}/` and legacy CSS — migrate Bootstrap markup while preserving behavior; associate each field error with its input on server-rendered validation failures. Remove runtime Bootstrap and CDN references.
- [x] `tests/ASI.Basecode.IntegrationTests/` — cover the matrix, including server-rendered error associations, with real HTTP tests; preserve authorization checks.

**Acceptance Criteria:**
- Given a clean checkout with Node and .NET 10, when CSS is built, the asset is reproducible and CI builds and tests the solution.
- Given an offline browser after the app is running, when each available role's shell and existing master-data screens are used, needed scripts, styles and fonts load locally, with no Bootstrap or CDN requests.
- Given desktop and mobile viewports, each role can navigate and see feedback. At 320px and 200% zoom controls remain usable by mouse and keyboard.

## Implementation Notes

- Added locally served, licensed fonts and role-aware shells, converted existing views, configured antiforgery AJAX headers, and added CI CSS reproducibility checks. Excluded `node_modules` from TypeScript compilation. Browser testing exposed and fixed a validation-message layout shift.
- Clean CSS rebuild matched; Release build passed with zero warnings/errors; 16 service and 46 isolated LocalDB integration tests passed. Chromium covered three roles at 1440, 390 and 320px, keyboard, reduced motion, blocked external assets, representative forms and logout. CSS zoom simulated 200%; real browser zoom remains unverified. Disposable browser database and accounts were removed.
- After the review loop, the clean CSS rebuild matched and the Release build, 16 service tests and 50 isolated LocalDB integration tests passed. Chromium verified all roles at 1440, 390 and 320px and all rail controls by keyboard at 1024×384, plus the new top bar, field errors, offline assets and reduced motion. Disposable app and database were removed; actual 200% browser zoom remains unverified.
- After the second review, changed validation container IDs, constrained top-bar text and distinguished no search results. Clean CSS rebuild and Release build passed; 16 service and 53 LocalDB integration tests passed. The focused client-side browser rerun stalled, so these final patches have not been rechecked in Chromium.

## Plan Change Log

- Quick review found an unreachable fixed rail at short desktop heights, missing programmatic server-error associations, and the handoff's missing management top bar. The tasks now require a scrollable rail, a 64px top bar and linked field errors to avoid inaccessible controls and incomplete layout. KEEP the working local assets, role checks, antiforgery, safe feedback, responsive tables, validation stability and passing HTTP matrix tests.
- The prior implementation diff is saved at `F:/AttendTaBakcend/tmp/bmad-temp/bmad-1-9-diff-znk0qi5j.patch` and its new assets at `F:/AttendTaBakcend/tmp/bmad-1-9-review-backup/`. Both include work to reuse during re-derivation. The diff also contains pre-existing unrelated documentation changes; preserve those and do not reapply them.

## Review Triage Log

- medium / bad_plan: Fixed management rail lacks overflow scrolling; at desktop breakpoints its contents can extend below a short viewport.
- medium / bad_plan: Server-rendered property errors lack programmatic associations with their inputs; ModelOnly summaries do not convey them.
- medium / bad_plan: The desktop management shell omits the 64px top bar required by DESIGN.md.
- medium / patch: jQuery Validation creates an inner `<field>-error` element, duplicating the server error-container ID; use distinct IDs while keeping accessible associations.
- medium / patch: Unbroken long account names can overflow the management top bar; constrain its flex children.
- low / patch: The members empty state does not distinguish an empty directory from a search with no matches, contrary to EXPERIENCE.md; select the message from the active search.

## Design Notes

Login redirects to Home; preserve ReturnUrl and generic errors. Home is available to all authenticated roles; Member, EventType and Venue require Management. No Dashboard or Settings route exists. Story 1.3 is not visible here; 1.9 declares 1.1 as its prerequisite.

## Verification

**Commands:**
- `npm --prefix ASI.Basecode.WebApp ci` and `npm --prefix ASI.Basecode.WebApp run css:build` — pinned dependencies install and CSS compiles.
- Check regenerated `wwwroot/css/app.css` matches the committed asset.
- `dotnet build ASI.Basecode.sln --configuration Release` — solution builds.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --configuration Release` — service tests pass.
- With `ATTENDTA_TEST_SQL` pointing to an isolated SQL Server, `dotnet test --project tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj --configuration Release` — integration and shell checks pass.

**Manual checks:**
- On merged 1.1, verify real admin, officer and member navigation, POST logout, a successful and failed message, and representative member/event-type/venue forms. Check desktop, 390px and 320px, 200% zoom, keyboard focus and reduced motion. In browser network tools block external requests and confirm assets load locally; compare the shell with the UX captures.
- Local tools: use `C:\Users\cruiz\AppData\Local\pi-node\current\npm.cmd` and `C:\Program Files\dotnet\dotnet.exe` (.NET 10). `ATTENDTA_TEST_SQL` is unset; establish isolated SQL Server before claiming integration coverage.
