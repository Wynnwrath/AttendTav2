# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

Officers and administrators are the primary operators: they review membership requests, maintain member/event-type/venue records, publish events, review registrations, run check-in, and read participation reports. Members are the other core audience: they request membership, browse events, register, hold passes, and view attendance and certificates. The demo prioritizes a complete officer flow with a member registration and attendance path.

## Product Purpose

AttendTa is a student-organization event registration and attendance system. It replaces in-person sign-ups and paper attendance with a reviewed membership request, online registration, door check-in, certificate eligibility, and post-event reporting. Success means an organization can run an event end to end and show a trustworthy record of who registered, who attended, and who earned a certificate.

## Positioning

One auditable lifecycle for student-organization events: request, approve, publish, register or waitlist, check in, certify, and report — with an append-only activity log behind every decision. Generic event tools stop at registration; AttendTa carries the same participant record through attendance and certification.

## Operating Context

Used by a University of Cebu student organization on a laptop at the door and on phones between classes. Officers work from a management shell (left rail, dense tables, filters, decision actions). Members work from a simpler top-header shell focused on events, passes, and personal history; their account block opens Profile and Sign out. The system runs as one ASP.NET Core MVC application against one SQL Server database; the demo runs on a local or hotspot host. Times are stored in UTC and shown in Asia/Manila.

## Capabilities and Constraints

- UI consistency implementation (2026-11-14): management content aligns to a shared 24px gutter
  beside the sidebar (16px on small screens); bordered row/page/dialog controls share a common
  vocabulary. Edited dialogs protect unsaved input with an inline discard choice. The phased
  delivery plan is recorded in `docs/ui-ux-implementation-plan.md`.

- Authentication by email and password with role-based access for administrator, officer, and member.
- Membership requests are reviewed and approved or declined with a reason; decisions are audited.
- Account-request tables lead with applicant identity and release both sticky edges below 1024px so every field remains horizontally readable. Fetched account/membership decisions and deactivation previews restore focus to the visible row-menu button when dismissed; impact previews include the server-read target name and email.
- Master data covers members, event types, and venues; records are deactivated rather than hard-deleted.
- Master Data lists announce a loading state while a filter, pager, search or status toggle is in flight, and a failed read keeps the shell, tabs and table headers with an in-shell notice and a Try again link instead of an empty-state row.
- Member, Event Type, Venue and Event create/edit use the same centered keyboard-operable record dialog with required markers, retained server validation, focus containment and restore, and a venue photo preview. Clicks on the dialog's own padding keep it open; only a backdrop click, Escape or Cancel dismisses it. Events open the same editor from the Events list and from the event dossier. Standalone form routes remain available.
- Events move through draft, published, completed, and cancelled states with capacity, deadline, and venue checks.
- Officers open an event dossier from Events to review metadata, confirmed capacity and paged status rosters, make explicit registration decisions, and confirm check-in and lifecycle changes in contextual dialogs.
- Failed dossier registration decisions retain the participant/registration context and entered reason for correction; review-queue failures stay within their original workflow.
- Registrations are pending, confirmed, waitlisted, or cancelled; seat allocation is transactional.
- Attendance is recorded by QR or manual lookup; certificates are issued one at a time from an eligibility review.
- The door scanner keeps scan and explicit manual check-in outcomes visible with participant identity and the original check-in time. Camera permission failure offers pass-code and manual alternatives; leaving camera mode cancels pending camera startup.
- Participation reports are per event and include attendance rate, no-shows, and course/year distribution.
- No hard deletes. Activity logs are append-only. Passwords are stored only as Argon2id hashes.
- The MVP is a light-theme web application; there is no dark mode, native app, or offline mode.
- The officer certificate ledger supports completed-event review, final decline reasons, individual issuance, and PDF viewing/download with the approved template and signatory. Reviews are real database rows and every issued document is retained in the private file store; each decision writes its own audit entry.
- My Certificates shows the member's pending, eligible, declined, issued and unavailable records with review reasons, independently paged history, and issued-only View/Download PDF. The history is the member's own confirmed registrations and their real certificate rows; a row offers a document only when its certificate is issued with a retained file. Every document read checks the current owner and returns the same not-found for another member's record or an unknown id.

## Brand Commitments

The product name is AttendTa, used by a University of Cebu student organization. The approved identity is the AttendTa logo with the University of Cebu institutional line. The application is a campus operations console: neutral ground, white panels, graphite ink, one cobalt action accent, and status carried by tinted labels. `DESIGN.md` at the repository root is the visual authority; `docs/design.md` is superseded as an anti-reference.

## Evidence on Hand

- `docs/UC-Project-FL.pdf` — approved function list and module ownership.
- `docs/AttendTa-System-Specification.md` — authoritative rule detail.
- `DESIGN.md` — visual authority for the 20 core screens; `docs/design.md` is the superseded anti-reference.
- `docs/architecture.md` and `_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md` — implementation baseline.
- `_bmad-output/initiative-attendta/prd-attendta/` — product requirements and decisions.
- Working ASP.NET Core MVC application with seeded local demo data; no production customer data or testimonials exist.

## Product Principles

1. Every decision leaves a record: approvals, rejections, edits, and attendance changes are auditable.
2. The participant record is the spine: one identity carries from request through registration, attendance, and certificate.
3. Server-side rules are the truth: capacity, deadlines, roles, and state guards are enforced on the server.
4. Officers need density and speed; members need clarity and confidence.
5. No dead ends: every empty, error, and blocked state explains what happened and what to do next.

## Accessibility & Inclusion

Target WCAG 2.1 AA: 4.5:1 normal-text contrast, visible keyboard focus, keyboard-operable controls and dialogs, labelled form fields, text alternatives for meaningful images, and usable layouts at 200% zoom. Status must never rely on color alone.
