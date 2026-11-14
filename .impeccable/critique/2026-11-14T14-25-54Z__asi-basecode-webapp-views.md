---
target: ASI.Basecode.WebApp/Views
total_score: 25
max_score: 40
na_heuristics: 
p0_count: 0
p1_count: 1
target_identity: "file:F:\\AttendTaBakcend\\AttendTa\\ASI.Basecode.WebApp\\Views"
timestamp: 2026-11-14T14-25-54Z
slug: asi-basecode-webapp-views
---
Method: dual-agent Impeccable critique (A: /root/ux_design_review; B: /root/ux_detector_review), with parent source verification.

Target: ASI.Basecode.WebApp/Views and shared dialog/navigation behavior.
Date: 2026-11-14.
No application code changed.

Evidence limits: Browser inspection stopped because Computer Use could not reliably verify the current Windows browser URL. No live visual audit completed. Old screenshots predate the current design direction and were not treated as current truth. Deterministic detector completed with exit 0 and JSON [] for the views directory; zero findings does not establish UX correctness.

Design specificity: Product-appropriate campus operations console, using familiar controls. Membership, registration, attendance, and certification establish a coherent workflow. Precision and predictability matter more than decorative novelty.

Priority additions to the agreed plan:
1. P1 - Unsaved dialog input can be lost. record-dialog.js closes on Cancel/Escape/backdrop without dirty-state protection and reloads server content on reopen. site.js resets decision textareas on open. Warn before discarding edited form data or entered reasons; untouched/read-only dialogs should close freely.
   Evidence: ASI.Basecode.WebApp/wwwroot/js/features/record-dialog.js:55,60,93; ASI.Basecode.WebApp/wwwroot/js/site.js:25,55.
   Suggested command: /impeccable harden.
2. P2 - Unexpected-error screen includes developer instructions rather than user recovery. Replace Development Mode directions with a plain explanation, safe return navigation and a reference ID.
   Evidence: ASI.Basecode.WebApp/Views/Shared/Error.cshtml:26,33.
   Suggested command: /impeccable clarify.
3. P2 - Shared event selector renders title only. Recurring same-name events are hard to distinguish. Add date and venue where available and preserve selected-event context across reports/certificates.
   Evidence: ASI.Basecode.WebApp/Views/Shared/_EventSelector.cshtml:14.
   Suggested command: /impeccable clarify.
4. P2 - Certificate viewer return link retains event/tab but not originating page. Preserve page and future search/filter context when returning to the ledger.
   Evidence: ASI.Basecode.WebApp/Views/CertificateLedger/Viewer.cshtml:11.
   Suggested command: /impeccable harden.
5. P2 - Draft cover removal is an immediate POST without confirmation. Identify event/current cover in a confirmation; preview selected replacement before save. Already related to agreed cover modal scope.
   Evidence: ASI.Basecode.WebApp/Views/Event/Cover.cshtml:35-60.
   Suggested command: /impeccable harden.

Already covered, not new scope: direct registration declines/cancellations, missing queue search and bulk approvals, repeated timezone labels, shared action styles, spacing, table trimming/details dialogs, login/membership CTA, report alignment, publish readiness guidance, responsive pagination and counts.

Strengths: Account-request/dossier decisions identify targets and consequences. Dossier retries preserve participant/reason context. Reports distinguish final/non-final metrics. Status tabs, explicit empty states and labelled actions aid recognition.

Provisional source-only Nielsen scores (not a visual grade): system status 3; familiar language 3; user control 3; consistency 2; error prevention 3; recognition 3; efficiency 1; minimal presentation 3; recovery 3; help 1. Total 25/40, Acceptable with significant improvements needed.

Persona red flags: First-time officer must distinguish recurring events and recover from errors without developer knowledge. Busy officer loses efficiency in queues and list position after PDF viewing. Interrupted user may dismiss edited dialog content accidentally. Keyboard/low-vision user requires live focus/zoom/contrast checks before claims can be made.

Cognitive load: dense account table (ten columns), multiple status filters and repeated actions increase simultaneous comparison demands. The agreed progressive disclosure and bulk approval work directly addresses that. Do not hide decision-critical identity/status information.

Minor observations and remaining checks: same labels/action names need standardization, safe individual issuance is intentional, details dialogs should not create unnecessary modal chains. Actual spacing, card alignment, sidebar gutter, touch targets, contrast, mobile overflow, focus restoration, auto-fit pagination stability and zoom behavior remain unverified.

Questions skipped: agreed scope is sufficient; findings require no additional user design decision.
